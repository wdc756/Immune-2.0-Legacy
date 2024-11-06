using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Cell : MonoBehaviour
{
    /*
    Actually controls the cell gameobject based on cell type commands from other scripts
    This is also used as a reference for the VisualManger
     */



    /*
    0: not classified
    1: Civilian
    2: Immune Cell
    3: Bacteria
     */
    [Tooltip("Used to determine the type of cell, which tells this script which scripts to use")]
    public int type = 0;

    /*
    -n: wait for seconds(n)
    0: nothing or random walk if canMove
    1: Move to vector3
    2: Move to GameObject
    3: Follow GameObject (until told to stop)
    4: Deactivate
     */
    [SerializeField, Tooltip("List of tasks to be executed")]
    private List<int> tasks = new List<int>();
    [SerializeField, Tooltip("List of targets to move at")]
    private List<Vector3> movementTargets = new List<Vector3>();
    [SerializeField, Tooltip("list of gameobjects to follow")]
    private List<GameObject> followObjects = new List<GameObject>();
    [SerializeField, Tooltip("Used to broadcast what the cell is doing")]
    private int currentTask = 0;

    //used when the cell is waiting, and keeps track of passed time
    private float waitTimer = 0;
    public CellSmallMovement cellSmallMovement;

    [Tooltip("Determines if the cell has a CellMovement script")]
    public bool canMove = false;
    //used to determine when the cell has reached a target and is ready to move to the next one
    private bool isMoving = false;
    //reference to the CellMovement class
    public CellMovement cellMovement;
    //reference to the CellRandomWalk class
    public CellRandomWalk cellWalk;
    [Tooltip("Used to determine how often the cell will change to a new random adjacent position when random walking; smaller number is smaller chance, 0 = never change")]
    public float randomWalkDirectionChangeChance;
    [Tooltip("The distance from the current position that the new random one can be")]
    public float randomWalkChangeDistance;
    [Tooltip("Determines how often the cell will veer off path when following a GameObject; smaller number is smaller chance, 0 = never change")]
    public float followChangeChance;
    [Tooltip("The distance that the cell will veer off to when it \"looses\" the gameobject its following for a second")]
    public float followChangeDistance;
    [Tooltip("How long the cell can veer off for, in seconds")]
    public float followChangeMaxTime;
    //the actual time the cell will veer off for
    private float followChangeTime;
    //used for timer stuff for followChange logic
    private float followChangeTimer;

    void Start()
    {
        cellSmallMovement = gameObject.GetComponent<CellSmallMovement>();
        cellSmallMovement.enabled = true;

        if (canMove)
        {
            cellMovement = gameObject.GetComponent<CellMovement>();
            cellWalk = gameObject.GetComponent<CellRandomWalk>();
            cellSmallMovement.enabled = false;
        }

        NewTask(0);
    }

    //Used whenever a cell is activated from the object pool
    public void ActivateCell(Vector3 position, float maxVertical, float maxHorizontal)
    {
        transform.position = position;
        ClearTasks();

        SetBounds(maxVertical, maxHorizontal);

        StopMoving();
    }
    public void ActivateCell(Vector3 position)
    {
        transform.position = position;
        ClearTasks();

        StopMoving();
    }
    //Used when a cell is deactivated and put back in the object pool
    public void DeactivateCell()
    {
        currentTask = 0;
        tasks.Clear();
        movementTargets.Clear();
        followObjects.Clear();
        StopMoving();
        gameObject.SetActive(false);
    }
    //Sets the min/max bounds for random movement
    public void SetBounds(float maxVertical, float maxHorizontal)
    {
        if (cellWalk != null)
        {
            cellWalk.SetBounds(maxVertical, maxHorizontal);
        }
    }



    //Puts a new task into the list
    public void NewTask(int task)
    {
        tasks.Add(task);
        movementTargets.Add(Vector3.zero);
        followObjects.Add(null);
    }
    public void NewTask(int task, Vector3 position)
    {
        tasks.Add(task);
        movementTargets.Add(position);
        followObjects.Add(null);
    }
    public void NewTask(int task, GameObject target)
    {
        tasks.Add(task);
        movementTargets.Add(Vector3.zero);
        followObjects.Add(target);
    }
    //Resets the list and what the cell is currently doing
    public void ClearTasks()
    {
        tasks.Clear();
        movementTargets.Clear();
        followObjects.Clear();
        currentTask = 0;
        StopMoving();

        NewTask(0);
    }



    //This is still safe to have here because this script will only run when the cell gameobject is active in the scene
    void Update()
    {
        if (canMove)
        {
            //Update moving and update if we are moving or not; inverted because of CellMovement bool handling
            isMoving = !cellMovement.UpdateCellMovement();
        }

        HandleTasks();
    }



    private void HandleTasks()
    {
        // If the task is negative, wait for x seconds
        if (currentTask < 0)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= Mathf.Abs(currentTask))
            {
                currentTask = 0;
                cellSmallMovement.enabled = false;
            }
        }

        //is doing nothing, and has only one task, then do randomWalk updates
        if (currentTask == 0 && tasks.Count <= 1)
        {
            if (canMove)
            {
                HandleRandomWalk();
            }
        }

        if (currentTask == 1)
        {
            //if the cell has stopped moving, it has arrived
            if (!isMoving)
            {
                currentTask = 0;
            }
            else
            {
                HandleTargetMovement();
            }
        }

        if (currentTask == 2 || currentTask == 3)
        {
            HandleFollowMovement();
        }

        //if the cell is on standby, and there are tasks in the queue, then load the next task
        if (currentTask == 0 && tasks.Count > 1)
        {
            LoadNextTask();
        }
    }

    // Loads in the next task and sets all relevant variables
    public void LoadNextTask()
    {
        // Remove the completed task from the queue
        tasks.RemoveAt(0);
        movementTargets.RemoveAt(0);
        followObjects.RemoveAt(0);

        // Check if there are more tasks to load
        if (tasks.Count >= 1)
        {
            currentTask = tasks[0];

            // Set up the task based on its type
            if (currentTask == 1)
            {
                SetMove(movementTargets[0]);
            }
            else if (currentTask == 4)
            {
                DeactivateCell();
            }
            else if (currentTask < 0)
            {
                cellSmallMovement.enabled = true;
            }
        }
        else
        {
            // No more tasks, reset to idle task
            currentTask = 0;
            NewTask(0);
        }
    }


    //Handles target movement functions and variables
    private void HandleTargetMovement()
    {
        //if the cell has not veered off, then regular update, if it has, then follow until the timer reaches max time
        if (followChangeTimer == 0)
        {
            SetMove(movementTargets[0]);

            if (Random.Range(0f, 100f) < followChangeChance)
            {
                followChangeTimer = 0.001f;
                followChangeTime = Random.Range(followChangeMaxTime / 3, followChangeMaxTime);
                SetMove(GetAdjustedPosition(followChangeDistance));
            }
        }
        else
        {
            followChangeTimer += Time.deltaTime;

            //if the cell has veered off for the alotted time, then refocus
            if (followChangeTimer >= followChangeTime)
            {
                followChangeTimer = 0;
            }
        }
    }
    //Handles follow movement functions and variables, based on conditionals
    private void HandleFollowMovement()
    {
        if (followObjects[0] == null)
        {
            Debug.LogWarning("Cell " + gameObject.name + " attempted to follow a null gameobject");
            //return true;
        }
        else
        {
            //if the cell has not veered off, then regular update, if it has, then follow until the timer reaches max time
            if (followChangeTimer == 0)
            {
                SetMove(followObjects[0].transform.position);

                if (Random.Range(0f, 100f) < followChangeChance)
                {
                    followChangeTimer = 0.001f;
                    followChangeTime = Random.Range(followChangeMaxTime / 3, followChangeMaxTime);
                    SetMove(GetAdjustedPosition(followChangeDistance));
                }
            }
            else
            {
                followChangeTimer += Time.deltaTime;

                //if the cell has veered off for the alotted time, then refocus
                if (followChangeTimer >= followChangeTime)
                {
                    followChangeTimer = 0;
                }
            }

            ////if the cell is not moving(arrived), check what type of task
            //if (isMoving)
            //{
            //    return false;
            //}
            //else
            //{
            //    //if the cell is not in continuous follow mode, then return true
            //    if (currentTask != 3)
            //    {
            //        return true;
            //    }
            //    return false;
            //}
        }
    }
    //Handles random walk functions and variables, based on conditionals
    private void HandleRandomWalk()
    {
        //if the cell can move, is not currently moving
        if (canMove && !isMoving)
        {
            SetMove(GetRandomPosition());
        }

        //if the cell can move, is currently moving, then randomly choose a new postion sometimes
        if (canMove && isMoving)
        {
            //Only change on occasion, not every time
            if (Random.Range(0f, 100f) < randomWalkDirectionChangeChance)
            {
                SetMove(GetAdjustedPosition(randomWalkChangeDistance));
            }
        }
    }
    private void SetMove(Vector3 target)
    {
        if (canMove)
        {
            cellMovement.SetMovementTarget(target);
        }
    }
    //returns a new random position using the logic in CellRandomWalk
    public Vector3 GetRandomPosition()
    {
        Vector3 newPosition = transform.position;
        if (canMove)
        {
            newPosition = cellWalk.GetNewPosition();
        }
        return newPosition;
    }
    //returns a new adjusted postion using the logic in CellRandomWalk
    public Vector3 GetAdjustedPosition(float maxDistance)
    {
        Vector3 newPosition = transform.position;
        if (canMove)
        {
            newPosition = cellWalk.GetAdjustedPosition(newPosition, maxDistance);
        }
        return newPosition;
    }
    public void StopMoving()
    {
        if (canMove && cellMovement != null)
        {
            cellMovement.SlowDown();

            //if in continuous follow mode, switch to follow mode, else if in follow mode, then go to regular move, then if in regular mode stop moving completely
            if (currentTask == 3)
            {
                currentTask = 2;
            }
            else if (currentTask == 2)
            {
                currentTask = 1;
            }
            else if (currentTask == 1)
            {
                currentTask = 0;
                SetMove(GetAdjustedPosition(followChangeDistance));
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

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
    [Tooltip("The amount of time the cell can stay active for; used to help with cells persisting when they were supposed to die")]
    public float persistTime = 15f;
    //The actual time remaining until the cell gets deactivated
    private float persistTimeLeft;

    /*
    -n: wait for seconds(n)
    0: nothing or random walk if canMove
    1: Move to vector3
    2: Move to GameObject
    3: Follow GameObject (until told to stop)
    4: Follow GameObject (kill on arrive)
    5: Follow GameObject (eat on arrive)
    6: Alert (alarm signal)
    10: Deactivate (aptosis animation)
    11: Deactivate (setActive(false))

    tutorial numbers, don't use in gameplay
    12: Destroy self
    14: Destroy gameobject on arrive
    15: Destroy gameobject on swallow
     */
    [Header("Task Variables")]
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
    private CellSmallMovement cellSmallMovement;

    [Header("Movement Variables")]
    [Tooltip("Determines if the cell moves to targets or just waves around")]
    public bool canMove = false;
    //used to determine when the cell has reached a target and is ready to move to the next one
    private bool isMoving = false;
    [Tooltip("Used to determine when a cell will interact with surrounding cells in the simulation, or other objects")]
    public float interactRadius = 1;
    private CellMovement cellMovement;
    private CellRandomWalk cellWalk;
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

    //used to hold what spot a civilian cell is using
    public int cellSpot;

    [Header("Sprite Rotation Variables")]
    public float rotationSpeed;
    public float rotationOffset;



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
        SetBounds(maxVertical, maxHorizontal);
        Activate(position);
    }
    public void ActivateCell(Vector3 position)
    {
        Activate(position);
    }
    private void Activate(Vector3 position)
    {
        transform.position = position;
        ClearTasks();

        if (canMove) { StopMoving(); }
        if (cellSmallMovement != null) { cellSmallMovement.ResetMovement(); }

        SetRandomRotation();

        ResetPersist();
    }
    //Used when a cell is deactivated and put back in the object pool
    public void DeactivateCell()
    {
        //add if == 10 || == 11 for animations later
        currentTask = 0;
        ClearTasks();
        gameObject.SetActive(false);


        //we need to add logic to handle varable changes
        //perhaps keep one cell as a reference in the stack
    }
    //Sets the min/max bounds for random movement
    public void SetBounds(float maxVertical, float maxHorizontal)
    {
        if (cellWalk == null)
        {
            cellWalk = gameObject.GetComponent<CellRandomWalk>();
        }
        cellWalk.SetBounds(maxVertical, maxHorizontal);
    }



    //Used to reset the persist timer, or anytime a cell funciton is called by another script
    public void ResetPersistTime(float newTime)
    {
        persistTime = newTime;
        ResetPersist();
    }
    public void ResetPersistTime()
    {
        ResetPersist();
    }
    private void ResetPersist()
    {
        persistTimeLeft = persistTime;
    }


    //Puts a new task into the list
    public void NewTask(int task)
    {
        tasks.Add(task);
        movementTargets.Add(Vector3.zero);
        followObjects.Add(null);

        ResetPersist();
    }
    public void NewTask(int task, Vector3 position)
    {
        tasks.Add(task);
        movementTargets.Add(position);
        followObjects.Add(null);

        ResetPersist();
    }
    public void NewTask(int task, GameObject target)
    {
        tasks.Add(task);
        movementTargets.Add(Vector3.zero);
        followObjects.Add(target);

        ResetPersist();
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

        ResetPersist();
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

        if (type != 1 && cellMovement != null)
        {
            HandleRotation();
        }

        persistTimeLeft -= Time.deltaTime;
        if (persistTimeLeft < 0)
        {
            DeactivateCell();
        }
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

        if (currentTask == 2 || currentTask == 3 || currentTask == 4 || currentTask == 5 || currentTask == 14 || currentTask == 15)
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
            else if (currentTask == 10 || currentTask == 11)
            {
                DeactivateCell();
            }
            else if (currentTask == 12)
            {
                Destroy(gameObject);
            }
            else if (currentTask < 0)
            {
                cellSmallMovement.enabled = true;
                cellSmallMovement.ResetMovement();
            }

            //If there is only one task left, add a new one
            if (tasks.Count == 1 && currentTask != 0)
            {
                NewTask(0);
            }
        }
        else
        {
            // No more tasks, reset to idle task
            currentTask = 0;
            NewTask(0);
        }

        ResetPersist();
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
            currentTask = 0;
        }
        else
        {
            //if the cell has not veered off, then regular update, if it has, then follow until the timer reaches max time
            if (followChangeTimer == 0)
            {

                //If the distance is lower than the interactRadius, then add distance to the target to make the cell move faster
                if (Vector3.Distance(transform.position, followObjects[0].transform.position) > interactRadius * 1.2f)
                {
                    //Get the direction to the current target
                    Vector3 direction = followObjects[0].transform.position - transform.position;

                    direction *= cellMovement.frictionForce / 7f;
                    //Debug.Log(direction.magnitude);

                    Vector3 movePastTarget = followObjects[0].transform.position + direction;

                    SetMove(movePastTarget);
                }
                else
                {
                    SetMove(followObjects[0].transform.position);
                }

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

            //if in kill follow mode, then when the target object is reached, kill it
            if ((currentTask == 4 || currentTask == 5) && Vector3.Distance(gameObject.transform.position, followObjects[0].transform.position) < interactRadius)
            {
                KillEnemyCell();
            }
            if ((currentTask == 14 || currentTask == 15) && Vector3.Distance(gameObject.transform.position, followObjects[0].transform.position) < interactRadius)
            {
                DestroyEnemyCell();
            }

            //if in follow stop mode, then when the target is reached, reset task
            if (currentTask == 2 && Vector3.Distance(gameObject.transform.position, followObjects[0].transform.position) < interactRadius / 2f)
            {
                if (isMoving)
                {
                    cellMovement.SlowDown();
                }
                else
                {
                    currentTask = 0;
                }
            }
        }
    }
    private void KillEnemyCell()
    {
        //get the current kill target
        Cell cell = followObjects[0].GetComponent<Cell>();
        if (cell != null)
        {
            //clear tasks and tell it to either kill immediately or to be swallowed
            cell.ClearTasks();
            //if in swallow mode, tell enemy to be swallowed
            if (currentTask == 5)
            {
                if (cell.canMove)
                {
                    cell.NewTask(2, gameObject);
                    //CellMovement enemyMovement = cell.GetComponent<CellMovement>();
                    //enemyMovement.tolerance = 0.7f;
                    cell.followChangeChance = 0f;
                }
            }
            //kill
            cell.NewTask(10);
        }
        //tell this cell to move to the target, to make swallowing easier
        currentTask = 2;
    }
    private void DestroyEnemyCell()
    {
        //get the current kill target
        Cell cell = followObjects[0].GetComponent<Cell>();
        if (cell != null)
        {
            //clear tasks and tell it to either kill immediately or to be swallowed
            cell.ClearTasks();
            //if in swallow mode, tell enemy to be swallowed
            if (currentTask == 15)
            {
                if (cell.canMove)
                {
                    cell.NewTask(2, gameObject);
                    //CellMovement enemyMovement = cell.GetComponent<CellMovement>();
                    //enemyMovement.tolerance = 0.7f;
                    cell.followChangeChance = 0f;
                }
            }
            //kill
            cell.NewTask(12);
        }
        //tell this cell to move to the target, to make swallowing easier
        currentTask = 2;
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
            if (currentTask == 4 || currentTask == 5)
            {
                currentTask = 3;
            }
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



    private void HandleRotation()
    {
        //Get the spot the cell is moving towards
        Vector3 targetPosition = cellMovement.targetPosition;

        // Calculate the difference in position
        Vector3 direction = targetPosition - transform.position;

        // Calculate the angle in radians and convert to degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Apply the rotation to the Z-axis only
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);

        // Interpolate to the new rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
    private void SetRandomRotation()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
    }
}

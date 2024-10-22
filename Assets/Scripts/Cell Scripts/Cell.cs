using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
    0: nothing
    1: Moving
    3: Attacking (this will also involve movement)
    4: Healing
     */
    [SerializeField, Tooltip("List of tasks to be executed")]
    private List<int> tasks = new List<int>();
    private List<Vector3> movementTargets = new List<Vector3>();
    [SerializeField, Tooltip("Used to broadcast what the cell is doing")]
    private int currentTask = 0;

    [SerializeField, Tooltip("Determines if the cell has a CellMovement script")]
    private bool canMove = false;
    //reference to the CellMovement class
    private CellMovement cellMovement;

    [SerializeField, Tooltip("Determines if the cell can attack")]
    private bool canAttack = false;
    //Set if the cell can attack and has an ImmuneCell/Bacteria componet
    //private float attackSpeed = 0.0f;


    private void Start()
    {
        if (canMove)
        {
            cellMovement = gameObject.GetComponent<CellMovement>();
        }

        if (canAttack)
        {
            //assign attack values from Immune/Bacteria scripts

        }
    }
    void Update()
    {
        if (canMove)
        {
            cellMovement.HandleMovement();
        }
        //UpdateTasks();
    }

    //public void UpdateTasks()
    //{
    //    if (canMove && (currentTask == 1 || currentTask == 3))
    //    {
    //        //check if moving is done
    //        if (cellMovement.HandleMovement())
    //        {
    //            NextTask();
    //        }
    //    }
    //}
    ////cycles to the next task in the list
    //private void NextTask()
    //{
    //    if (tasks.Count > 0)
    //    {
    //        currentTask = tasks[0];
    //        tasks.RemoveAt(0);
    //    }

    //    NewTask();
    //}
    ////initializes and calls proper functions when the next task is loaded in
    //private void NewTask()
    //{
    //    switch (currentTask)
    //    {
    //        case 0:
    //            return;
    //        case 1:
                
    //    }
    //}

    //Used whenever a cell is activated from the object pool
    public void ActivateCell(Vector3 position)
    {
        transform.position = position;
        currentTask = 0;
        StopMoving();
    }
    //Used when a cell is deactivated and put back in the object pool
    public void DeactivateCell()
    {
        currentTask = 0;
        tasks.Clear();
        StopMoving();
    }



    public void SetMove(Vector3 target)
    {
        if (canMove)
        {
            cellMovement.UpdateTargetPos(target);
        }
    }
    public void StopMoving()
    {
        if (canMove)
        {
            cellMovement.StopMoving();
        }
    }
}

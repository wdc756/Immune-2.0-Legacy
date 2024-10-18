using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    /*
    Actually controls the cell gameobject based on cell type commands from other scripts
    This is also used as a reference for the VisualManger
     */

    /*
    0: default
    1: Civilian
    2: Immune Cell
    3: Bacteria
     */
    [Tooltip("Used to determine the type of cell, which tells this script which scripts to use")]
    public int type = 0;

    [Tooltip("Determines if the cell has a CellMovement script")]
    public bool canMove = false;
    //reference to the CellMovement class
    private CellMovement cellMovement;


    void Start()
    {
        if (canMove)
        {
            cellMovement = gameObject.GetComponent<CellMovement>();
        }
    }
    void Update()
    {
        
    }

    
}

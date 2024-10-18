using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellMovement : MonoBehaviour
{
    /*
    Handles the actual movement of cells
     */



    //determines if the cell should be speeding up
    private bool isAccelerating = false;
    [Tooltip("Determines the rate that the cell accelerates at")]
    public float acceleration = 0.1f;
    //determines if the cell should be slowing down
    private bool isDeccelerating = false;
    [Tooltip("Determines the rate that the cell deccelerates at")]
    public float deccelerate = 0.5f;
    //used by the movement functions to determine how fast the cell moves at any given point
    private float speed = 0.0f;
    [Tooltip("The maximum speed that the cell can move at")]
    public float maxSpeed = 1.0f;

    [Tooltip("Used by other scripts to set the next movement position")]
    public Vector3 targetPos;



    // Start is called before the first frame update
    void Start()
    {
        targetPos = transform.position;
    }



    //sets the new targetPos for the movement function
    public void UpdateTargetPos(Vector3 newTargetPos)
    {
        targetPos = newTargetPos;
        isAccelerating = true;
    }

    //Calls relevant movement functions and returns a bool for whether or not the cell has reached targetPos and has slowed down
    public bool HandleMovement()
    {
        Accelerate();

        Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPos) < 0.1)
        {
            isAccelerating = false;
            isDeccelerating = true;
        }
        
        if (Vector3.Distance(transform.position, targetPos) < 0.1 && !isDeccelerating)
        {
            return true;
        }
        return false;
    }
    //updates speed value
    private void Accelerate()
    {
        if (isAccelerating)
        {
            speed += acceleration;
        }
        else if (isDeccelerating)
        {
            speed -= deccelerate;
        }

        speed = Mathf.Clamp(speed, 0.0f, maxSpeed);
        
        if (speed == maxSpeed)
        {
            isAccelerating = false;
        }
        else if (speed <= 0.0f)
        {
            isDeccelerating = false;
        }
    }
}

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
    public float acceleration = 0.7f;
    //determines if the cell should be slowing down
    private bool isDeccelerating = false;
    [Tooltip("Determines the rate that the cell deccelerates at")]
    public float decelerate = 1.0f;
    //used by the movement functions to determine how fast the cell moves at any given point
    private float speed = 0.0f;
    [Tooltip("The maximum speed that the cell can move at")]
    public float maxSpeed = 7.0f;

    [Tooltip("Used by other scripts to set the next movement position")]
    public Vector3 targetPos;
    //used to determine the point that deceleration starts
    private float decelerateDistance;
    private Vector3 movementDirection;


    // Start is called before the first frame update
    void Start()
    {
        targetPos = transform.position;
    }



    //sets the new targetPos for the movement function
    public void UpdateTargetPos(Vector3 newTargetPos)
    {
        newTargetPos.z = gameObject.transform.position.z;
        targetPos = newTargetPos;
        isAccelerating = true;
        decelerateDistance = Vector3.Distance(gameObject.transform.position, targetPos) / 2.25f;
        movementDirection = (targetPos - transform.position).normalized;
    }
    //sets the current position as the target and tells the class to slow down
    public void StopMoving()
    {
        targetPos = gameObject.transform.position;
        isAccelerating = false;
        isDeccelerating = true;
    }

    //Calls relevant movement functions and returns a bool for whether or not the cell has reached targetPos and has slowed down
    public bool HandleMovement()
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPos);

        // If we are close enough to the target, start decelerating
        if (distanceToTarget < decelerateDistance)
        {
            isDeccelerating = true;
            isAccelerating = false;
        }

        Accelerate();  // Update speed based on acceleration/deceleration

        // Move the cell manually based on speed and direction
        transform.position += movementDirection * speed * Time.deltaTime;

        // If we are at the target, return true
        if (distanceToTarget < 0.001f && speed <= 0.01f)
        {
            return true;
        }

        return false;
    }

    private void Accelerate()
    {
        // If the cell is accelerating
        if (isAccelerating)
        {
            speed += acceleration * Time.deltaTime;
        }
        else if (isDeccelerating)
        {
            speed -= decelerate * Time.deltaTime;
        }

        // Ensure the speed stays within the limits
        speed = Mathf.Clamp(speed, 0.0f, maxSpeed);

        // Stop accelerating when max speed is reached
        if (speed == maxSpeed)
        {
            isAccelerating = false;
        }

        // Stop decelerating when speed reaches zero
        if (speed <= 0.0f)
        {
            isDeccelerating = false;
        }
    }
}

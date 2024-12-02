using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellMovement : MonoBehaviour
{
    /*
    Handles the actual movement of cells
     */

    [Tooltip("The amount of change in momentum per frame when accelerating")]
    public float accelerationForce = 0.1f;
    [Tooltip("The mass of the object, or it's resistance to acceleration")]
    public float mass = 0.1f;

    [Tooltip("The amount of change in momentum per frame when not accelerating")]
    public float frictionForce = 1.0f;

    [Tooltip("The maximum speed the object can reach")]
    public float maxSpeed = 1.0f;

    [Tooltip("Used to determine the stopping distance")]
    public float tolerance = 0.7f;

    //The direction the cell is moving in
    public Vector3 momentum;
    public bool isSlowingDown = false;
    //The target position the cell is trying to reach
    public Vector3 targetPosition;
    //The direction of the target from where the cell currently is
    private Vector3 targetDirection;

    private void Start()
    {
        StopMoving();
    }



    public void SetMovementTarget(Vector3 position)
    {
        position.z = gameObject.transform.position.z;
        targetPosition = position;
    }
    public void SlowDown()
    {
        if (momentum.magnitude > 0.01f)
        {
            momentum *= 0.99f;
        }
        else
        {
            StopMoving();
        }
    }
    public void StopMoving()
    {
        momentum = Vector3.zero;
        targetPosition = transform.position;
        targetDirection = Vector3.zero;
    }

    //Must be called every frame to actually move the cell, and returns if it has reached the target
    public bool UpdateCellMovement()
    {
        //used to return a value at the end of the script
        bool hasArrived = false;

        targetDirection = CalculateMovementDirection();

        // if we are close and not moving fast, then we should use SlowDown() to dampen momentum instead of just using friction force because friction
        // is unreliable at best, this also helps us avoid orbiting and vibrating when close to the target
        if (Vector3.Distance(gameObject.transform.position, targetPosition) <= 2f * tolerance && momentum.magnitude < 1f)
        {
            isSlowingDown = true;
            SlowDown();
            hasArrived = true;
        }
        else
        {
            isSlowingDown = false;
            if (ShouldAccelerate())
            {
                ApplyForce(accelerationForce);
            }
            else
            {
                ApplyForce(-frictionForce);

                //not sure why this being here helps, it just does
                momentum *= 0.95f;
            }
        }

        if (momentum.magnitude > 0)
        {
            gameObject.transform.position += momentum * Time.deltaTime;
        }

        return hasArrived;
    }

    private Vector3 CalculateMovementDirection()
    {
        //By subtracting our current pos from the target, we get the direciton we need to move in, and we normalize it to get a vector with a magnitude of 1
        return (targetPosition - gameObject.transform.position).normalized;
    }

    private bool ShouldAccelerate()
    {
        // Calculate stopping distance based on momentum and friction
        // dx = -v^2 / 2f
        float stoppingDistance = (momentum.magnitude * momentum.magnitude) / (2 * frictionForce);

        // Calculate current distance to target
        float currentDistance = Vector3.Distance(gameObject.transform.position, targetPosition);

        // Only decelerate if moving towards the target and within stopping range; using a dot product here because it calculates the angle between vectors
        // so if the angle is positive, then it means they both point toward the same direction, and the opposite if negative
        bool movingTowardsTarget = Vector3.Dot(momentum.normalized, targetDirection) > 0;

        // if we are not moving towards the target or we are far from the target accelerate
        if (!movingTowardsTarget || currentDistance > stoppingDistance + tolerance)
        {
            return true;
        }

        return false;
    }

    private void ApplyForce(float force)
    {
        //Calculates the amount to change the velocity
        // a = F / m   targetDirection is to make sure it applies in the correct direction
        Vector3 acceleration = targetDirection * (force / CalculateMass());

        //Calculates the new momentum by adding the acceleration with respect to time
        // v = v[i] + at
        momentum = momentum + (acceleration * Time.deltaTime);

        //keep the momentum(speed) from getting to big
        momentum = Vector3.ClampMagnitude(momentum, maxSpeed);
        //stop any movement on the z axis
        momentum.z = 0.0f;


        // I'm not sure why this works, but it keeps the cell from vibrating endlessly as it approaches the target
        if (force < 0 && Mathf.Abs(momentum.magnitude) < 0.1f)
        {
            SlowDown();
        }
    }

    //Used to make the cell have more resistance when far from the object and little resistence when close; helps to stop orbiting
    public float CalculateMass()
    {
        return Mathf.Clamp(Vector3.Distance(gameObject.transform.position, targetPosition) / 7, 0.001f, mass);
    }
}

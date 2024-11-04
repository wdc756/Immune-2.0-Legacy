using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellRandomWalk : MonoBehaviour
{
    /*
     Will return random locations for the cells to move to within the visual scene, creating a sort of patrol behavior
     */

    private float verticalMax = 10f;
    private float horizontalMax = 16f;

    //used to set the maximum bounds of the random function
    public void SetBounds(float vMax, float hMax)
    {
        verticalMax = Mathf.Abs(vMax);
        horizontalMax = Mathf.Abs(hMax);
    }

    //returns a completely random position inside the bounds for the cell to move to
    public Vector3 GetNewPosition()
    {
        Vector3 newPosition = new Vector3();

        newPosition.x = Random.Range(-horizontalMax, horizontalMax);
        newPosition.y = Random.Range(-verticalMax, verticalMax);

        return newPosition;
    }

    //returns a position nearby to the current cell
    public Vector3 GetAdjustedPosition(Vector3 currentPosition, float maxDistance)
    {
        //calculates a new distance the cell should move to
        float newDistance = Random.Range(maxDistance / 3, maxDistance);

        Vector3 newPosition = new Vector3();

        //Calculates a new position to move to by adding a random amount of the distance to the current positon then clamping the result to min/max bounds
        newPosition.x = Mathf.Clamp(currentPosition.x + Random.Range(-newDistance, newDistance), -horizontalMax, horizontalMax);
        newPosition.y = Mathf.Clamp(currentPosition.y + Random.Range(-newDistance, newDistance), -verticalMax, verticalMax);

        return newPosition;
    }
}

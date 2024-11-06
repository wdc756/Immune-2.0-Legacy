using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CellSmallMovement : MonoBehaviour
{
    /*
     This is very similar to CellRandomWalk, however this is for non-moving cells, to create a stort of waving or vibrating motion
     */

    //Used as a reference for where the cell starts from, so it doesn't move away
    public Vector3 startPosition;
    //used as the target position the cell should move to
    private Vector3 targetPosition;

    [Tooltip("The maximum distance the cell can be from the starting position")]
    public float maxDistance = 0.2f;
    [Tooltip("The time it will take the cell to move to it's new position")]
    public float movementTime = 0.5f;

    private void Start()
    {
        Cell cell = GetComponent<Cell>();
        startPosition = gameObject.transform.position;
        targetPosition = GetRandomPosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(gameObject.transform.position, targetPosition) < 0.1f)
        {
            targetPosition = GetRandomPosition();
        }
        else
        {
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, targetPosition, movementTime * Time.deltaTime);
        }
    }

    private Vector3 GetRandomPosition()
    {
        Vector3 newPos = new Vector3();

        newPos.z = 0;
        newPos.x = Random.Range(startPosition.x - maxDistance, startPosition.x + maxDistance);
        newPos.y = Random.Range(startPosition.y - maxDistance, startPosition.y + maxDistance);

        return newPos;
    }
}

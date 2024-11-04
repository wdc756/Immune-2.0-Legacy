//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class MovementTest : MonoBehaviour
//{
//    public Camera mainCamera; // Reference to the main camera
//    public Cell cell;
//    public GameObject bacteria;
//    public bool followMouse = true;

//    public Vector3 GetMouseWorldPosition()
//    {
//        // Get the mouse position in screen coordinates (pixels)
//        Vector3 mouseScreenPosition = Input.mousePosition;

//        // Set the z-coordinate to the camera's near plane distance for perspective or orthographic positioning
//        mouseScreenPosition.z = Mathf.Abs(mainCamera.transform.position.z);

//        // Convert the screen point to a world point
//        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
//        return mouseWorldPosition;
//    }

//    void Update()
//    {
//        if (Input.GetMouseButton(0))
//        {
//            cell.SetMove(GetMouseWorldPosition());
//        }
//        if (Input.GetMouseButtonDown(1))
//        {
//            cell.StopMoving();
//            if (followMouse)
//            {
//                followMouse = false;
//            }
//            else
//            {
//                followMouse = true;
//            }
//        }

//        if (!followMouse)
//        {
//            cell.SetMove(bacteria.transform.position);
//        }
//    }
//}

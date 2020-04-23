using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;
    public float zoomSpeed = 1;
    private void LateUpdate()
    {
        //Calculate position to travel to
        Vector3 desiredPositon = target.position + offset;

        //Linearly interpolate between point and move camera to that position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPositon, smoothSpeed);
        transform.position = smoothedPosition;

        //If Q key is pressed increase view size to "zoom out"
        //If E key is pressed decrease view size to "zoom in"
        if (Input.GetKey(KeyCode.Q))
        {
            GetComponent<Camera>().orthographicSize += zoomSpeed;
        }
        if (Input.GetKey(KeyCode.E))
        {
            if (GetComponent<Camera>().orthographicSize-zoomSpeed >= 1)
            {
                GetComponent<Camera>().orthographicSize -= zoomSpeed;
            }
        }
    }
}

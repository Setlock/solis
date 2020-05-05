using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
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
        Vector2 lerp = Vector2.Lerp(transform.position, desiredPositon, smoothSpeed);
        Vector3 smoothedPosition = new Vector3(desiredPositon.x, desiredPositon.y, transform.position.z);
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
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}

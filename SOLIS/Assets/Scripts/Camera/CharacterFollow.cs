using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class CharacterFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;
    public float zoomSpeed = 1;
    new Camera camera;
    private void Start()
    {
        camera = GetComponent<Camera>();
    }
    private void LateUpdate()
    {
        Vector3 targetPosition = target.position + offset;

        Vector3 finalPosition = new Vector3(targetPosition.x, targetPosition.y, camera.transform.position.z);

        camera.transform.position = finalPosition;

        //If Q key is pressed increase view size to "zoom out"
        //If E key is pressed decrease view size to "zoom in"
        if (Input.GetKey(KeyCode.Q))
        {
            camera.orthographicSize += zoomSpeed;
        }
        if (Input.GetKey(KeyCode.E))
        {
            if (camera.orthographicSize-zoomSpeed >= 1)
            {
                camera.orthographicSize -= zoomSpeed;
            }
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
    private Vector3 PixelPerfectClamp(Vector3 vector, float pixelsPerUnit)
    {
        Vector3 vectorInPixels = new Vector3(Mathf.RoundToInt(vector.x * pixelsPerUnit)/pixelsPerUnit, Mathf.RoundToInt(vector.y * pixelsPerUnit)/pixelsPerUnit, vector.z);
        return vectorInPixels;
    }
}

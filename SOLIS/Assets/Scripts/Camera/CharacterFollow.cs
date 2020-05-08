using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class CharacterFollow : MonoBehaviour
{
    public Transform target;
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

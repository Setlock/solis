using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWater : MonoBehaviour
{
    public Camera cam;
    Vector2 camStartPos;
    Vector2 camCurrentPos;
    private void Start()
    {
        camStartPos = cam.transform.position;
    }
    void Update()
    {
        camCurrentPos = cam.transform.position;
        Vector2 offset = camCurrentPos - camStartPos;
        GetComponent<SpriteRenderer>().material.SetVector("_PosOffset", offset);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateLiquid : MonoBehaviour
{
    public GameObject liquid;
    void Update()
    {
        Camera camera = GetComponent<Camera>();
        float x = camera.aspect * 2f * camera.orthographicSize+10;
        float y = 2f * camera.orthographicSize+10;
        Vector2 size = new Vector2(x, y);
        liquid.GetComponent<SpriteRenderer>().size = size;
    }
}

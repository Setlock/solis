using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BushEntity : Entity
{
    public Color color;
    private void Start()
    {
        SetColor();
    }
    void SetColor()
    {
        GetComponent<SpriteRenderer>().color = color;
    }
}

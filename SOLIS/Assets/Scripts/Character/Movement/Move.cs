using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public float speed = 2;
    void Update()
    {
        //Get direction to move in based on input
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
        moveDirection *= speed;

        //Add this direction to current position
        this.transform.position += moveDirection;
    }
}

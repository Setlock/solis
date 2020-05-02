using UnityEngine;

public class Move : MonoBehaviour
{
    public float speed = 2;
    void FixedUpdate()
    {
        //Get direction to move in based on input
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
        moveDirection *= speed;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector2 pos = new Vector2(rb.position.x+moveDirection.x*Time.deltaTime,rb.position.y+moveDirection.y*Time.deltaTime);
        rb.MovePosition(pos);
    }
}

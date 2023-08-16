using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed;
    private Vector2 playerDirection;
    private Rigidbody2D playerRb;

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        playerDirection = new Vector2(horizontal, vertical).normalized;
    }

    private void FixedUpdate()
    {
        playerRb.velocity = new Vector2(playerDirection.x * speed, playerDirection.y * speed);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;

    private Rigidbody2D enemyRb;
    private PlayerAwareness playerAwarenessController;
    private Vector2 targetDirection;
    // Start is called before the first frame update
    private void Awake()
    {
        enemyRb = GetComponent<Rigidbody2D>();
        playerAwarenessController = GetComponent<PlayerAwareness>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        UpdateTargetDirection();
        RotateTowardsTarget();
        SetVelocity();
        }

    private void UpdateTargetDirection()
    {
        if (playerAwarenessController.AwareOfPlayer)
        {
            targetDirection = playerAwarenessController.DirectionToPlayer;
        }
        else
        {
            targetDirection = Vector2.zero;
        }
    }

    private void RotateTowardsTarget()
    {
        if(targetDirection == Vector2.zero)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(transform.forward, targetDirection);
        Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        enemyRb.SetRotation(rotation);
    }

    private void SetVelocity()
    {
        if(targetDirection == Vector2.zero)
        {
            enemyRb.velocity = Vector2.zero;
        }
        else
        {
            enemyRb.velocity = transform.up * speed;
        }
    }
}

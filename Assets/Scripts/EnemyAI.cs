using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState
    {
        Chase,
        Flee
    }

    EnemyState _currentState = EnemyState.Chase;

    public Transform target;

    public float speed = 200f;
    public float nextWaypointDistance = 0.1f;

    Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;

    Seeker seeker;
    Rigidbody2D rb;

    private bool borderCollide;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        InvokeRepeating("UpdatePath", 0f, 0.1f);
    }

    void FixedUpdate()
    {
        switch (_currentState)
        {
            case EnemyState.Chase:
                chasePlayer();
                break;
            case EnemyState.Flee:
                fleePlayer();
                break;
            default:
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            if(_currentState == EnemyState.Chase)
            {
                _currentState = EnemyState.Flee;
            }
            else if(_currentState == EnemyState.Flee)
            {
                _currentState = EnemyState.Chase;
            }
        }
    }

    void UpdatePath()
    {
        Debug.Log(-target.position);
        if (_currentState == EnemyState.Chase)
        {
            if (seeker.IsDone()) seeker.StartPath(rb.position, target.position, OnPathComplete);

        }
        else
        {
            // This is what controls the destination when the enemy flees the player
            if (seeker.IsDone()) seeker.StartPath(rb.position, -target.position, OnPathComplete);
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    void chasePlayer()
    {
        if (path == null) return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else reachedEndOfPath = false;

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = speed * Time.deltaTime * direction;

        rb.AddForce(force);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance) currentWaypoint++;
    }

    void fleePlayer()
    {
        if (path == null) return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else reachedEndOfPath = false;

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = speed * Time.deltaTime * direction;

        rb.AddForce(force);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance) currentWaypoint++;
    }
}

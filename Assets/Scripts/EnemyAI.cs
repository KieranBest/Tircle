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
        if (seeker.IsDone()) seeker.StartPath(rb.position, target.position, OnPathComplete);
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

        Vector2 fleeDestination = fleePlayerDestination(target, borderCollide);

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - fleeDestination).normalized;
        Vector2 force = speed * Time.deltaTime * direction;

        rb.AddForce(force);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance) currentWaypoint++;
    }

    static Vector2 fleePlayerDestination(Transform target, bool borderCollide)
    {
        int xRandom = Random.Range(0, 100);
        int yRandom = Random.Range(0, 100);
        Vector2 destination;
        if (target.position.x < -2.5)
        {
            destination.x = 6;
        }
        else if(target.position.x > 2.5)
        {
            destination.x = -6;
        }
        else
        {
            if(xRandom < 50)
            {
                destination.x = 6;
            }
            else
            {
                destination.x = -6;
            }
        }

        if (target.position.y < -2.5)
        {
            destination.y = 3;
        }
        else if (target.position.x > 2.5)
        {
            destination.y = -3;
        }
        else
        {
            if (yRandom < 50)
            {
                destination.y = 3;
            }
            else
            {
                destination.y = -3;
            }
        }

        if (borderCollide)
        {
            if(destination.x > -8)
            {
                destination.x = -7;
            }
            else
            {
                destination.x = 7;
            }

            if (destination.y > -4)
            {
                destination.x = -4;
            }
            else
            {
                destination.x = 4;
            }
        }


        Debug.Log(destination);


        return destination;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Border")
        {
            borderCollide = true;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Border")
        {
            borderCollide = false;
        }
    }
}

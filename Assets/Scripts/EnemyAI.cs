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


    List<Vector2> inaccessibleTargets = new List<Vector2> { 
        new Vector2(0,0),
        new Vector2(1,0),
        new Vector2(0,1),
        new Vector2(1,1),
        new Vector2(-1,0),
        new Vector2(0,-1),
        new Vector2(-1,-1),
        new Vector2(0,4),
        new Vector2(0,-4),
        new Vector2(-3,2),
        new Vector2(-3,1),
        new Vector2(-3,0),
        new Vector2(-3,-1),
        new Vector2(-3,-2),
        new Vector2(3,2),
        new Vector2(3,1),
        new Vector2(3,0),
        new Vector2(3,-1),
        new Vector2(3,-2),
        new Vector2(-4,2),
        new Vector2(-4,1),
        new Vector2(-4,0),
        new Vector2(-4,-1),
        new Vector2(-4,-2),
        new Vector2(4,2),
        new Vector2(4,1),
        new Vector2(4,0),
        new Vector2(4,-1),
        new Vector2(4,-2),
    };
    private float Threshold = 5f;
    private float xTarget;
    private float yTarget;

    Seeker seeker;
    Rigidbody2D rb;

    private Vector2 targetPosition;
    private Vector2 oldTarget;

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
                if(Vector2.Distance(rb.position, targetPosition) <= Threshold) {
                    oldTarget = 
                    targetPosition = new Vector2(Random.Range(-8, 8), Random.Range(-4, 4));
                    while (inaccessibleTargets.Contains(targetPosition)) targetPosition = new Vector2(Random.Range(-8, 8), Random.Range(-4, 4));
                }
                if(Vector2.Distance(rb.position, target.position) <= Threshold){
                    xTarget = rb.position.x + (3 * (rb.position.x - target.position.x));
                    yTarget = rb.position.y + (3 * (rb.position.y - target.position.y));

                    // if reaches a corner redirect.

                    targetPosition = new Vector2(xTarget, yTarget);
                }
                fleePlayer();
                break;
            default:
                break;
        }
        Debug.Log(targetPosition);


    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            if(_currentState == EnemyState.Chase)
            {
                targetPosition = new Vector2(Random.Range(-8, 8), Random.Range(-4, 4));
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
        if (_currentState == EnemyState.Chase)
        {
            if (seeker.IsDone()) seeker.StartPath(rb.position, target.position, OnPathComplete);

        }
        else
        {
            // This is what controls the destination when the enemy flees the player
            if (seeker.IsDone())
            {
                if(rb.position == targetPosition)
                {
                    targetPosition = new Vector2(Random.Range(-8, 8), Random.Range(-4, 4));
                }
                seeker.StartPath(rb.position, targetPosition, OnPathComplete);
            }
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

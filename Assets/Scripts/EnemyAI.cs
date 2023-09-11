using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    public Transform target;
    Seeker seeker;
    Rigidbody2D rb;
    Path path;
    
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;
    private float Threshold = 5f;
    private float xTarget;
    private float yTarget;
    private float angledDistance;
    private Vector2 targetPosition;
    private bool cornerTargetModified;

    public float speed = 200f;
    public float nextWaypointDistance = 0.1f;

    public enum EnemyState
    {
        Chase,
        Flee
    }
    EnemyState _currentState = EnemyState.Chase;

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

    List<Vector2> maxCorner = new List<Vector2>
    {
        new Vector2(-9,-4),
        new Vector2(-9,4),
        new Vector2(9,-4),
        new Vector2(9,4)
    };

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
                enemyMovement();
                break;
            case EnemyState.Flee:
                // Distance between enemy and enemy's target destination
                if(Vector2.Distance(rb.position, targetPosition) <= (Threshold / 10)) {
                    targetPosition = new Vector2(Random.Range(-9, 9), Random.Range(-4, 4));
                    while (inaccessibleTargets.Contains(targetPosition)) targetPosition = new Vector2(Random.Range(-9, 9), Random.Range(-4, 4));
                    //Debug.Log("target " + targetPosition);
                }
                // Distance between enemy and players location
                if(Vector2.Distance(rb.position, target.position) <= (Threshold/3) && !cornerTargetModified)
                {
                    xTarget = (float)Clamp(System.Math.Round(rb.position.x + (3 * (rb.position.x - target.position.x))),-9,9);
                    yTarget = (float)Clamp(System.Math.Round(rb.position.y + (3 * (rb.position.y - target.position.y))),-4,4);
                    targetPosition = new Vector2(xTarget, yTarget);
                    //Debug.Log("Close " + targetPosition);
                }
                // Distance between enemy and corner of map
                foreach (Vector2 corner in maxCorner)
                {
                    if (Vector2.Distance(rb.position, corner) <= (Threshold / 5) && !cornerTargetModified)
                    {
                        cornerTargetModified = true;
                        angledDistance = Mathf.Rad2Deg * (Mathf.Atan2(rb.position.y - target.position.y, rb.position.x - target.position.x));
                        if (angledDistance <= -135 && angledDistance > -180)
                        {
                            yTarget = Clamp(yTarget + 3, -4, 4);
                        }
                        if (angledDistance <= -90 && angledDistance > -135)
                        {
                            xTarget = Clamp(xTarget + 3, -9, 9);
                        }
                        if (angledDistance <= -45 && angledDistance > -90)
                        {
                            xTarget = Clamp(xTarget - 3, -9, 9);
                        }
                        if (angledDistance <= 0 && angledDistance > -45)
                        {
                            yTarget = Clamp(yTarget + 3, -4, 4);
                        }
                        if (angledDistance <= 45 && angledDistance > 0)
                        {
                            yTarget = Clamp(yTarget - 3, -4, 4);
                        }
                        if (angledDistance <= 90 && angledDistance > 45)
                        {
                            xTarget = Clamp(xTarget - 3, -9, 9);
                        }
                        if (angledDistance <= 135 && angledDistance > 90)
                        {
                            xTarget = Clamp(xTarget + 3, -9, 9);
                        }
                        if (angledDistance <= 180 && angledDistance > 135)
                        {
                            yTarget = Clamp(yTarget - 3, -4, 4);
                        }
                        targetPosition = new Vector2(xTarget, yTarget);
                        //Debug.Log("corner (" + xTarget + "," + yTarget + ")");
                    }
                    if (Vector2.Distance(rb.position, targetPosition) <= (Threshold) && cornerTargetModified)
                    {
                        cornerTargetModified = false;
                    }
                    Debug.Log(cornerTargetModified);
                }
                enemyMovement();
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

    void enemyMovement()
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

    public static T Clamp<T>(T value, T min, T max) where T : System.IComparable<T>
    {
        if (value.CompareTo(min) < 0) return min;
        if (value.CompareTo(max) > 0) return max;
        return value;
    }
}

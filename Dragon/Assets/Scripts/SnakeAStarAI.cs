using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class SnakeAStarAI : MonoBehaviour
{

    public Transform target;
    public int nextWaypointDistance = 1;

    Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;

    private Seeker seeker;
    private SnakeAIController controller;

    Vector3 start;

    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();
        controller = GetComponent<SnakeAIController>();
        start = new Vector3(transform.position.x, transform.position.y, transform.position.z); //for offsets if desired
        InvokeRepeating("UpdatePath", 0f, 0.0025f);
        InvokeRepeating("UpdateBrain", 0f, 0.000025f);
    }

    void UpdatePath()
    {
        do
        {
            target = GameObject.FindWithTag("Food").transform;
        } while (target == null);

        if (seeker.IsDone()) seeker.StartPath(start, target.position, OnPathComplete);
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0; // Reset the waypoint so we start to move towards the first point in the path
        }
    }

    // Update is called once per frame
    void UpdateBrain()
    {
        //UpdatePath();
        if (path == null) return; // If we don't have a path yet do nothing
        start = new Vector3(transform.position.x, transform.position.y, transform.position.z); //for offsets if desired

        // The distance to the next waypoint in the path
        float distanceToWaypoint;
        while (true)
        {
            // If you want maximum performance you can check the squared distance instead to get rid of a
            // square root calculation. But that is outside the scope of this tutorial.
            distanceToWaypoint = Vector2.Distance(start, path.vectorPath[currentWaypoint]);
            if (distanceToWaypoint < nextWaypointDistance)
            {
                // Check if there is another waypoint or if we have reached the end of the path
                if (currentWaypoint + 1 < path.vectorPath.Count)
                {
                    currentWaypoint++;
                }
                else
                {
                    // Set a status variable to indicate that the agent has reached the end of the path.
                    // You can use this to trigger some special code if your game requires that.
                    reachedEndOfPath = true;
                    break;
                }
            }
            else
            {
                break;
            }
        }

        // Direction to the next waypoint
        Vector2 dir = (path.vectorPath[currentWaypoint] - start).normalized;
        movementSignal(dir);
        
    }

    void movementSignal(Vector2 direction)
    {
        Debug.Log(direction);
        float x = direction.x;
        float y = direction.y;

        // Move in the y direction
        if (Mathf.Abs(y) > Mathf.Abs(x))
        {
            MoveY(direction, y);
        }
        // Move in the x direction
        else if (Mathf.Abs(x) > Mathf.Abs(y))
        {
            MoveX(direction, x);
        }
        else
        {
            updateBools(false, false, false, false);
        }

    }

    void MoveX(Vector2 direction, float x)
    {
        if (x > 0) updateBools(false, false, true, false); //right
        else updateBools(false, false, false, true); //left
    }

    void MoveY(Vector2 direction, float y)
    {
        if (y > 0) updateBools(true, false, false, false); //up
        else updateBools(false, true, false, false); //down
    }

    void updateBools(bool up, bool down, bool right, bool left)
    {
        controller.isUp = up;
        controller.isDown = down;
        controller.isRight = right;
        controller.isLeft = left;
    }
}

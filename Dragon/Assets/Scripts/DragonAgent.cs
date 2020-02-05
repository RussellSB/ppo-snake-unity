using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class DragonAgent : Agent
{
    public Vector2Int initPos;
    public Vector2 globInitPos;

    public Transform topBorder;
    public Transform bottomBorder;
    public Transform leftBorder;
    public Transform rightBorder;

    public GameObject foodHandler;
    private GameObject food;
    public GameObject tailPrefab;

    public Vector2Int gridPosition;
    private Vector2 globalGridPos;
    public Vector2Int gridDirection;
    public Vector2 targetPos;

    List<Vector2> tail;
    List<int> tailRotation;
    List<Vector2> snakesize;

    public bool dead;
    private int snakebodysize;
    private int headRotationCode;
    private int headRotationCode_PREV;

    private float Timer;
    private float MaxTimer;
    
    public List<GameObject> bodyParts;

    public bool observeVectors = true;
    public bool vectorWalls = true;
    public bool observeRays = false;
    public bool observeRaysOnly = false;

    /*****************************************************/
    /* 0 -----> Up
     * 1 -----> Down
     * 2 -----> Right
     * 3 -----> Left
     ******************************************************/

    //TODO: Check significance of local position for borders

    void Start()
    {
        initPos = new Vector2Int((int)transform.localPosition.x, (int)transform.localPosition.y);
        globInitPos = new Vector2(transform.position.x, transform.position.y);
        SnakeInit();
        bodyParts = new List<GameObject>();
    }

    private void SnakeInit()
    {
        int x = (int)Random.Range(leftBorder.position.x + 1, rightBorder.position.x - 1);
        int y = (int)Random.Range(bottomBorder.position.y + 1, topBorder.position.y - 1);

        gridPosition = initPos; //new Vector2Int(x, y);
        globalGridPos = globInitPos;
        gridDirection = new Vector2Int(0, -1);

        MaxTimer = 0.06f; // 0.07f //0.0025f
        Timer = MaxTimer;

        tail = new List<Vector2>();
        tailRotation = new List<int>();
        snakebodysize = 0;

        snakesize = GetFullSnake();
        refreshBody();
        newFood();
        
        dead = false;
    }

    void newFood()
    {
        if (food) GameObject.Destroy(food);
        food = foodHandler.GetComponent<FoodML>().SpawnFood(snakesize);
        food.transform.parent = transform.parent;
        targetPos = food.transform.localPosition; // locates new flame
    }

    void FixedUpdate()
    {
        if (dead) return;

        Movement();
        CheckWithinBorders();
    }

    void CheckWithinBorders()
    {
        if (gridPosition.x > rightBorder.transform.localPosition.x || gridPosition.x < leftBorder.transform.localPosition.x)
        {
            dead = true;
            Done();
        }

        if (gridPosition.y > topBorder.transform.localPosition.y || gridPosition.y < bottomBorder.transform.localPosition.y)
        {
            dead = true;
            Done();
        }
    }

    private float GetAngleFromVector(Vector2Int dir)
    {
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0)
        {
            n += 360;
        }

        return n;
    }

    private void Movement()
    {
        Timer += Time.deltaTime;
        if (Timer >= MaxTimer)
        {
            refreshBody();
            Timer -= MaxTimer;
            tail.Insert(0, globalGridPos); // gridPosition

            headRotationCode_PREV = headRotationCode;
            headRotationCode = calcCurrHeadRotationCode();

            if (headRotationCode != headRotationCode_PREV)
            {
                tailRotation.Insert(0, calcTurnTypeCode());
            }
            else
            {
                tailRotation.Insert(0, headRotationCode);
            }

            gridPosition += gridDirection;
            globalGridPos += gridDirection;

            if (tail.Count > snakebodysize)
            {
                tail.RemoveAt(tail.Count - 1);
                tailRotation.RemoveAt(tailRotation.Count - 1);
            }
            
            for (int i = 0; i < tail.Count; i++)
            {
                Vector2 snakePosition = tail[i];
                int rotation = tailRotation[i];
                Vector3 p = new Vector3(snakePosition.x, snakePosition.y);
                GameObject g = (GameObject)Instantiate(tailPrefab, p, Quaternion.identity);
                g.transform.parent = transform.parent;

                GameObject childStraight = g.transform.Find("Straight").gameObject;
                GameObject childCorner1 = g.transform.Find("Corner1").gameObject;
                GameObject childCorner2 = g.transform.Find("Corner2").gameObject;
                GameObject childCorner3 = g.transform.Find("Corner3").gameObject;
                GameObject childCorner4 = g.transform.Find("Corner4").gameObject;

                switch (rotation)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        g.GetComponent<Transform>().Rotate(Vector3.forward * 90);
                        break;
                    case 3:
                        g.GetComponent<Transform>().Rotate(Vector3.forward * -90);
                        break;
                    case 4:
                        childStraight.SetActive(false);
                        childCorner3.SetActive(true);
                        break;
                    case 5:
                        childStraight.SetActive(false);
                        childCorner4.SetActive(true);
                        break;
                    case 6:
                        childStraight.SetActive(false);
                        childCorner1.SetActive(true);
                        break;
                    case 7:
                        childStraight.SetActive(false);
                        childCorner2.SetActive(true);
                        break;
                    case 8:
                        childStraight.SetActive(false);
                        childCorner2.SetActive(true);
                        break;
                    case 9:
                        childStraight.SetActive(false);
                        childCorner4.SetActive(true);
                        break;
                    case 10:
                        childStraight.SetActive(false);
                        childCorner1.SetActive(true);
                        break;
                    case 11:
                        childStraight.SetActive(false);
                        childCorner3.SetActive(true);
                        break;
                }
                
                bodyParts.Add(g); // Fills the array
            }
            


            transform.localPosition = new Vector3(gridPosition.x, gridPosition.y);
            transform.eulerAngles = new Vector3(0, 0, GetAngleFromVector(gridDirection) - 270);

            //if (!canRotate) canRotate = true;
            RequestDecision();
            //AddReward(-0.01f); // Negative reward with every movement
        }
    }

    int calcCurrHeadRotationCode()
    {
        if (gridDirection.x == 0 && gridDirection.y == 1) return 0;
        if (gridDirection.x == 0 && gridDirection.y == -1) return 1;
        if (gridDirection.x == 1 && gridDirection.y == 0) return 2;
        if (gridDirection.x == -1 && gridDirection.y == 0) return 3;
        return 1;
    }

    int calcTurnTypeCode()
    {
        if (headRotationCode_PREV == 0 && headRotationCode == 2) return 4;
        if (headRotationCode_PREV == 0 && headRotationCode == 3) return 5;
        if (headRotationCode_PREV == 1 && headRotationCode == 2) return 6;
        if (headRotationCode_PREV == 1 && headRotationCode == 3) return 7;
        if (headRotationCode_PREV == 2 && headRotationCode == 0) return 8;
        if (headRotationCode_PREV == 2 && headRotationCode == 1) return 9;
        if (headRotationCode_PREV == 3 && headRotationCode == 0) return 10;
        if (headRotationCode_PREV == 3 && headRotationCode == 1) return 11;
        return 0;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Food")
        {
            GameObject.FindGameObjectWithTag("SFX").GetComponent<SFXManager>().PlaySound("Food");
            newFood();
            snakebodysize++;
            GameController.instance.SnakeAte();
            snakesize = GetFullSnake();

            if (!observeRays)
            {
                SetReward(1f);
                Done();
            }
            else
            {
                AddReward(1); // Accumulate and don't end
            }
        }
        else
        {
            GameObject.FindGameObjectWithTag("SFX").GetComponent<SFXManager>().PlaySound("Death");
            dead = true;
            GameController.instance.s = 0;
            Done(); // No point in neg reward if it dies ... (just finish ep)
            refreshBody();
        }
    }

    public void refreshBody()
    {
        if (snakebodysize > 0)
        {
            for (int i = bodyParts.Count - 1; i >= 0; i--)
            {
                Destroy(bodyParts[i]);
            }
            bodyParts.Clear();
        }
    }

    public List<Vector2> GetFullSnake()
    {
        List<Vector2> list = new List<Vector2>() { gridPosition };
        list.AddRange(tail);
        return list;
    }

    /******************************************************************************************************************/

    public override void AgentReset()
    {
        if (dead)
        {
            //ResetReward();
            SnakeInit();
        }
    }

    public override void CollectObservations()
    {
        if (observeVectors)
        {
            AddVectorObs(targetPos);
            AddVectorObs(new Vector2(gridPosition.x, gridPosition.y));
            AddVectorObs((float)gridDirection.x);
            AddVectorObs((float)gridDirection.y);

            if (vectorWalls)
            {
                AddVectorObs(topBorder.localPosition.y);
                AddVectorObs(bottomBorder.localPosition.y);
                AddVectorObs(rightBorder.localPosition.x);
                AddVectorObs(leftBorder.localPosition.x);

                // 10
            }
            // 6
        }

        if (observeRays)
        {
            Vector2 origin = transform.position;
            LayerMask layerMask = LayerMask.GetMask("Obstacle");
            int rayLength = 30;

            RaycastHit2D hitFront = Physics2D.Raycast(origin, -transform.up, rayLength, layerMask);
            RaycastHit2D hitLeft = Physics2D.Raycast(origin, -transform.right, rayLength, layerMask);
            RaycastHit2D hitRight = Physics2D.Raycast(origin, transform.right, rayLength, layerMask);

            // Forward observation
            if (hitFront.collider != null)
            {
                //Debug.Log(hitFront.collider.gameObject.name);
                //Debug.Log(hitFront.distance/rayLength);
                //Debug.Log("===========");
                Debug.DrawRay(origin, -transform.up * hitFront.distance, Color.red);
                AddVectorObs(1 - hitFront.distance / rayLength);
            }
            else
            {
                AddVectorObs(0); //Out of sight
            }

            // Left observation
            if (hitLeft.collider != null)
            {
                //Debug.Log(hitLeft.collider.gameObject.name);
                //Debug.Log(hitLeft.distance / rayLength);
                //Debug.Log("===========");
                Debug.DrawRay(origin, -transform.right * hitLeft.distance, Color.red);
                AddVectorObs(1 - hitLeft.distance / rayLength);
            }
            else
            {
                AddVectorObs(0); //Out of sight
            }

            // Right observation
            if (hitRight.collider != null)
            {
                //Debug.Log(hitRight.collider.gameObject.name);
                //Debug.Log(hitRight.distance/rayLength);
                //Debug.Log("===========");
                Debug.DrawRay(origin, transform.right * hitRight.distance, Color.red);
                AddVectorObs(1 - hitRight.distance / rayLength);
            }
            else
            {
                AddVectorObs(0); //Out of sight
            }
            //3
        }

        if (observeRaysOnly) // boolean style (purely from the agents perspective)
        {
            // Added to for direction (comment if model below raycast14)
            AddVectorObs((float)gridDirection.x);
            AddVectorObs((float)gridDirection.y);

            Vector2 origin = transform.position;
            LayerMask layerMask = LayerMask.GetMask("Obstacle", "Tail");
            LayerMask layerMask2 = LayerMask.GetMask("Food", "Obstacle");
            int rayLength = 30;

            RaycastHit2D hitFront = Physics2D.Raycast(origin, -transform.up, rayLength, layerMask);
            RaycastHit2D hitLeft = Physics2D.Raycast(origin, -transform.right, rayLength, layerMask);
            RaycastHit2D hitRight = Physics2D.Raycast(origin, transform.right, rayLength, layerMask);

            RaycastHit2D hitFront2 = Physics2D.Raycast(origin, -transform.up, rayLength, layerMask2);
            RaycastHit2D hitLeft2 = Physics2D.Raycast(origin, -transform.right, rayLength, layerMask2);
            RaycastHit2D hitRight2 = Physics2D.Raycast(origin, transform.right, rayLength, layerMask2);

            // Forward observation
            if (hitFront.collider != null)
            {
                //Debug.Log(hitFront.collider.gameObject.name);
                //Debug.Log(hitFront.distance/rayLength);
                //Debug.Log("===========");
                Debug.DrawRay(origin, -transform.up * hitFront.distance, Color.red);
                AddVectorObs(1 - hitFront.distance / rayLength);
            }
            else
            {
                AddVectorObs(0); //Out of sight
            }

            // Left observation
            if (hitLeft.collider != null)
            {
                //Debug.Log(hitLeft.collider.gameObject.name);
                //Debug.Log(hitLeft.distance / rayLength);
                //Debug.Log("===========");
                Debug.DrawRay(origin, -transform.right * hitLeft.distance, Color.red);
                AddVectorObs(1 - hitLeft.distance / rayLength);
            }
            else
            {
                AddVectorObs(0); //Out of sight
            }

            // Right observation
            if (hitRight.collider != null)
            {
                //Debug.Log(hitRight.collider.gameObject.name);
                //Debug.Log(hitRight.distance/rayLength);
                //Debug.Log("===========");
                Debug.DrawRay(origin, transform.right * hitRight.distance, Color.red);
                AddVectorObs(1 - hitRight.distance / rayLength);
            }
            else
            {
                AddVectorObs(0); //Out of sight
            }

            // =====================================================================================

            // Forward observation 2
            if (hitFront2.collider != null && hitFront2.collider.gameObject.layer == 11)
            {
                //Debug.Log(hitFront.collider.gameObject.name);
                //Debug.Log(hitFront.distance/rayLength);
                //Debug.Log("===========");
                Debug.DrawRay(origin, -transform.up * hitFront2.distance, Color.green);
                AddVectorObs(1 - hitFront2.distance / rayLength);
            }
            else
            {
                AddVectorObs(0); //Out of sight
            }

            // Left observation 2
            if (hitLeft2.collider != null &&  hitLeft2.collider.gameObject.layer == 11)
            {
                //Debug.Log(hitLeft.collider.gameObject.name);
                //Debug.Log(hitLeft.distance / rayLength);
                //Debug.Log("===========");
                Debug.DrawRay(origin, -transform.right * hitLeft2.distance, Color.green);
                AddVectorObs(1 - hitLeft2.distance / rayLength);
            }
            else
            {
                AddVectorObs(0); //Out of sight
            }

            // Right observation 2
            if (hitRight2.collider != null &&  hitRight2.collider.gameObject.layer == 11)
            {
                //Debug.Log(hitRight.collider.gameObject.name);
                //Debug.Log(hitRight.distance/rayLength);
                //Debug.Log("===========");
                Debug.DrawRay(origin, transform.right * hitRight2.distance, Color.green);
                AddVectorObs(1 - hitRight2.distance / rayLength);
            }
            else
            {
                AddVectorObs(0); //Out of sight
            }
            // 8
        }
    }

    public override void AgentAction(float[] vectorAction)
    {
        var input = Mathf.FloorToInt(vectorAction[0]); // Get index action for movement input
        //AddReward(-0.01f);

        switch (input)
        {
            case 0:
                if (gridDirection.x != 0 && gridDirection.y != -1)
                {
                    gridDirection.x = 0;
                    gridDirection.y = 1;
                }
                break;

            case 1:
                if (gridDirection.x != 0 && gridDirection.y != 1)
                {
                    gridDirection.x = 0;
                    gridDirection.y = -1;
                }
                break;

            case 2:
                if (gridDirection.x != -1 && gridDirection.y != 0)
                {
                    gridDirection.x = 1;
                    gridDirection.y = 0;
                }
                break;

            case 3:
                if (gridDirection.x != 1 && gridDirection.y != 0)
                {
                    gridDirection.x = -1;
                    gridDirection.y = 0;
                }
                break;
            case 4:
                // Do nothing
                break;
            default:
                throw new System.ArgumentException("Invalid action value");
        }
    }

    public override float[] Heuristic()
    {
        var action = new float[1];

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            action[0] = 0;
            return action;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            action[0] = 1;
            return action;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            action[0] = 2;
            return action;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            action[0] = 3;
            return action;
        }

        // If nothing is equal to 4
        action[0] = 4;
        return action;
    }
}

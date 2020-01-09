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
    public bool canRotate;
    private int snakebodysize;
    private int headRotationCode;
    private int headRotationCode_PREV;

    private float Timer;
    private float MaxTimer;
    private float ExecutionTimer;

    public GameObject prevG;
    public List<GameObject> bodyParts;

    public bool isVisual = true;

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

        MaxTimer = 0.1f; // 0.07f //0.0025f
        Timer = MaxTimer;
        ExecutionTimer = 0;

        tail = new List<Vector2>();
        tailRotation = new List<int>();
        snakebodysize = 0;

        snakesize = GetFullSnake();
        newFood();

        canRotate = true;
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

        //if (canRotate)
        //{
            
        //}

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

            if (tail.Count >= snakebodysize + 1)
            {
                tail.RemoveAt(tail.Count - 1);
                tailRotation.RemoveAt(tailRotation.Count - 1);
            }

           //prevG = null;

            for (int i = 0; i < tail.Count; i++)
            {
                Vector2 snakePosition = tail[i];
                int rotation = tailRotation[i];
                Vector3 p = new Vector3(snakePosition.x, snakePosition.y);

                // if (prevG!=null) Destroy(prevG);  // destroy tail
                if (tail.Count > 0)
                {
                    GameObject tailObject = bodyParts[tail.Count - 1];
                    Destroy(tailObject);
                    bodyParts.RemoveAt(tail.Count - 1);
                }
                //Destroy(bodyParts[bodyParts.Count - 1]);
                GameObject g = (GameObject)Instantiate(tailPrefab, p, Quaternion.identity);
                bodyParts.Add(g);
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

                //Object.Destroy(g, MaxTimer);
                
                // add to game object list here

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
            newFood();
            snakebodysize++;
            snakesize = GetFullSnake();
            SetReward(1f);
            Done();
        }
        else
        {
            
            dead = true;
            //SetReward(-0.5f);
            Done();
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
        /* FIRST ATTEMPT: No convergance from this....*/
        // Border positions (0)
        //AddVectorObs(topBorder.localPosition.y);
        //AddVectorObs(bottomBorder.localPosition.y);
        //AddVectorObs(rightBorder.localPosition.x);
        //AddVectorObs(leftBorder.localPosition.x);

        // Target and Agent positions (4)
        //AddVectorObs(Vector2.Distance(targetPos, new Vector2(gridPosition.x, gridPosition.y)));
        // ==AddVectorObs(targetPos);
        // ==AddVectorObs(new Vector2(gridPosition.x, gridPosition.y));

        // Agent direction and length (2)
        // ==AddVectorObs((float)gridDirection.x);
        // ==AddVectorObs((float)gridDirection.y);
        //AddVectorObs((float)snakebodysize);
        //*/

        // There are no numeric observations to collect as this environment uses visual
        // observations.

        if (!isVisual)
        {
            AddVectorObs(targetPos);
            AddVectorObs(new Vector2(gridPosition.x, gridPosition.y));
            AddVectorObs((float)gridDirection.x);
            AddVectorObs((float)gridDirection.y);

            AddVectorObs(topBorder.localPosition.y);
            AddVectorObs(bottomBorder.localPosition.y);
            AddVectorObs(rightBorder.localPosition.x);
            AddVectorObs(leftBorder.localPosition.x);

            // 10
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
                canRotate = false;
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
                canRotate = false;
                break;

            case 3:
                if (gridDirection.x != 1 && gridDirection.y != 0)
                {
                    gridDirection.x = -1;
                    gridDirection.y = 0;
                }
                canRotate = false;
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

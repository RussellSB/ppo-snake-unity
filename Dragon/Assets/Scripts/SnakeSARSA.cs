using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class SnakeSARSA : MonoBehaviour
{
    private Vector2Int gridPosition;
    private Vector2Int gridDirection;

    public GameObject food;
    public GameObject tailPrefab;

    public Transform topBorder;
    public Transform bottomBorder;
    public Transform leftBorder;
    public Transform rightBorder;

    List<Vector2Int> tail;
    List<int> tailRotation; //Used in congruence to tail (0 - up, 1 - down, 2 - right, 3 - left)
    List<Vector2Int> snakesize;

    private int snakebodysize = 0;
    bool eat;
    bool dead;
    int headRotationCode = 1; // (0 - up, 1 - down, 2 - right, 3 - left)
    int headRotationCode_PREV; // (0 - up, 1 - down, 2 - right, 3 - left)

    private float Timer;
    private float MaxTimer;
    //private float ExecutionTimer;

    public List<GameObject> bodyParts;

    float learning_rate = 0.9f;
    float discount = 0.9f;
    float epilson_rate = 1950;

    int reward;

    string current_state;
    int current_action;
    string next_state;
    int next_action;

    Dictionary<string, float[]> qvalues = new Dictionary<string, float[]>();

    bool canRotate = true;

    private void onStart()
    {
        float lb = GameObject.Find("LeftBorder").transform.position.x;
        float rb = GameObject.Find("RightBorder").transform.position.x;
        float tb = GameObject.Find("TopBorder").transform.position.y;
        float bb = GameObject.Find("BottomBorder").transform.position.y;

        //Debug.Log((lb-rb) + " " + (rb-lb) + " " + (bb - tb) + " " + (tb - bb));

        string destination = Application.dataPath + "/Saves/save.dat";

        if (File.Exists(destination))
        {
            LoadFile();
            Debug.Log("Loaded");

            //for (int i = (int)(lb - rb); i <= (int)(rb - lb); i++)
            //{
            //    for (int j = (int)(bb - tb); j <= (int)(tb - bb); j++)
            //    {
            //        string state = (i) + ", " + (j);

            //        for (int k = 0; k < 4; k++)
            //        {
            //            Debug.Log("State: " + state + ", Action: " + k + ", Q-value: " + qvalues[state][k]);
            //        }
            //    }
            //}
        }
        else
        {
            for (int i = (int)(lb - rb); i <= (int)(rb - lb); i++)
            {
                for (int j = (int)(bb - tb); j <= (int)(tb - bb); j++)
                {
                    float[] possActions = { 0, 0, 0, 0 };

                    string state = (i) + ", " + (j);
                    qvalues.Add(state, possActions);
                }
            }
        }

    }

    private void Awake()
    {
        bodyParts = new List<GameObject>();
        SnakeInit();
        onStart();
    }

    private void SnakeInit()
    {
        int x = (int)Random.Range(leftBorder.position.x + 1, rightBorder.position.x - 1);
        int y = (int)Random.Range(bottomBorder.position.y + 1, topBorder.position.y - 1);

        gridPosition = new Vector2Int(x, y);
        gridDirection = new Vector2Int(0, -1);

        MaxTimer = 0.0001f;
        Timer = MaxTimer;
        //ExecutionTimer = 0;

        tail = new List<Vector2Int>();
        tailRotation = new List<int>();
        snakebodysize = 0;

        snakesize = GetFullSnake();

        GameObject[] fd = GameObject.FindGameObjectsWithTag("Food");
        foreach (GameObject fb in fd)
            GameObject.Destroy(fb);

        food.GetComponent<FoodQL>().SpawnFood(snakesize);

        canRotate = true;

        current_state = State();
        current_action = GetAction(current_state);
        AIInput(current_action);

    }

    // main occurance of SARSA algorithm
    void Update()
    {
        if (!dead)
        {
            if (canRotate)
            {
                current_state = State();
                next_action = GetAction(current_state);
                AIInput(next_action);
            }

            Movement();
            int old_reward = reward;

            next_state = State();
            next_action = GetAction(next_state);
            AIInput(next_action);

            //updates the Q value
            UpdateState(current_state, current_action, old_reward, next_state, next_action);

            current_state = next_state;
            current_action = next_action;

            if (gridPosition.x > rightBorder.transform.position.x || gridPosition.x < leftBorder.transform.position.x)
            {
                dead = true;
            }

            if (gridPosition.y > topBorder.transform.position.y || gridPosition.y < bottomBorder.transform.position.y)
            {
                dead = true;
            }


        }

        else if (dead == true)
        {
            SnakeInit();
            GameController.instance.Iteration();
            UpdateEpilson();
            SaveFile(); 
            dead = false;
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

    public string State()
    {
        float f = GameObject.FindGameObjectWithTag("Food").transform.position.x;
        float s = gridPosition.x;
        float dif, dify;

        dif = f - s;

        f = GameObject.FindGameObjectWithTag("Food").transform.position.y;
        s = gridPosition.y;

        dify = f - s;

        string state = dif + ", " + dify;

        if (dif == 0 && dify == 0)
        {
            //Reward given on collision
        }
        else
        {
            reward = reward - 5; 
        }


        return state;
    }

    public int GetAction(string state)
    {
        System.Random rnd = new System.Random();
        float num = rnd.Next(0, 2001); 

        if (num < epilson_rate)
        {
            int action = rnd.Next(0, 4);
            return action;
        }
        else
        {
            float max = qvalues[state][0];

            int action = 0;

            for (int i = 0; i < 4; i++)
            {
                if (max > qvalues[state][i])
                {
                    max = qvalues[state][i];
                    action = i;
                }
            }

            if (max == 0)
            {
                action = rnd.Next(0, 4);
            }

            return action;
        }
    }

    public void UpdateState(string currentState, int currentAction, int reward, string nextState, int nextAction)
    {

        // current qvalue obtained by current policy
        float predict = qvalues[currentState][currentAction];

        // q value of current policy is updated according to the SARSA algorithm
        float target = (float)reward + (discount * qvalues[nextState][nextAction]);
        qvalues[currentState][currentAction] = predict + (learning_rate * (target - predict));

    }

    public void UpdateEpilson()
    {
        if (epilson_rate > 20)
        {
            epilson_rate = epilson_rate - 1;

        }
    }

    private void AIInput(int action)
    {
        if (action == 0)                //Action 0 defined as up
        {
            if (gridDirection.x != 0 && gridDirection.y != -1)
            {
                gridDirection.x = 0;
                gridDirection.y = 1;
            }
            canRotate = false;
        }
        if (action == 1)                //Action 1 defined as down
        {
            if (gridDirection.x != 0 && gridDirection.y != 1)
            {
                gridDirection.x = 0;
                gridDirection.y = -1;
            }

            canRotate = false;
        }
        if (action == 2)                //Action 2 defined as right
        {
            if (gridDirection.x != -1 && gridDirection.y != 0)
            {
                gridDirection.x = 1;
                gridDirection.y = 0;
            }

            canRotate = false;
        }
        if (action == 3)                // Action 3 defined as left
        {
            if (gridDirection.x != 1 && gridDirection.y != 0)
            {
                gridDirection.x = -1;
                gridDirection.y = 0;
            }

            canRotate = false;
        }

    }

    private void Movement()
    {
        Timer = Timer + Time.deltaTime;

        if (Timer >= MaxTimer)
        {
            refreshBody();
            Timer = Timer - MaxTimer;
            tail.Insert(0, gridPosition);

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

            gridPosition = gridPosition + gridDirection;

            if (eat)
            {
                snakebodysize++;
                eat = false;
            }

            if (tail.Count >= snakebodysize + 1)
            {
                tail.RemoveAt(tail.Count - 1);
                tailRotation.RemoveAt(tailRotation.Count - 1);
            }

            for (int i = 0; i < tail.Count; i++)
            {
                Vector2Int snakePosition = tail[i];
                int rotation = tailRotation[i];
                Vector3 p = new Vector3(snakePosition.x, snakePosition.y);

                GameObject g = (GameObject)Instantiate(tailPrefab, p, Quaternion.identity);
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
                bodyParts.Add(g);
            }


            transform.position = new Vector3(gridPosition.x, gridPosition.y);
            transform.eulerAngles = new Vector3(0, 0, GetAngleFromVector(gridDirection) - 270);

            if (!canRotate) canRotate = true;

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
            eat = true;
            reward = reward + 50; 
            Destroy(collision.gameObject);
            food.GetComponent<FoodQL>().SpawnFood(snakesize);
            GameController.instance.Iteration();
            UpdateEpilson();
            GameController.instance.SnakeAte();
            GameController.instance.SingleGame(true);

        }
        else
        {
            GameObject.FindGameObjectWithTag("SFX").GetComponent<SFXManager>().PlaySound("Death");
            dead = true;
            reward = reward - 1000; 
            GameController.instance.SingleGame(false);
            refreshBody();
        }
    }

    public List<Vector2Int> GetFullSnake()
    {
        List<Vector2Int> list = new List<Vector2Int>() { gridPosition };
        list.AddRange(tail);
        return list;
    }

    public void SaveFile()
    {
        string destination = Application.dataPath + "/Saves/save.dat"; // saves in Assets folder
        FileStream file;

        if (File.Exists(destination)) file = File.OpenWrite(destination);
        else file = File.Create(destination);

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, qvalues);
        file.Close();

    }

    public void LoadFile()
    {
        string destination = Application.dataPath + "/Saves/save.dat";
        FileStream file;

        if (File.Exists(destination)) file = File.OpenRead(destination);
        else
        {
            Debug.LogError("File not found");
            return;
        }

        BinaryFormatter bf = new BinaryFormatter();
        qvalues = (Dictionary<string, float[]>)bf.Deserialize(file);
        file.Close();

    }

}

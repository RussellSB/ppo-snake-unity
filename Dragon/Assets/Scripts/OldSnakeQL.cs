using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OldSnakeQL : MonoBehaviour
{
    private Vector2Int gridPosition;
    private Vector2Int gridDirection;

    public GameObject food;
    public GameObject tailPrefab;

    List<Vector2Int> tail;
    List<int> tailRotation; //Used in congruence to tail (0 - up, 1 - down, 2 - right, 3 - left)
    List<Vector2Int> snakesize;

    private int snakebodysize = 0;
    bool eat;
    bool dead;
    int headRotationCode = 1; // (0 - up, 1 - down, 2 - right, 3 - left)
    int headRotationCode_PREV; // (0 - up, 1 - down, 2 - right, 3 - left)

    float fx, fy, x, y;
    
    private float Timer;
    private float MaxTimer;

    float[,] qtable = new float[6, 4];
    float learning_rate = 0.9f;
    float discount = 0.75f;
    float epilson_rate = 950;

    int current_state;
    int reward;

    private void Awake()
    {
        gridPosition = new Vector2Int(0, 0);
        gridDirection = new Vector2Int(0, -1);
        MaxTimer = 0.1f;
        Timer = MaxTimer;

        tail = new List<Vector2Int>();
        tailRotation = new List<int>();
        snakebodysize = 0;

        snakesize = GetFullSnake();
        food.GetComponent<Food>().SpawnFood(snakesize);

        for(int state = 0; state < 6; state++)
        {
            for (int action = 0; action < 4; action++)
            {
                qtable[state, action] = 0;
            }
        }

    }

    void Update()
    {
       current_state = 0;                                          
        
       if(!dead)
        {
            int action = GetAction(current_state);
            int old_state = current_state;
            Debug.Log(action);
            Movement(action);
            Movement();
            if(current_state != 0 || current_state != 2 || current_state != 3 || current_state != 4)
            {
                current_state = 1;
                reward = -5;
            }

            closetoApple();
            Debug.Log("Reward" + reward);
            UpdateTable(action, old_state, current_state);
            Debug.Log("Reward" + reward);
        }else
        {
            gridPosition = new Vector2Int(0, 0);
            gridDirection = new Vector2Int(0, -1);
            MaxTimer = 0.1f;
            Timer = MaxTimer;

            tail = new List<Vector2Int>();
            tailRotation = new List<int>();
            snakebodysize = 0;

            snakesize = GetFullSnake();
            UpdateEpilson();
            dead = false;
        }
    
    }
    
    public void closetoApple()
    {
        float oldx = x;
        float oldy = y;

        float dif = fx - x;
        float dift = fx - oldx;

        float dify = fy - x;
        float difty = fy - oldx;

        x = gridPosition.x;
        y = gridPosition.y;

        if (fx - Mathf.Abs(x) < 5 && fx - Mathf.Abs(x) > -5 && fy - Mathf.Abs(y) < 2 && fy - Mathf.Abs(y) > -2) 
        {
            reward = reward + 20;
        }else if (fx - Mathf.Abs(x) < 10 && fx - Mathf.Abs(x) > -10 && fy - Mathf.Abs(y) < 5 && fy - Mathf.Abs(y) > -5)
        {
            reward = reward + 10;
        }


       
    }
    
    public int  GetAction(int state)
    {
        System.Random rnd = new System.Random();
        float num = rnd.Next(0, 1001);
       
        if(num < epilson_rate)
        {
            int action = rnd.Next(0, 4);
            return action;
        }else
        {
            float max = qtable[state, 0];
            int action = 0;

            for(int i = 0; i < 4; i++)
            {
                if(max > qtable[state, i])
                {
                    max = qtable[state, i];
                    action = i;
                }
            }

            return action;
        }
    }

    public void UpdateEpilson()
    {
        epilson_rate = epilson_rate - 1;
    }

    public void UpdateTable(int action, int state, int nextstate)
    {
        Debug.Log(action);
        float part1 = qtable[state, action];
        float part2 = Mathf.Max(qtable[nextstate, 0], qtable[nextstate, 1], qtable[nextstate, 2], qtable[nextstate, 3]);
        float update = qtable[state, action] + learning_rate * (reward + (discount * (Mathf.Max(qtable[nextstate, 0], qtable[nextstate, 1], qtable[nextstate, 2], qtable[nextstate, 3]))) - qtable[state, action]);
        qtable[state, action] = update;
        reward = 0;
    }

    private void Movement(int action)
    {
        if (action == 0)
        {
            if (gridDirection.x != 0 && gridDirection.y != -1)
            {
                SpriteRotation(0);
                gridDirection.x = 0;
                gridDirection.y = 1;
            }
        }
        if (action == 1)
        {
            if (gridDirection.x != 0 && gridDirection.y != 1)
            {
                SpriteRotation(1);
                gridDirection.x = 0;
                gridDirection.y = -1;
            }
        }
        if (action == 2)
        {
            if (gridDirection.x != -1 && gridDirection.y != 0)
            {
                SpriteRotation(2);
                gridDirection.x = 1;
                gridDirection.y = 0;
            }
        }
        if (action == 3)
        {
            if (gridDirection.x != 1 && gridDirection.y != 0)
            {
                SpriteRotation(3);
                gridDirection.x = -1;
                gridDirection.y = 0;
            }
        }
    }

    private void SpriteRotation(int r)
    {
        if (r == 0)
        {
            if (gridDirection.x == 1)
            {
                RotateRight();
            }
            else if (gridDirection.x == -1)
            {
                RotateLeft();
            }
        }
        else if (r == 1)
        {
            if (gridDirection.x == 1)
            {
                RotateLeft();
            }
            else if (gridDirection.x == -1)
            {
                RotateRight();
            }
        }
        else if (r == 2)
        {
            if (gridDirection.y == 1)
            {
                RotateLeft();
            }
            else if (gridDirection.y == -1)
            {
                RotateRight();
            }
        }
        else
        {
            if (gridDirection.y == 1)
            {
                RotateRight();
            }
            else if (gridDirection.y == -1)
            {
                RotateLeft();
            }
        }
    }

    private void RotateLeft()
    {
        transform.Rotate(Vector3.forward * -90);

    }

    private void RotateRight()
    {
        transform.Rotate(Vector3.forward * 90);
    }

    private void Movement()
    {
        Timer = Timer + Time.deltaTime;

        if (Timer >= MaxTimer)
        {
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
                        Debug.Log("Up");
                        break;
                    case 1:
                        Debug.Log("Down");
                        break;
                    case 2:
                        Debug.Log("Right");
                        g.GetComponent<Transform>().Rotate(Vector3.forward * 90);
                        break;
                    case 3:
                        Debug.Log("Left");
                        g.GetComponent<Transform>().Rotate(Vector3.forward * -90);
                        break;
                    case 4:
                        Debug.Log("Upright");
                        childStraight.SetActive(false);
                        childCorner3.SetActive(true);
                        break;
                    case 5:
                        Debug.Log("Upleft");
                        childStraight.SetActive(false);
                        childCorner4.SetActive(true);
                        break;
                    case 6:
                        Debug.Log("Downright");
                        childStraight.SetActive(false);
                        childCorner1.SetActive(true);
                        break;
                    case 7:
                        Debug.Log("Downleft");
                        childStraight.SetActive(false);
                        childCorner2.SetActive(true);
                        break;
                    case 8:
                        Debug.Log("Rightup");
                        childStraight.SetActive(false);
                        childCorner2.SetActive(true);
                        break;
                    case 9:
                        Debug.Log("Rightdown");
                        childStraight.SetActive(false);
                        childCorner4.SetActive(true);
                        break;
                    case 10:
                        Debug.Log("Leftup");
                        childStraight.SetActive(false);
                        childCorner1.SetActive(true);
                        //
                        break;
                    case 11:
                        Debug.Log("Leftdown");
                        childStraight.SetActive(false);
                        childCorner3.SetActive(true);
                        break;
                }

                Object.Destroy(g, MaxTimer);
            }

            transform.position = new Vector3(gridPosition.x, gridPosition.y);
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
        if (collision.name.StartsWith("Apple"))
        {
            eat = true;
            Destroy(collision.gameObject);
            GameController.instance.SnakeAte();
            reward = 150;
            current_state = 4;
            snakesize = GetFullSnake();
            food.GetComponent<Food>().SpawnFood(snakesize);
            fx = food.transform.position.x;
            fy = food.transform.position.x;
        }
        else if(collision.gameObject.CompareTag("Wall"))
        {
            dead = true;
            reward = -25;
            current_state = 2;
        }else if(collision.gameObject.CompareTag("Border"))
        {
            dead = true;
            reward = -25;
            current_state = 3;
        }else
        {
            dead = true;
            reward = -25;
            current_state = 5;
        }
    }

    public List<Vector2Int> GetFullSnake()
    {
        List<Vector2Int> list = new List<Vector2Int>() { gridPosition };
        list.AddRange(tail);
        return list;
    }

}

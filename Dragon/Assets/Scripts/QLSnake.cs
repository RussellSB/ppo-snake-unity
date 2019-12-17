﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class QLSnake : MonoBehaviour
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

    private float Timer;
    private float MaxTimer;

    Dictionary<string, float[]> qtable = new Dictionary<string, float[]>();

    float learning_rate = 0.9f;
    float discount = 0.75f;
    float epilson_rate = 950;

    int current_state;
    int reward;
    int iteration;

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

    }

    void Update()
    {

        if (!dead)
        {
            string current_state = State();
            int action = GetAction(current_state);
            Movement(action);
            Movement();
            string next_state = State();
           
            UpdateTable(action, current_state, next_state);
            reward = 0;
        }
        else
        {
            gridPosition = new Vector2Int(0, 0);
            gridDirection = new Vector2Int(0, -1);
            MaxTimer = 0.1f;
            Timer = MaxTimer;

            tail = new List<Vector2Int>();
            tailRotation = new List<int>();
            snakebodysize = 0;
            iteration += 1;
            Debug.Log(iteration);
            snakesize = GetFullSnake();
            UpdateEpilson();
            dead = false;
        }

    }

    public string State()
    {
        float f = food.transform.position.x;
        float s = gridPosition.x;
        float dif, dify;

        if(f < 0 || s < 0)
        {
            if(f < 0)
            {
                dif = Mathf.Abs(f) + s;
            }else
            {
                dif = Mathf.Abs(s) + f;
            }
        }else 
        {
            dif = f - s;
            if(dif < 0)
            {
                dif = Mathf.Abs(dif);
            }
        }

        f = food.transform.position.y;
        s = gridPosition.y;

        if (f < 0 || s < 0)
        {
            if (f < 0)
            {
                dify = Mathf.Abs(f) + s;
            }
            else
            {
                dify = Mathf.Abs(s) + f;
            }
        }
        else
        {
            dify = f - s;
            if (dify < 0)
            {
                dify = Mathf.Abs(dif);
            }
        }

        string state = System.Math.Round(dif, 2) + ", " + System.Math.Round(dif, 2);
        return state;
    }

    public void NewRow(string state)
    {
        if (qtable.ContainsKey(state))
        {
            return;
        }
        else
        {
            float[] actions = { 0, 0, 0, 0 };
            qtable.Add(state, actions);
        }
            
    }

    public int GetAction(string state)
    {
        System.Random rnd = new System.Random();
        float num = rnd.Next(0, 1001);

        if (num < epilson_rate || !qtable.ContainsKey(state))
        {
            int action = rnd.Next(0, 4);
            return action;
        }
        else
        {
            float max = qtable[state][0];
            int action = 0;

            for (int i = 0; i < 4; i++)
            {
                if (max > qtable[state][i])
                {
                    max = qtable[state][i];
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

    public void UpdateEpilson()
    {
        if(epilson_rate > 300)
        {
            epilson_rate = epilson_rate - 1;

        }
    }

    public void UpdateTable(int action, string state, string nextstate)
    {
        NewRow(state);
        NewRow(nextstate);
        float update = qtable[state][action] + learning_rate * (reward + (discount * (Mathf.Max(qtable[nextstate][0], qtable[nextstate][1], qtable[nextstate][2], qtable[nextstate][3]))) - qtable[state][action]);
        qtable[state][action] = update;
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
            reward = 25;
            snakesize = GetFullSnake();
            food.GetComponent<Food>().SpawnFood(snakesize);
        }
        else
        {
            dead = true;
            reward = -25;
        }
    }

    public List<Vector2Int> GetFullSnake()
    {
        List<Vector2Int> list = new List<Vector2Int>() { gridPosition };
        list.AddRange(tail);
        return list;
    }

}
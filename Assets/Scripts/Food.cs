using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    public GameObject food;

    public Transform topBorder;
    public Transform bottomBorder;
    public Transform leftBorder;
    public Transform rightBorder;

    void SpawnFood()
    {
        int x = (int)Random.Range(leftBorder.position.x, rightBorder.position.x);
        int y = (int)Random.Range(bottomBorder.position.y, topBorder.position.y);

        Instantiate(food, new Vector2(x, y), Quaternion.identity);
    }

    void Start()
    {
        InvokeRepeating("SpawnFood", 3, 4);    
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodQL : MonoBehaviour
{
    public GameObject food;

    public Transform topBorder;
    public Transform bottomBorder;
    public Transform leftBorder;
    public Transform rightBorder;

    public void SpawnFood(List<Vector2Int> list)
    {
        Vector2Int element = new Vector2Int();
        int x, y;

        do
        {
            x = (int)Random.Range(leftBorder.position.x + 4, rightBorder.position.x - 4);
            y = (int)Random.Range(bottomBorder.position.y + 4, topBorder.position.y - 4);
            element.x = x;
            element.y = y;

        } while (list.IndexOf(element) != -1);

        Instantiate(food, new Vector2(x, y), Quaternion.identity);
    }

}



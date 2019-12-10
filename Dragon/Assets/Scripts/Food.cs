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

    public BoxCollider2D wall;


    public void SpawnFood(List<Vector2Int> list)
    {
        Vector2Int element = new Vector2Int();
        int x, y;

        Vector2[] wallpts = CalculateWall();
        bool check = false;

        do
        {
            x = (int)Random.Range(leftBorder.position.x+1, rightBorder.position.x-1);
            y = (int)Random.Range(bottomBorder.position.y+1, topBorder.position.y-1);
            element.x = x;
            element.y = y;

            check = CheckWalls(wallpts, x, y);
            Debug.Log(check);

        } while (list.IndexOf(element) != -1  | check == true);

        Instantiate(food, new Vector2(x, y), Quaternion.identity);
    }


    public Vector2[] CalculateWall()
    {
        Vector2 size = wall.bounds.extents;
        Vector2 centre = wall.bounds.center;

        float top = centre.y + size.y;
        float btm = centre.y - size.y;

        float left = centre.x - size.x;
        float right = centre.x + size.x;

        Vector2 topLeft = new Vector2(left, top);
        Vector2 topRight = new Vector2(right, top);
        Vector2 btmLeft = new Vector2(left, btm);
        Vector2 btmRight = new Vector2(right, btm);

        Vector2[] wallpts = { topLeft, topRight, btmLeft, btmRight };

        return wallpts;

    }


    public bool CheckWalls(Vector2[] wallpts, int x, int y)
    {
        bool check = false;
       
        if (y <= wallpts[0].y  & y >= wallpts[2].y)
        {
            if (x >= wallpts[0].x & x <= wallpts[1].x)
            {
                check = true;
            }            
        }else
        {
            check = false;
        }

        return check;
    }
}


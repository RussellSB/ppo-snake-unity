using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    private Vector2Int gridPosition;
    private Vector2Int gridDirection;
    private float Timer;
    private float MaxTimer;

    private void Awake()
    {
        gridPosition = new Vector2Int(0, 0);
        gridDirection = new Vector2Int(0, -1);
        MaxTimer = 0.75f;
        Timer = MaxTimer;
    }

    // Update is called once per frame
    void Update()
    {
        UserInput();
        Timer = Timer + Time.deltaTime;
        if(Timer >= MaxTimer)
        {
            gridPosition = gridPosition + gridDirection;
            Timer = Timer - MaxTimer;
        }

        transform.position = new Vector3(gridPosition.x, gridPosition.y);
    }

    private void UserInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (gridDirection.x != 0 && gridDirection.y != -1)
            {
                SpriteRotation(0);
                gridDirection.x = 0;
                gridDirection.y = 1;
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (gridDirection.x != 0 && gridDirection.y != 1)
            {
                SpriteRotation(1);
                gridDirection.x = 0;
                gridDirection.y = -1;
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (gridDirection.x != -1 && gridDirection.y != 0)
            {
                SpriteRotation(2);
                gridDirection.x = 1;
                gridDirection.y = 0;
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
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
        if(r == 0)
        {
            if(gridDirection.x == 1)
            {
                RotateRight();
            }else if(gridDirection.x == -1)
            {
                RotateLeft();
            }
        }else if (r == 1)
        {
            if (gridDirection.x == 1)
            {
                RotateLeft();
            }
            else if (gridDirection.x == -1)
            {
                RotateRight();
            }
        }else if (r == 2)
        {
            if (gridDirection.y == 1)
            {
                RotateLeft();
            }
            else if (gridDirection.y == -1)
            {
                RotateRight();
            }
        }else
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
}

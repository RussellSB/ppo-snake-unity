using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Snake : MonoBehaviour
{
    private Vector2Int gridPosition;
    private Vector2Int gridDirection;

    public GameObject tailPrefab;
    List<Vector2Int> tail = new List<Vector2Int>();
    int snakebodysize = 0;
    bool eat;

    private float Timer;
    private float MaxTimer;

    private void Awake()
    {
        gridPosition = new Vector2Int(0, 0);
        gridDirection = new Vector2Int(0, -1);
        MaxTimer = 0.3f;
        Timer = MaxTimer;
    }

    // Update is called once per frame
    void Update()
    {
        UserInput();
        Timer = Timer + Time.deltaTime;
        if(Timer >= MaxTimer)
        {
            tail.Insert(0, gridPosition);

            gridPosition = gridPosition + gridDirection;
            Timer = Timer - MaxTimer;

            if (eat)
            {
                Debug.Log("Eat is true");
                snakebodysize++;
                eat = false;
                Debug.Log("Eat is false");
            }

            if (tail.Count >= snakebodysize + 1)
            {
                tail.RemoveAt(tail.Count - 1);
            }

            for(int i = 0; i < tail.Count; i++)
            {
                Vector2Int  tailPosition = tail[i];
                Vector3 tp = new Vector3(tailPosition.x, tailPosition.y);
                GameObject g = (GameObject)Instantiate(tailPrefab, tp, Quaternion.identity);
                
            }

           

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

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.name.StartsWith("Apple"))
        {
            eat = true;
            Destroy(collision.gameObject);
        }else
        {
            //Die
        }
    }

  
}

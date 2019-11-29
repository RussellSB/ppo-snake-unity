using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food
{
    private Vector2Int foodPosition;
    private GameObject food;
    private int h;
    private int w;


    public Food(int width, int height)
    {
        this.w = width;
        this.h = height;

        SpawnFood();
    }

    private void SpawnFood()
    {
        foodPosition = new Vector2Int(Random.Range(0, w), Random.Range(0, h));

        food = new GameObject("food", typeof(SpriteRenderer));
        food.GetComponent<SpriteRenderer>().sprite = GameManger.i.foodSprite;
        food.transform.position = new Vector2(foodPosition.x, foodPosition.y);
    }
  
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManger : MonoBehaviour
{
    [SerializeField] private Snake snake;
    [SerializeField] private Food fd;

    public static GameManger i;

    public Sprite snakeHeadSprite;
    public Sprite foodSprite;

    public void Start()
    {
        fd = new Food(5, 5);

    }
    public void Awake()
    {
        i = this;
    }

}

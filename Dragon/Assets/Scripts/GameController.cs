using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public Text score;
    public GameObject gameover;

    private int s = 0;
    public bool gameOver = false;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }else if(instance != this)
        {
            Destroy(gameObject);
        }
        
    }
  

    void Update()
    {
        //Restart when prompted
    }

    public void SnakeAte()
    {
        if(gameOver)
        {
            return;
        }

        s = s + 1;
        score.text = "Score: " + s.ToString();

    }

    public void GameEnd()
    {
        gameover.SetActive(true);
        gameOver = true;
    }
}

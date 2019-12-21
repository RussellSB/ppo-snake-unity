using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public Text score;
    public Text singleScore;
    public Text iteration;

    public GameObject gameover;

    protected bool pause = false;

    private int s = 0;
    private int s1 = 0;
    private int i = 0;

    public bool gameOver = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else if (instance != this)
        {
            Destroy(gameObject);
        }

    }

    void Update()
    {
        if ((gameover) && gameOver && Input.GetKeyUp(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else if ((!gameover) && gameOver)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Pausecontinue();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }

    }

    public void Pausecontinue()
    {
        if (pause == false)
        {
            pause = true;
            PauseGame();
        }
        else if (pause == true)
        {
            pause = false;
            ContinueGame();
        }
    }

    public void SnakeAte()
    {
        if (gameOver)
        {
            return;
        }

        s = s + 1;
        score.text = "Score: " + s.ToString();

    }

    public void SingleGame(bool c)
    {
       if(c)
        {
            s1 = s1 + 1;
            singleScore.text = "Score for Game: " + s1.ToString();
        }else
        {
            s1 = 0;
            singleScore.text = "Score for Game: 0";

        }
    }

    public void Iteration()
    {
        i = i + 1;
        iteration.text = "Iteration: " + i.ToString();
    }

    public void GameEnd()
    {
        if (gameover) gameover.SetActive(true);
        gameOver = true;
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void ContinueGame()
    {
        Time.timeScale = 1;
    }

    public void ReturnMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}


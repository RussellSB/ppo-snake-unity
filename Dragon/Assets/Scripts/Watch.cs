using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Watch : MonoBehaviour
{
    public void LoadQ()
    {
        SceneManager.LoadScene(10);
    }

    public void LoadSARSA()
    {
        SceneManager.LoadScene(9);
    }

    public void LoadPPO_V()
    {
        SceneManager.LoadScene(5);
    }

    public void LoadPPO_C()
    {
        SceneManager.LoadScene(8);
    }

    public void LoadPPO_R1()
    {
        SceneManager.LoadScene(6);
    }

    public void LoadPPO_R2()
    {
        SceneManager.LoadScene(7);
    }

    public void LoadA()
    {
        SceneManager.LoadScene(2);
    }
}

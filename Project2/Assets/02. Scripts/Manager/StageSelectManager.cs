using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectManager : MonoBehaviour
{
    public void LoadTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void LoadStage1()
    {
        SceneManager.LoadScene("Stage1");
    }

    public void LoadStage2()
    {
        SceneManager.LoadScene("Stage2");
    }

    public void LoadInfiniteStage1()
    {
        SceneManager.LoadScene("InfiniteStage1");
    }

    public void LoadInfiniteStage2()
    {
        SceneManager.LoadScene("InfiniteStage2");
    }

    public void LoadMain()
    {
        SceneManager.LoadScene("MainScene");
    }
}

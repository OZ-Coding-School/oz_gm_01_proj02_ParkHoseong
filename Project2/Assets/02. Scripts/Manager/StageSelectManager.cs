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
        DataManager.Instance.isInfiniteMode = false;
        SceneManager.LoadScene("Stage1");
    }

    public void LoadStage2()
    {
        DataManager.Instance.isInfiniteMode = false;
        SceneManager.LoadScene("Stage2");
    }

    public void LoadInfiniteStage1()
    {
        DataManager.Instance.isInfiniteMode = true;
        SceneManager.LoadScene("Stage1");
    }

    public void LoadInfiniteStage2()
    {
        DataManager.Instance.isInfiniteMode = true;
        SceneManager.LoadScene("Stage2");
    }

    public void LoadMain()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("MainScene");
    }
}

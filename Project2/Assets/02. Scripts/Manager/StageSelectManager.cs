using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectManager : MonoBehaviour
{
    public void LoadTutorial()
    {
        if (DataManager.Instance != null)
            DataManager.Instance.selectedMode = 0;

        SceneManager.LoadScene("Tutorial");
    }

    public void LoadStage1()
    {
        if (DataManager.Instance != null)
            DataManager.Instance.selectedMode = 0;

        SceneManager.LoadScene("Stage1");
    }

    public void LoadStage1Annihilation()
    {
        if (DataManager.Instance != null)
            DataManager.Instance.selectedMode = 1;

        SceneManager.LoadScene("Stage1");
    }

    public void LoadStage2()
    {
        if (DataManager.Instance != null)
            DataManager.Instance.selectedMode = 0;

        SceneManager.LoadScene("Stage2");
    }

    public void LoadStage2Annihilation()
    {
        if (DataManager.Instance != null)
            DataManager.Instance.selectedMode = 1;

        SceneManager.LoadScene("Stage2");
    }

    public void LoadInfiniteStage1()
    {
        if (DataManager.Instance != null)
            DataManager.Instance.selectedMode = 2;

        SceneManager.LoadScene("Stage1");
    }

    public void LoadInfiniteStage2()
    {
        if (DataManager.Instance != null)
            DataManager.Instance.selectedMode = 2;

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject menuPanel; // 인스펙터에서 MenuPanel 할당

    [Header("Settings")]
    [SerializeField] private bool isFPSScene = true;

    private bool isPaused = false;

    void Start()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    void Update()
    {
        // Esc 키로도 메뉴 토글 가능
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    // Menu Button을 눌렀을 때 호출
    public void Pause()
    {
        isPaused = true;
        if (menuPanel != null) menuPanel.SetActive(true);

        Time.timeScale = 0f; // 게임 일시정지

        // 에러 해결: SetLock 대신 Acquire 사용
        InputLockManager.Acquire("PauseMenu");
    }

    // Resume 버튼을 눌렀을 때 호출
    public void Resume()
    {
        isPaused = false;

        if (menuPanel != null) menuPanel.SetActive(false); //

        Time.timeScale = 1f;

        InputLockManager.Release("PauseMenu");

        if (isFPSScene)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            // 스테이지 선택 씬 등에서는 마우스를 계속 보여줍니다.
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void GoToMain()
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.Save();
        }

        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("MainScene");
    }
}

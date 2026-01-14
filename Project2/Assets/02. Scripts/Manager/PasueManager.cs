using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PasueManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject menuPanel; // 인스펙터에서 MenuPanel 할당

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
        if (menuPanel != null) menuPanel.SetActive(false);

        Time.timeScale = 1f; // 게임 재개

        // 에러 해결: SetLock 대신 Release 사용
        InputLockManager.Release("PauseMenu");
    }

    public void GoToMain()
    {
        Time.timeScale = 1f;
        // 해제해주지 않고 씬을 넘기면 다음 씬에서도 입력이 막힐 수 있음
        InputLockManager.Release("PauseMenu");
        SceneManager.LoadScene("MainScene");
    }
}

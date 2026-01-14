using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        CheckContinueButton();
    }
    private void CheckContinueButton()
    {
        if (continueButton == null) return;

        bool hasSaveData = PlayerPrefs.HasKey("LastClearedStage");

        continueButton.interactable = hasSaveData;
    }
    public void StartNewGame()
    {
        PlayerPrefs.DeleteKey("LastClearedStage");
        SceneManager.LoadScene("StageSelectScene");
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene("StageSelectScene");
    }

    public void OpenSettings() => settingsPanel.SetActive(true);
    public void CloseSettings() => settingsPanel.SetActive(false);

    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    [Header("모드 설정")]
    [SerializeField] private bool isInfiniteMode = false;
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("UI참조")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI clipsText; //예비 탄창 텍스트
    [SerializeField] private TextMeshProUGUI resultTitleText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private UnityEngine.UI.Slider healthSlider;

    public int CurrentAmmo;
    public int TotalClips;
    private int MaxAmmo;

    void Awake()
    {
        if (DataManager.Instance != null)
            isInfiniteMode = DataManager.Instance.isInfiniteMode;

        ResetSession();

        if (scorePanel != null)
            scorePanel.SetActive(isInfiniteMode);

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        UpdateUI();
    }

    public void ResetSession()
    {
        DataManager.ResetCurrentScore();
        CurrentAmmo = 0;
        TotalClips = 0;
        MaxAmmo = 0;
    }

    // PlayerShooter에서 총알 소비 시 호출
    public void ConsumeAmmo(int currentAmmo, int totalClips, WeaponData data)
    {
        this.CurrentAmmo = currentAmmo;
        this.TotalClips = totalClips;
        if (data != null) this.MaxAmmo = data.maxAmmo;
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        DataManager.AddScore(amount);
        UpdateUI();
    }
    public void UpdateHealthUI(int current, int max)
    {
        if (healthText!=null)
            healthText.text = $"HP: {current}/{max}";

        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
    }

    public void ShowGameOver(bool isWin)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (resultTitleText != null)
            {
                resultTitleText.text = isWin ? "STAGE CLEAR!" : "YOU DIE";
                
                resultTitleText.color = isWin ? Color.green : Color.red;
            }

            UpdateUI();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void UpdateUI()
    {
        if (scoreText) scoreText.text = "Score: " + DataManager.TotalScore;
        if (finalScoreText) finalScoreText.text = "Final Score: " + DataManager.TotalScore;
        if (bestScoreText) bestScoreText.text = "Best: " + DataManager.GetBestScore();
        if (ammoText) ammoText.text = $"Ammo: {CurrentAmmo}/{MaxAmmo}";
        if (clipsText) clipsText.text = $"Clips: {TotalClips}";
    }
    public void ShowFinalScore()
    {
        if (scorePanel != null) scorePanel.SetActive(true);
        UpdateUI();
    }
}

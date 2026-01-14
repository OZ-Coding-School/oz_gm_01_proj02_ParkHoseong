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

    [Header("UI참조")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI clipsText; //예비 탄창 텍스트
    [SerializeField] private TextMeshProUGUI restartText;

    // 이제 Ammo와 Clips는 PlayerShooter에서 가져옴
    public int CurrentAmmo;
    public int TotalClips;
    private int MaxAmmo;

    void Awake()
    {
        ResetSession();

        if (scorePanel != null)
        {
            scorePanel.SetActive(isInfiniteMode);
        }

        UpdateUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SceneManager.LoadScene("FPSField");
        }
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

    private void UpdateUI()
    {
        if (scoreText) scoreText.text = "Score: " + DataManager.TotalScore;
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

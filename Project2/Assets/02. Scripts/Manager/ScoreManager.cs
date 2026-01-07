using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    [Header("탄약 세팅")]
    [SerializeField] private int maxAmmo = 30;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI clipsText; // 예비 탄창 텍스트
    [SerializeField] private TextMeshProUGUI restartText;

    public int Score { get; private set; }

    // 이제 Ammo와 Clips는 PlayerShooter에서 가져옴
    [HideInInspector] public int CurrentAmmo;
    [HideInInspector] public int TotalClips;

    void Awake()
    {
        ResetSession();
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
        Score = 0;
        CurrentAmmo = maxAmmo;
        TotalClips = 3; // 기본 예비 탄창 개수
    }

    // PlayerShooter에서 총알 소비 시 호출
    public void ConsumeAmmo(int currentAmmo, int totalClips)
    {
        CurrentAmmo = currentAmmo;
        TotalClips = totalClips;
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        Score += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText) scoreText.text = "Score: " + Score;
        if (ammoText) ammoText.text = $"Ammo: {CurrentAmmo}/{maxAmmo}";
        if (clipsText) clipsText.text = $"Clips: {TotalClips}";
    }
}

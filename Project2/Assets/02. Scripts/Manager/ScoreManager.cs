using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    [Header("모드 설정")]
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private GameObject gameOverPanel;

    [SerializeField] private GameObject warningUI;
    [SerializeField] private TextMeshProUGUI warningText;

    [Header("UI참조")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI clipsText; //예비 탄창 텍스트
    [SerializeField] private TextMeshProUGUI resultTitleText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private UnityEngine.UI.Slider healthSlider;

    [Header("Tutorial Guidance")]
    [SerializeField] private GameObject missionGuidePanel;
    [SerializeField] private TextMeshProUGUI missionGuideText;
    [SerializeField] private Image missionGuideImage;

    [Header("Mission Guide Sprites")]
    [SerializeField] private Sprite scoutGuideSprite;
    [SerializeField] private Sprite annihilationGuideSprite;
    [SerializeField] private Sprite infiniteGuideSprite;

    public int CurrentAmmo;
    public int TotalClips;
    private int MaxAmmo;

    private int currentStageIndex = -1;

    void Awake()
    {
        bool isInfiniteMode = (DataManager.Instance != null && DataManager.Instance.selectedMode == 2);

        ResetSession();

        if (scorePanel != null)
            scorePanel.SetActive(isInfiniteMode);

        if (gameOverPanel != null) 
            gameOverPanel.SetActive(false);

        if (warningUI != null) 
            warningUI.SetActive(false);

        if (warningText != null)
            warningText.text = "";

        SetMissionGuideVisible(false);

        UpdateUI();
    }

    void Start()
    {
        int mode = 0;
        if (DataManager.Instance != null) mode = DataManager.Instance.selectedMode;

        EnemyManager em = FindFirstObjectByType<EnemyManager>();
        if (em != null) currentStageIndex = em.CurrentStageIndex;

        SetMissionGuide(mode);
        SetMissionGuideVisible(true);
    }

    void Update()
    {
        HandleMissionGuideInput();
    }

    private void HandleMissionGuideInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleMissionGuideVisible();
        }
    }

    private void ToggleMissionGuideVisible()
    {
        bool current = IsMissionGuideVisible();
        SetMissionGuideVisible(!current);
    }

    private bool IsMissionGuideVisible()
    {
        if (missionGuidePanel != null) return missionGuidePanel.activeSelf;
        if (missionGuideText != null) return missionGuideText.gameObject.activeSelf;
        return false;
    }

    private void SetMissionGuideVisible(bool visible)
    {

        if (currentStageIndex == 0)
        {
            if (missionGuideText != null)
                missionGuideText.text = "Tutorial: Learn how to move/shoot/reload and then get to the escape hatch. (Tab: Toggle guidance)";

            if (missionGuideImage != null)
                missionGuideImage.enabled = false;

            return;
        }

        if (missionGuidePanel != null)
        {
            if (missionGuidePanel.activeSelf != visible)
                missionGuidePanel.SetActive(visible);
            return;
        }

        if (missionGuideText != null)
        {
            if (missionGuideText.gameObject.activeSelf != visible)
                missionGuideText.gameObject.SetActive(visible);
        }
    }

    public void SetWarning(bool isActive, string message=null)
    {
        if (warningUI != null && warningUI.activeSelf != isActive)
        {
            warningUI.SetActive(isActive);
        }

        if (warningText == null) return;

        warningText.text = isActive ? (message != null ? message : "") : "";
    }

    public void ResetSession()
    {
        DataManager.ResetCurrentScore();
        CurrentAmmo = 0;
        TotalClips = 0;
        MaxAmmo = 0;
    }

    //PlayerShooter에서 총알 소비 시 호출
    public void ConsumeAmmo(int currentAmmo, int totalClips, WeaponData data)
    {
        this.CurrentAmmo = currentAmmo;
        this.TotalClips = totalClips;
        if (data != null) this.MaxAmmo = data.maxAmmo;
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        int mode = 0;
        if (DataManager.Instance != null) mode = DataManager.Instance.selectedMode;

        if (mode == 0) amount = -Mathf.Abs(amount);
        else amount = Mathf.Abs(amount);

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

            if (warningUI != null) warningUI.SetActive(false);

            if (warningText != null) warningText.text = "";

            SetMissionGuideVisible(false);
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

    public void SetMissionGuide(int mode)
    {
        if (missionGuideText == null) return;

        if (mode == 0) //정찰
            missionGuideText.text = "Objective: Reach the target point without being detected.";
        else if (mode == 1) //섬멸
            missionGuideText.text = "Objective: Eliminate all enemies within the area.";
        else if (mode == 2) //무한
            missionGuideText.text = "Objective: Survive as long as possible and score points.";
    }
    public void AddObjectiveScore(int amount)
    {
        amount = Mathf.Abs(amount);
        DataManager.AddScore(amount);
        UpdateUI();
    }
}

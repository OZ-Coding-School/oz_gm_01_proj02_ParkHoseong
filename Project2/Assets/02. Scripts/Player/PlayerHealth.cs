using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHealth : HealthBase
{
    private ScoreManager scoreManager;
    protected override void Awake()
    {
        base.Awake(); //부모의 체력 초기화 실행
        scoreManager = FindObjectOfType<ScoreManager>();
        UpdateUI();
    }

    public override void TakeDamage(int amount, bool isHeadShot = false)
    {
        base.TakeDamage(amount, isHeadShot);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreManager != null)
            scoreManager.UpdateHealthUI(currentHealth, maxHealth);
    }

    protected override void Die()
    {
        isDead = true;
        Debug.Log("Player Died!");
        //여기서 게임오버 팝업 등을 띄움

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (scoreManager != null)
            scoreManager.ShowGameOver();
    }
}
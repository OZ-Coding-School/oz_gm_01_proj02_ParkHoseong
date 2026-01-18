using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHealth : HealthBase
{
    [SerializeField] private TMPro.TextMeshProUGUI healthText;

    protected override void Awake()
    {
        base.Awake(); //부모의 체력 초기화 실행
        UpdateUI();
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (healthText != null)
            healthText.text = $"HP: {currentHealth}/{maxHealth}";
    }

    protected override void Die()
    {
        isDead = true;
        Debug.Log("Player Died!");
        //여기서 게임오버 팝업 등을 띄움
    }
}
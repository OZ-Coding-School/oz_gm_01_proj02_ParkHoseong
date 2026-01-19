using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HealthBase : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] protected int maxHealth = 100;
    protected int currentHealth;
    protected bool isDead = false;

    public bool isHeadShot { get; protected set; }

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int amount, bool isHeadShot = false)
    {
        if (isDead) return;

        this.isHeadShot = isHeadShot;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public virtual void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    protected abstract void Die();
}

using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // Whether player can take damage.
    // 玩家是否可以受伤
    public bool canTakeDamage = true;

    // Invincible time after taking damage.
    // 受伤后的短暂无敌时间
    public float invincibleTime = 0.5f;

    public static event System.Action OnDeath;
    public static event System.Action OnPlayerHit;

    // Current invincible timer.
    // 当前无敌计时器
    private float invincibleTimer = 0f;

    private bool isDead = false;

    private void Update()
    {
        UpdateInvincibleTimer();
    }

    private void UpdateInvincibleTimer()
    {
        if (invincibleTimer > 0f)
        {
            invincibleTimer -= Time.deltaTime;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead)
        {
            return;
        }

        if (!canTakeDamage)
        {
            return;
        }

        if (damageAmount <= 0)
        {
            return;
        }

        if (invincibleTimer > 0f)
        {
            return;
        }

        if (GlobalRunManager.Instance == null)
        {
            Debug.Log("GlobalRunManager was not found.");
            return;
        }

        GlobalRunManager.Instance.DamagePlayer(damageAmount);

        invincibleTimer = invincibleTime;
        OnPlayerHit?.Invoke();

        if (GlobalRunManager.Instance.currentHealth <= 0)
        {
            isDead = true;
            OnDeath?.Invoke();
        }
    }

    public void Heal(int healAmount)
    {
        if (healAmount <= 0)
        {
            return;
        }

        if (GlobalRunManager.Instance == null)
        {
            Debug.Log("GlobalRunManager was not found.");
            return;
        }

        GlobalRunManager.Instance.HealPlayer(healAmount);
    }

    public void ResetDeath()
    {
        isDead = false;
    }
}
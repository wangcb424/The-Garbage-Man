using System.Collections;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    // 当前燃烧效果
    private Coroutine burnCoroutine;

    // 当前减速效果
    private Coroutine slowCoroutine;
    // 原始移动速度
    private float originalMoveSpeed;

    // 给玩家施加燃烧效果
    public void ApplyBurn(int damagePerTick, float duration, float tickInterval)
    {
        if (burnCoroutine != null)
        {
            StopCoroutine(burnCoroutine);
        }
        burnCoroutine = StartCoroutine(BurnRoutine(damagePerTick, duration, tickInterval));
    }

    // 燃烧持续伤害
    private IEnumerator BurnRoutine(int damagePerTick, float duration, float tickInterval)
    {
        float timer = 0f;
        while (timer < duration)
        {
            if (GlobalRunManager.Instance != null)
            {
                GlobalRunManager.Instance.DamagePlayer(damagePerTick);
            }
            timer += tickInterval;
            yield return new WaitForSeconds(tickInterval);
        }
        burnCoroutine = null;
    }

    
    // 施加减速效果
    public void ApplySlow(float speedMultiplier, float duration)
    {
        if (GlobalRunManager.Instance == null)
        {
            return;
        }
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
            RestoreMoveSpeed();
        }
        slowCoroutine = StartCoroutine(SlowRoutine(speedMultiplier, duration));
    }

    private IEnumerator SlowRoutine(float speedMultiplier, float duration)
    {
        originalMoveSpeed = GlobalRunManager.Instance.moveSpeed;
        GlobalRunManager.Instance.moveSpeed = originalMoveSpeed * speedMultiplier;

        yield return new WaitForSeconds(duration);

        RestoreMoveSpeed();
        slowCoroutine = null;
    }

    // 恢复玩家移动速度
    private void RestoreMoveSpeed()
    {
        if (GlobalRunManager.Instance == null)
        {
            return;
        }
        GlobalRunManager.Instance.moveSpeed = originalMoveSpeed;
    }
}
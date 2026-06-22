using UnityEngine;

public class EnemyHitResponder : MonoBehaviour
{
    private HitFlash hitFlash;
    private EnemyCore enemyCore;

    private void Awake()
    {
        hitFlash = GetComponent<HitFlash>();
        enemyCore = GetComponent<EnemyCore>();
    }

    private void OnEnable()
    {
        if (enemyCore != null)
        {
            enemyCore.OnHit += HandleHit;
        }
    }

    private void OnDisable()
    {
        if (enemyCore != null)
        {
            enemyCore.OnHit -= HandleHit;
        }
    }

    private void HandleHit(int damage)
    {
        if (hitFlash != null)
        {
            hitFlash.Flash();
        }
    }
}
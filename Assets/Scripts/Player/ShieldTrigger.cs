using UnityEngine;

public class ShieldTrigger : MonoBehaviour
{
    private PlayerParry playerParry;
    private PlayerBlock playerBlock;
    private PlayerHealth playerHealth;
    public static event System.Action OnBlockHit;

    private void Awake()
    {
        playerParry = GetComponentInParent<PlayerParry>();
        playerBlock = GetComponentInParent<PlayerBlock>();
        playerHealth = GetComponentInParent<PlayerHealth>();
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyBullet enemyBullet = other.GetComponentInParent<EnemyBullet>();

        if (enemyBullet == null)
        {
            return;
        }

        if (playerParry != null && playerParry.isParrying)
        {
            enemyBullet.GetParried();
            playerParry.TriggerParrySuccess();
            return;
        }

        if (playerBlock != null && playerBlock.isBlocking)
        {
            int reducedDamage = enemyBullet.GetBlocked(enemyBullet.damage);

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(reducedDamage);
            }
            
            OnBlockHit?.Invoke();
            return;
        }
    }
}
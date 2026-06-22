using UnityEngine;

public class EnemyTouchDamageWeapon : EnemyWeaponModule
{
    // Damage dealt to player.
    // 碰到玩家时造成的伤害
    public int touchDamage = 1;

    // If true, enemy dies after touching player.
    // 如果为 true，碰到玩家后敌人死亡
    public bool dieAfterTouch = false;

    public override void OnEnemyTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        DamagePlayer(other.gameObject);
    }

    public override void OnEnemyCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        DamagePlayer(collision.gameObject);
    }

    private void DamagePlayer(GameObject playerObject)
    {
        PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            playerHealth = playerObject.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(touchDamage);
        }
        else if (GlobalRunManager.Instance != null)
        {
            GlobalRunManager.Instance.DamagePlayer(touchDamage);
        }

        if (dieAfterTouch && enemyCore != null)
        {
            enemyCore.Die();
        }
    }
}
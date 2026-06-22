using UnityEngine;

public class EnemyWeapon : EnemyWeaponModule
{
    // Bullet prefab fired by this weapon.
    // 这个武器发射的子弹 prefab
    public GameObject bulletPrefab;

    // Bullet movement speed.
    // 子弹速度
    public float bulletSpeed = 10f;

    // Time between each attack.
    // 每次攻击之间的间隔
    public float attackInterval = 1.5f;

    // Enemy only shoots when player is within this range.
    // 玩家在这个范围内时敌人才会射击
    public float attackRange = 18f;

    // Fire point of this weapon.
    // 武器发射点
    public Transform firePoint;

    // If true, enemy faces player before shooting.
    // 如果为 true，射击前让敌人朝向玩家
    public bool facePlayerWhenShooting = true;

    // If true, this weapon will shoot automatically.
    // 如果为 true，这个武器会自动攻击
    public bool autoFire = true;

    // Attack timer.
    // 攻击计时器
    private float attackTimer = 0f;

    public override void Initialize(EnemyCore newEnemyCore)
    {
        base.Initialize(newEnemyCore);

        // Start with a small random delay so enemies do not all shoot at the same time.
        // 开始时加一点随机延迟，避免所有敌人同时射击
        attackTimer = Random.Range(0f, attackInterval);
    }

    public override void TickWeapon()
    {
        if (!autoFire)
        {
            return;
        }

        if (enemyCore == null || enemyCore.PlayerTarget == null)
        {
            return;
        }

        if (bulletPrefab == null)
        {
            return;
        }

        Vector3 directionToPlayer = enemyCore.PlayerTarget.position - enemyCore.transform.position;
        directionToPlayer.y = 0f;

        float distanceToPlayer = directionToPlayer.magnitude;

        // Do not shoot if player is too far away.
        // 如果玩家太远，就不射击
        if (distanceToPlayer > attackRange)
        {
            return;
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
        {
            return;
        }

        attackTimer = attackInterval;

        FireWeapon();
    }

    public void FireWeapon()
    {
        if (enemyCore == null || enemyCore.PlayerTarget == null)
        {
            return;
        }

        if (bulletPrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = transform.position + transform.forward * 0.5f + Vector3.up * 0.2f;

        // Use fire point if assigned.
        // 如果设置了 firePoint，就从 firePoint 发射
        if (firePoint != null)
        {
            spawnPosition = firePoint.position;
        }

        Vector3 shootDirection = enemyCore.PlayerTarget.position - spawnPosition;
        shootDirection.y = 0f;

        if (shootDirection.sqrMagnitude <= 0.001f)
        {
            return;
        }

        shootDirection.Normalize();

        if (facePlayerWhenShooting)
        {
            enemyCore.transform.rotation = Quaternion.LookRotation(shootDirection);
        }

        GameObject bulletObject = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);

        EnemyBullet bullet = bulletObject.GetComponent<EnemyBullet>();

        if (bullet != null)
        {
            bullet.Launch(shootDirection, bulletSpeed);
        }
    }
}
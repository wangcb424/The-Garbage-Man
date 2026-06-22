using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    // Bullet prefab fired by this weapon.
    // 这个武器发射的子弹 prefab
    public GameObject bulletPrefab;

    // Bullet movement speed.
    // 子弹速度
    public float bulletSpeed = 14f;

    // Time between each attack.
    // 每次攻击之间的间隔
    public float attackInterval = 0.4f;

    // Weapon can only target enemies within this range.
    // 武器只能锁定这个范围内的敌人
    public float targetRange = 18f;

    // Fire point of this weapon.
    // 武器发射点
    public Transform firePoint;

    // If true, holding left mouse button keeps firing.
    // 如果为 true，按住左键会持续开火
    public bool holdToFire = true;

    // If true, weapon automatically rotates toward nearest enemy.
    // 如果为 true，武器会自动朝最近敌人旋转
    public bool autoAim = true;

    // How fast the weapon rotates toward target.
    // 武器朝目标旋转的速度
    public float aimRotateSpeed = 12f;

    [Header("Target Marker")]

    // Marker prefab shown above the current target.
    // 显示在当前目标头上的瞄准图标 prefab
    public GameObject targetMarkerPrefab;

    // Marker offset above enemy.
    // 图标在敌人头顶的偏移
    public Vector3 targetMarkerOffset = new Vector3(0f, 2.2f, 0f);

    public event System.Action OnWeaponShoot;

    // Attack timer.
    // 攻击计时器
    private float attackTimer = 0f;

    // Player weapon holder that owns this weapon.
    // 拥有这个武器的 PlayerWeaponHolder
    protected PlayerWeaponHolder weaponHolder;

    // Current target enemy.
    // 当前锁定的敌人
    private EnemyCore currentTarget;

    // Previous target enemy.
    // 上一帧锁定的敌人
    private EnemyCore previousTarget;

    // Current target marker object.
    // 当前瞄准图标物体
    private GameObject currentTargetMarkerObject;

    // Current target marker script.
    // 当前瞄准图标脚本
    private TargetMarker currentTargetMarker;

    // PlayerShoot object, 使用new input system
    private PlayerShoot playerShoot;

    public static event System.Action OnShoot;

    public void Initialize(PlayerWeaponHolder newWeaponHolder)
    {
        // Store weapon holder.
        // 保存武器持有者
        weaponHolder = newWeaponHolder;
        playerShoot = newWeaponHolder.GetComponent<PlayerShoot>();
    }

    public void TickWeapon()
    {
        // Count down attack timer.
        // 攻击冷却倒计时
        attackTimer -= Time.deltaTime;

        // Find the nearest enemy every frame.
        // 每帧寻找最近的敌人
        currentTarget = FindNearestEnemy();

        // Update marker when target changes.
        // 目标变化时更新瞄准图标
        UpdateTargetMarker();

        // // Rotate weapon toward target.
        // // 让武器朝向目标
        // if (autoAim && currentTarget != null)
        // {
        //     AimAtTarget(currentTarget.transform.position);
        // }

        bool wantsToFire = false;

        // Left mouse button controls firing only.
        // 左键只负责开火
        if (playerShoot != null)
        {
            if (holdToFire)
            {
                wantsToFire = playerShoot.isFireHeld;
            }
            else
            {
                wantsToFire = playerShoot.isFirePressed;
            }
        }

        if (!wantsToFire)
        {
            return;
        }

        TryFire();
    }

    private EnemyCore FindNearestEnemy()
    {
        // Find all active enemies in the scene.
        // 找到场景里所有启用中的敌人
        EnemyCore[] enemies = FindObjectsByType<EnemyCore>();

        EnemyCore nearestEnemy = null;
        float nearestDistance = targetRange;

        Vector3 weaponPosition = transform.position;

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null)
            {
                continue;
            }

            Vector3 directionToEnemy = enemies[i].transform.position - weaponPosition;
            directionToEnemy.y = 0f;

            float distanceToEnemy = directionToEnemy.magnitude;

            if (distanceToEnemy < nearestDistance)
            {
                nearestDistance = distanceToEnemy;
                nearestEnemy = enemies[i];
            }
        }

        return nearestEnemy;
    }

    private void UpdateTargetMarker()
    {
        // If target did not change, do nothing.
        // 如果目标没有变化，不需要更新
        if (currentTarget == previousTarget)
        {
            return;
        }

        // Remove old marker.
        // 删除旧图标
        if (currentTargetMarkerObject != null)
        {
            Destroy(currentTargetMarkerObject);
            currentTargetMarkerObject = null;
            currentTargetMarker = null;
        }

        previousTarget = currentTarget;

        // If there is no target, no marker is needed.
        // 如果没有目标，就不显示图标
        if (currentTarget == null)
        {
            return;
        }

        if (targetMarkerPrefab == null)
        {
            return;
        }

        // Create marker above current target.
        // 在当前目标头上生成图标
        currentTargetMarkerObject = Instantiate(
            targetMarkerPrefab,
            currentTarget.transform.position + targetMarkerOffset,
            Quaternion.identity
        );

        currentTargetMarker = currentTargetMarkerObject.GetComponent<TargetMarker>();

        if (currentTargetMarker != null)
        {
            currentTargetMarker.offset = targetMarkerOffset;
            currentTargetMarker.SetTarget(currentTarget.transform);
        }
    }

    private void AimAtTarget(Vector3 targetPosition)
    {
        Vector3 directionToTarget = targetPosition - transform.position;
        directionToTarget.y = 0f;

        if (directionToTarget.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget.normalized);

        // Smoothly rotate weapon toward enemy.
        // 平滑转向敌人
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            aimRotateSpeed * Time.deltaTime
        );
    }

    private void TryFire()
    {
        if (bulletPrefab == null)
        {
            return;
        }

        if (autoAim && currentTarget == null)
        {
            return;
        }

        if (attackTimer > 0f)
        {
            return;
        }

        attackTimer = GetFinalAttackInterval();

        FireWeapon();
    }

    private float GetFinalAttackInterval()
    {
        float attackSpeedMultiplier = 1f;

        if (GlobalRunManager.Instance != null)
        {
            attackSpeedMultiplier = GlobalRunManager.Instance.attackSpeedMultiplier;
        }

        if (attackSpeedMultiplier <= 0f)
        {
            attackSpeedMultiplier = 1f;
        }

        // Higher attack speed means shorter attack interval.
        // 攻速越高，攻击间隔越短
        return attackInterval / attackSpeedMultiplier;
    }

    protected virtual void FireWeapon()
    {
        if (currentTarget == null && autoAim)
        {
            return;
        }

        Vector3 spawnPosition = transform.position + transform.forward * 0.5f + Vector3.up * 0.2f;

        if (firePoint != null)
        {
            spawnPosition = firePoint.position;
        }

        Vector3 shootDirection;

        if (autoAim && currentTarget != null)
        {
            shootDirection = currentTarget.transform.position - spawnPosition;
        }
        else
        {
            shootDirection = weaponHolder.GetPlayerTransform().forward;
        }

        shootDirection.y = 0f;

        if (shootDirection.sqrMagnitude <= 0.001f)
        {
            return;
        }

        shootDirection.Normalize();

        GameObject bulletObject = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        InvokeOnShoot();

        PlayerBullet bullet = bulletObject.GetComponent<PlayerBullet>();

        if (bullet != null)
        {
            bullet.Launch(shootDirection, bulletSpeed);
        }
    }

    private void OnDestroy()
    {
        // Remove target marker when weapon is destroyed.
        // 武器被删除时，也删除瞄准图标
        if (currentTargetMarkerObject != null)
        {
            Destroy(currentTargetMarkerObject);
        }
    }

    protected void InvokeOnShoot()
    {
        OnShoot?.Invoke();
        OnWeaponShoot?.Invoke();
    }
}
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public enum BulletElementType
    {
        Normal,
        Fire,
        Ice,
        Lightning,
        Poison,
        Slow,
        Knockback
    }

    [Header("Bullet Stats")]

    // Current bullet element type.
    // 当前子弹属性
    public BulletElementType bulletElementType = BulletElementType.Normal;

    // Bullet damage value.
    // 子弹伤害
    public int damage = 1;

    // Extra effect duration.
    // 特殊效果持续时间，先预留
    public float effectDuration = 2f;

    // How long the bullet exists.
    // 子弹存在时间
    public float lifeTime = 5f;

    [Header("Collision Settings")]

    // How many times this bullet can bounce from bounce walls.
    // 子弹最多可以从反弹墙反弹几次
    public int maxBounceCount = 3;

    // If true, this bullet is destroyed when it hits player.
    // 如果为 true，碰到玩家后删除子弹
    public bool destroyOnPlayerHit = true;

    // If true, this bullet is destroyed when it hits normal walls.
    // 如果为 true，碰到普通墙后删除子弹
    public bool destroyOnNormalWallHit = true;

    // If true, this bullet ignores all enemy colliders.
    // 如果为 true，子弹会穿过所有敌人
    public bool ignoreEnemyCollisions = true;

    // If true, this bullet ignores other enemy bullets.
    // 如果为 true，子弹会穿过其他敌人子弹
    public bool ignoreOtherEnemyBullets = true;

    private Rigidbody bulletRigidbody;
    private Collider bulletCollider;

    private float moveSpeed = 10f;
    private Vector3 moveDirection;
    private int currentBounceCount = 0;
    private bool isLaunched = false;

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();

        if (bulletRigidbody == null)
        {
            bulletRigidbody = gameObject.AddComponent<Rigidbody>();
        }

        bulletCollider = GetComponent<Collider>();

        bulletRigidbody.useGravity = false;
        bulletRigidbody.isKinematic = false;
        bulletRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void Start()
    {
        IgnoreEnemyAndEnemyBulletCollisions();
    }

    public void Launch(Vector3 direction, float speed)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;
        isLaunched = true;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }

        IgnoreEnemyAndEnemyBulletCollisions();

        bulletRigidbody.linearVelocity = moveDirection * moveSpeed;

        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        if (!isLaunched)
        {
            return;
        }

        bulletRigidbody.linearVelocity = moveDirection * moveSpeed;
    }

    private void IgnoreEnemyAndEnemyBulletCollisions()
    {
        if (bulletCollider == null)
        {
            return;
        }

        if (ignoreEnemyCollisions)
        {
            EnemyCore[] enemies = FindObjectsByType<EnemyCore>();

            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] == null)
                {
                    continue;
                }

                Collider[] enemyColliders = enemies[i].GetComponentsInChildren<Collider>();

                for (int j = 0; j < enemyColliders.Length; j++)
                {
                    if (enemyColliders[j] == null)
                    {
                        continue;
                    }

                    Physics.IgnoreCollision(bulletCollider, enemyColliders[j], true);
                }
            }
        }

        if (ignoreOtherEnemyBullets)
        {
            EnemyBullet[] enemyBullets = FindObjectsByType<EnemyBullet>();

            for (int i = 0; i < enemyBullets.Length; i++)
            {
                if (enemyBullets[i] == null)
                {
                    continue;
                }

                if (enemyBullets[i] == this)
                {
                    continue;
                }

                Collider otherBulletCollider = enemyBullets[i].GetComponent<Collider>();

                if (otherBulletCollider != null)
                {
                    Physics.IgnoreCollision(bulletCollider, otherBulletCollider, true);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ShouldIgnoreHit(other.gameObject))
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            HitPlayer(other.gameObject);
            return;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (ShouldIgnoreHit(collision.gameObject))
        {
            // Keep bullet moving if Unity still sends a collision event.
            // 如果 Unity 还是发出了碰撞事件，继续保持子弹速度
            bulletRigidbody.linearVelocity = moveDirection * moveSpeed;
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            HitPlayer(collision.gameObject);
            return;
        }

        DungeonWallTile wallTile = collision.gameObject.GetComponent<DungeonWallTile>();

        if (wallTile == null)
        {
            wallTile = collision.gameObject.GetComponentInParent<DungeonWallTile>();
        }

        if (wallTile != null)
        {
            HandleWallCollision(collision, wallTile);
            return;
        }

        Destroy(gameObject);
    }

    private void HitPlayer(GameObject playerObject)
    {
        PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            playerHealth = playerObject.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
        else if (GlobalRunManager.Instance != null)
        {
            GlobalRunManager.Instance.DamagePlayer(damage);
        }

        // 应用子弹效果
        PlayerStatus playerStatus = playerObject.GetComponent<PlayerStatus>();

        if (playerStatus == null)
        {
            playerStatus = playerObject.GetComponentInParent<PlayerStatus>();
        }

        if (playerStatus != null)
        {
            ApplyElementEffect(playerStatus);
        }

        if (destroyOnPlayerHit)
        {
            Destroy(gameObject);
        }
    }

    private void ApplyElementEffect(PlayerStatus playerStatus)
    {
        // 火焰子弹造成持续燃烧伤害
        if (bulletElementType == BulletElementType.Fire)
        {
            playerStatus.ApplyBurn(1, effectDuration, 1f);
            return;
        }

        // 冰霜子弹降低玩家移动速度
        if (bulletElementType == BulletElementType.Ice)
        {
            playerStatus.ApplySlow(0.5f, effectDuration);
             return;
        }
    }

    private bool ShouldIgnoreHit(GameObject hitObject)
    {
        if (hitObject == null)
        {
            return true;
        }

        if (hitObject == gameObject)
        {
            return true;
        }

        // Enemy bullets should pass through all enemies.
        // 敌人子弹穿过所有敌人
        if (ignoreEnemyCollisions && hitObject.GetComponentInParent<EnemyCore>() != null)
        {
            return true;
        }

        // Enemy bullets should pass through other enemy bullets.
        // 敌人子弹穿过其他敌人子弹
        if (ignoreOtherEnemyBullets && hitObject.GetComponentInParent<EnemyBullet>() != null)
        {
            return true;
        }

        return false;
    }

    private void HandleWallCollision(Collision collision, DungeonWallTile wallTile)
    {
        if (wallTile.wallType == DungeonWallTile.WallType.Bounce)
        {
            BounceFromWall(collision, wallTile);
            return;
        }

        if (destroyOnNormalWallHit)
        {
            Destroy(gameObject);
        }
    }

    private void BounceFromWall(Collision collision, DungeonWallTile wallTile)
    {
        if (collision.contacts.Length == 0)
        {
            Destroy(gameObject);
            return;
        }

        currentBounceCount++;

        if (currentBounceCount > maxBounceCount)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 wallNormal = collision.contacts[0].normal;

        moveDirection = Vector3.Reflect(moveDirection, wallNormal).normalized;

        moveSpeed = wallTile.bounceForce;

        transform.position += moveDirection * 0.1f;

        bulletRigidbody.linearVelocity = moveDirection * moveSpeed;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    public void GetParried()
    {
        Destroy(gameObject);
    }
    public int GetBlocked(int incomingDamage)
    {
        float reduction = 0.5f;

        if (GlobalRunManager.Instance != null)
        {
            reduction = GlobalRunManager.Instance.blockDamageReduction;
        }

        return Mathf.RoundToInt(incomingDamage * (1f - reduction));
    }
}
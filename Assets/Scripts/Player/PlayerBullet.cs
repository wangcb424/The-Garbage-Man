using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    // Bullet attribute type.
    // 子弹属性类型，先预留
    public enum BulletElementType
    {
        Normal,
    }

    // Current bullet element type.
    // 当前子弹属性
    public BulletElementType bulletElementType = BulletElementType.Normal;

    // Bullet damage.
    // 子弹伤害
    public int damage = 1;

    // Extra effect duration.
    // 特殊效果持续时间，先预留
    public float effectDuration = 2f;

    // How long the bullet exists.
    // 子弹存在时间
    public float lifeTime = 5f;

    // How many times this bullet can bounce from bounce walls.
    // 子弹最多可以反弹几次
    public int maxBounceCount = 3;

    // If true, bullet is destroyed after hitting enemy.
    // 如果为 true，打到敌人后销毁
    public bool destroyOnEnemyHit = true;

    // If true, bullet is destroyed when hitting normal walls.
    // 如果为 true，打到普通墙后销毁
    public bool destroyOnNormalWallHit = true;

    // If true, player bullet destroys enemy bullet when touching it.
    // 如果为 true，玩家子弹碰到敌人子弹时会抵消
    public bool destroyEnemyBulletOnHit = false;

    // Rigidbody used for bullet movement.
    // 用来移动子弹的 Rigidbody
    private Rigidbody bulletRigidbody;

    // Current bullet speed.
    // 当前子弹速度
    private float moveSpeed = 14f;

    // Current movement direction.
    // 当前移动方向
    private Vector3 moveDirection;

    // Current bounce count.
    // 当前反弹次数
    private int currentBounceCount = 0;

    // Whether this bullet has been launched.
    // 子弹是否已经发射
    private bool isLaunched = false;

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();

        if (bulletRigidbody == null)
        {
            bulletRigidbody = gameObject.AddComponent<Rigidbody>();
        }

        bulletRigidbody.useGravity = false;
        bulletRigidbody.isKinematic = false;
        bulletRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
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

    private void OnTriggerEnter(Collider other)
    {
        if (ShouldIgnoreHit(other.gameObject))
        {
            return;
        }

        EnemyCore enemyCore = other.GetComponentInParent<EnemyCore>();

        if (enemyCore != null)
        {
            HitEnemy(enemyCore);
            return;
        }

        EnemyBullet enemyBullet = other.GetComponentInParent<EnemyBullet>();

        if (enemyBullet != null)
        {
            HitEnemyBullet(enemyBullet);
            return;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (ShouldIgnoreHit(collision.gameObject))
        {
            return;
        }

        EnemyCore enemyCore = collision.gameObject.GetComponentInParent<EnemyCore>();

        if (enemyCore != null)
        {
            HitEnemy(enemyCore);
            return;
        }

        EnemyBullet enemyBullet = collision.gameObject.GetComponentInParent<EnemyBullet>();

        if (enemyBullet != null)
        {
            HitEnemyBullet(enemyBullet);
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

    private bool ShouldIgnoreHit(GameObject hitObject)
    {
        if (hitObject == null)
        {
            return true;
        }

        // Ignore this bullet itself.
        // 忽略自己
        if (hitObject == gameObject)
        {
            return true;
        }

        // Ignore player and player's child objects.
        // 忽略 Player 和 Player 的子物体
        if (hitObject.CompareTag("Player"))
        {
            return true;
        }

        if (hitObject.GetComponentInParent<PlayerWeaponHolder>() != null)
        {
            return true;
        }

        // Ignore other player bullets.
        // 忽略其他玩家子弹
        if (hitObject.GetComponentInParent<PlayerBullet>() != null)
        {
            return true;
        }

        return false;
    }

    private void HitEnemy(EnemyCore enemyCore)
    {
        int finalDamage = GetFinalDamage();

        enemyCore.TakeDamage(finalDamage);

        if (destroyOnEnemyHit)
        {
            Destroy(gameObject);
        }
    }

    private int GetFinalDamage()
    {
        int finalDamage = damage;

        if (GlobalRunManager.Instance != null)
        {
            float critChance = GlobalRunManager.Instance.critChance;

            if (Random.value < critChance)
            {
                finalDamage *= 2;
                Debug.Log("Critical hit!");
            }
        }

        return finalDamage;
    }

    private void HitEnemyBullet(EnemyBullet enemyBullet)
    {
        if (!destroyEnemyBulletOnHit)
        {
            return;
        }

        Destroy(enemyBullet.gameObject);
        Destroy(gameObject);
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
}
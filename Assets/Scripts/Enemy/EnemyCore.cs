using System.Collections.Generic;
using UnityEngine;

public class EnemyCore : MonoBehaviour
{
    // Enemy max health.
    // 敌人最大生命值
    public int maxHealth = 3;

    // If true, enemy will destroy itself when touching player if no weapon module exists.
    // 如果没有武器模块，碰到玩家时是否默认删除自己
    public bool destroyOnPlayerTouchIfNoWeapon = true;

    // Where the weapon prefab will be attached.
    // 武器 prefab 会挂到这个位置
    public Transform weaponSocket;

    // Movement modules this enemy can randomly use.
    // 这个敌人可以随机使用的移动模块
    public List<GameObject> movementModulePrefabs = new List<GameObject>();

    // Weapon modules this enemy can randomly use.
    // 这个敌人可以随机使用的武器模块
    public List<GameObject> weaponModulePrefabs = new List<GameObject>();

    // Current enemy health.
    // 当前敌人生命值
    private int currentHealth;

    // Whether this enemy is already dying.
    // 防止死亡逻辑重复执行
    private bool isDead = false;

    // Player target.
    // 玩家目标
    public Transform PlayerTarget { get; private set; }

    // Current movement module.
    // 当前移动模块
    private EnemyMovementModule movementModule;

    // Current weapon module.
    // 当前武器模块
    private EnemyWeaponModule weaponModule;

    public static event System.Action<Vector3, int> OnEnemyHit;
    public static event System.Action<Vector3> OnEnemyDeath;
    public event System.Action<int> OnHit;
    public event System.Action OnDied;

    private void Awake()
    {
        // Set current health.
        // 设置当前生命值
        currentHealth = maxHealth;
    }

    private void Start()
    {
        // Find player when enemy starts.
        // 敌人开始时寻找玩家
        FindPlayer();
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        // If player is missing, try to find player again.
        // 如果玩家引用丢失，就重新寻找玩家
        if (PlayerTarget == null)
        {
            FindPlayer();
        }

        // Run current movement module.
        // 运行当前移动模块
        if (movementModule != null)
        {
            movementModule.TickMovement();
        }

        // Run current weapon module.
        // 运行当前武器模块
        if (weaponModule != null)
        {
            weaponModule.TickWeapon();
        }
    }

    public void AssignRandomModules()
    {
        // Assign movement from this enemy's own list.
        // 从这个 enemy 自己的 list 里分配移动模块
        AssignRandomMovementModule();

        // Assign weapon from this enemy's own list.
        // 从这个 enemy 自己的 list 里分配武器模块
        AssignRandomWeaponModule();
    }

    private void AssignRandomMovementModule()
    {
        GameObject movementPrefab = GetRandomPrefab(movementModulePrefabs);

        if (movementPrefab == null)
        {
            Debug.Log(gameObject.name + " has no movement module prefab assigned.");
            return;
        }

        // Movement modules are attached directly to the enemy root.
        // 移动模块直接挂到 enemy 根物体下面
        GameObject movementObject = Instantiate(movementPrefab, transform);
        movementObject.name = movementPrefab.name;

        movementObject.transform.localPosition = Vector3.zero;
        movementObject.transform.localRotation = Quaternion.identity;
        movementObject.transform.localScale = Vector3.one;

        movementModule = movementObject.GetComponent<EnemyMovementModule>();

        if (movementModule != null)
        {
            movementModule.Initialize(this);
        }
        else
        {
            Debug.Log(movementPrefab.name + " does not have EnemyMovementModule.");
        }
    }

    private void AssignRandomWeaponModule()
    {
        GameObject weaponPrefab = GetRandomPrefab(weaponModulePrefabs);

        if (weaponPrefab == null)
        {
            return;
        }

        // Attach weapon to weapon socket if it exists.
        // 如果有 weaponSocket，就把武器挂到 weaponSocket 上
        Transform weaponParent = transform;

        if (weaponSocket != null)
        {
            weaponParent = weaponSocket;
        }

        GameObject weaponObject = Instantiate(weaponPrefab, weaponParent);
        weaponObject.name = weaponPrefab.name;

        // Reset local transform so the weapon follows the socket.
        // 重置本地坐标，让武器对齐 socket
        weaponObject.transform.localPosition = Vector3.zero;
        weaponObject.transform.localRotation = Quaternion.identity;
        weaponObject.transform.localScale = Vector3.one;

        weaponModule = weaponObject.GetComponent<EnemyWeaponModule>();

        if (weaponModule != null)
        {
            weaponModule.Initialize(this);
        }
        else
        {
            Debug.Log(weaponPrefab.name + " does not have EnemyWeaponModule.");
        }
    }

    private GameObject GetRandomPrefab(List<GameObject> prefabs)
    {
        if (prefabs == null || prefabs.Count == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, prefabs.Count);
        return prefabs[randomIndex];
    }

    private void FindPlayer()
    {
        // Find player by Player tag.
        // 通过 Player tag 寻找玩家
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            PlayerTarget = playerObject.transform;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead)
        {
            return;
        }

        // Reduce health.
        // 扣除生命值
        currentHealth -= damageAmount;
        OnHit?.Invoke(damageAmount);
        OnEnemyHit?.Invoke(transform.position, damageAmount);

        Debug.Log(gameObject.name + " health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        EnemyDropTable dropTable = GetComponent<EnemyDropTable>();

        OnDied?.Invoke();
        OnEnemyDeath?.Invoke(transform.position);

        if (dropTable != null)
        {
            dropTable.DropResources();
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead)
        {
            return;
        }

        // Let weapon module handle trigger if it exists.
        // 如果有武器模块，就让武器模块处理 trigger
        if (weaponModule != null)
        {
            weaponModule.OnEnemyTriggerEnter(other);
            return;
        }

        // Fallback behavior when there is no weapon module.
        // 没有武器模块时使用默认行为
        if (destroyOnPlayerTouchIfNoWeapon && other.CompareTag("Player"))
        {
            Die();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead)
        {
            return;
        }

        // Let weapon module handle collision if it exists.
        // 如果有武器模块，就让武器模块处理 collision
        if (weaponModule != null)
        {
            weaponModule.OnEnemyCollisionEnter(collision);
            return;
        }

        // Fallback behavior when there is no weapon module.
        // 没有武器模块时使用默认行为
        if (destroyOnPlayerTouchIfNoWeapon && collision.gameObject.CompareTag("Player"))
        {
            Die();
        }
    }
}
using UnityEngine;

public class ResourcePickup : MonoBehaviour
{
    public enum ResourceType
    {
        Gold,
        Trash
    }

    // What type of resource this pickup gives.
    // 这个掉落物给玩家什么资源
    public ResourceType resourceType = ResourceType.Gold;

    // How much resource this pickup gives.
    // 这个掉落物给多少数量
    public int amount = 1;

    // Whether this pickup destroys itself after being collected.
    // 被收集后是否删除自己
    public bool destroyAfterCollect = true;

    [Header("Visual Effect")]

    // Rotation speed for simple visual effect.
    // 简单旋转效果的速度
    public float rotateSpeed = 90f;

    // Floating effect height.
    // 上下浮动高度
    public float floatHeight = 0.15f;

    // Floating effect speed.
    // 上下浮动速度
    public float floatSpeed = 2f;

    [Header("Magnet Settings")]

    // If true, pickup will move toward player when player is close.
    // 如果为 true，玩家靠近后掉落物会吸附过去
    public bool useMagnet = true;

    // Player must be within this distance to start magnet movement.
    // 玩家进入这个范围后开始吸附
    public float magnetRange = 5f;

    // Pickup collection distance.
    // 掉落物距离玩家多近时自动收集
    public float collectDistance = 0.8f;

    // Pickup movement speed when attracted.
    // 吸附时掉落物移动速度
    public float magnetMoveSpeed = 8f;

    // How quickly pickup accelerates while moving toward player.
    // 吸附时加速速度
    public float magnetAcceleration = 20f;

    // Delay before pickup can be magnetized.
    // 掉落物生成后多久才可以被吸附
    public float magnetStartDelay = 0.25f;

    [Header("Audio")]
    public AudioClip collectClip;
    private AudioSource audioSource;

    // Current magnet speed.
    // 当前吸附速度
    private float currentMagnetSpeed = 0f;

    // Starting position.
    // 初始位置
    private Vector3 startPosition;

    // Player target.
    // 玩家目标
    private Transform playerTarget;

    // Spawn time.
    // 生成时间
    private float spawnTime;

    // Whether this pickup has already been collected.
    // 防止重复收集
    private bool isCollected = false;

    private void Start()
    {
        // Save start position for floating effect.
        // 保存初始位置，用来做上下浮动
        startPosition = transform.position;

        // Save spawn time.
        // 保存生成时间
        spawnTime = Time.time;

        FindPlayer();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private void Update()
    {
        if (isCollected)
        {
            return;
        }

        RotatePickup();

        if (useMagnet && CanUseMagnet())
        {
            MagnetMove();
        }
        else
        {
            FloatPickup();
        }
    }

    private void FindPlayer()
    {
        // Find player by Player tag.
        // 通过 Player tag 寻找玩家
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
        }
    }

    private bool CanUseMagnet()
    {
        if (Time.time < spawnTime + magnetStartDelay)
        {
            return false;
        }

        if (playerTarget == null)
        {
            FindPlayer();

            if (playerTarget == null)
            {
                return false;
            }
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        return distanceToPlayer <= magnetRange;
    }

    private void RotatePickup()
    {
        // Rotate around Y axis.
        // 围绕 Y 轴旋转
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    private void FloatPickup()
    {
        // Move up and down slightly.
        // 轻微上下浮动
        float offsetY = Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        transform.position = new Vector3(
            startPosition.x,
            startPosition.y + offsetY,
            startPosition.z
        );
    }

    private void MagnetMove()
    {
        if (playerTarget == null)
        {
            return;
        }

        // Move toward player's center.
        // 朝玩家中心移动
        Vector3 targetPosition = playerTarget.position + Vector3.up * 1f;

        Vector3 directionToPlayer = targetPosition - transform.position;

        float distanceToPlayer = directionToPlayer.magnitude;

        // Collect when close enough.
        // 距离足够近时自动收集
        if (distanceToPlayer <= collectDistance)
        {
            CollectPickup();
            return;
        }

        if (directionToPlayer.sqrMagnitude <= 0.001f)
        {
            return;
        }

        directionToPlayer.Normalize();

        // Accelerate toward max magnet speed.
        // 慢慢加速到最大吸附速度
        currentMagnetSpeed += magnetAcceleration * Time.deltaTime;
        currentMagnetSpeed = Mathf.Min(currentMagnetSpeed, magnetMoveSpeed);

        transform.position += directionToPlayer * currentMagnetSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected)
        {
            return;
        }

        // Only player can collect this pickup.
        // 只有 Player 可以收集这个掉落物
        if (!other.CompareTag("Player"))
        {
            return;
        }

        CollectPickup();
    }

    private void CollectPickup()
    {
        if (isCollected)
        {
            return;
        }

        isCollected = true;

        if (GlobalRunManager.Instance == null)
        {
            Debug.Log("GlobalRunManager was not found.");

            if (destroyAfterCollect)
            {
                Destroy(gameObject);
            }

            return;
        }

        if (resourceType == ResourceType.Gold)
        {
            GlobalRunManager.Instance.AddRunGold(amount);
        }
        else if (resourceType == ResourceType.Trash)
        {
            GlobalRunManager.Instance.AddRunTrash(amount);
        }

        if (destroyAfterCollect)
        {
            if (collectClip != null)
                {
                    AudioSource.PlayClipAtPoint(collectClip, transform.position);
                }
            Destroy(gameObject);
        }
    }
}
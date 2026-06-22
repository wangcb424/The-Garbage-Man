using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeightedDecorationPrefab
{
    // Decoration prefab.
    // 装饰物 prefab
    public GameObject decorationPrefab;

    // Spawn weight. Higher weight means more likely to spawn.
    // 生成权重，数值越高越容易生成
    public float weight = 1f;
}

public class DecorationGenerator : MonoBehaviour
{
    [Header("Decoration Prefabs")]

    // Prefabs that can be randomly spawned.
    // 可以被随机生成的装饰物 prefab，比如草、花、石头、蘑菇等
    public List<WeightedDecorationPrefab> decorationPrefabs = new List<WeightedDecorationPrefab>();

    [Header("Spawn Count")]

    // Minimum number of decorations to spawn.
    // 最少生成多少个装饰物
    public int minSpawnCount = 3;

    // Maximum number of decorations to spawn.
    // 最多生成多少个装饰物
    public int maxSpawnCount = 8;

    [Header("Spawn Area")]

    // Size of the spawn area in local space.
    // 生成区域大小，基于当前物体的本地坐标
    public Vector2 areaSize = new Vector2(3f, 3f);

    // Height offset added to spawned decorations.
    // 装饰物生成时的高度偏移，防止陷进地面
    public float spawnHeightOffset = 0f;

    [Header("Spacing")]

    // Minimum distance between decorations.
    // 装饰物之间的最小距离，避免挤在一起
    public float minimumDistance = 0.4f;

    // Maximum attempts for each decoration.
    // 每个装饰物最多尝试多少次寻找位置，防止死循环
    public int maxAttemptsPerDecoration = 30;

    [Header("Avoid Colliders")]
    public bool avoidColliders = true;
    public float colliderCheckRadius = 0.5f;
    public LayerMask obstacleLayers;

    [Header("Random Rotation")]

    // If true, decorations will have random Y rotation.
    // 如果勾选，装饰物会随机绕 Y 轴旋转
    public bool randomYRotation = true;

    [Header("Random Scale")]

    // If true, decorations will have random scale.
    // 如果勾选，装饰物会随机缩放
    public bool randomScale = true;

    // Minimum random scale multiplier.
    // 随机缩放的最小倍率
    public float minScaleMultiplier = 0.8f;

    // Maximum random scale multiplier.
    // 随机缩放的最大倍率
    public float maxScaleMultiplier = 1.2f;

    [Header("Debug")]

    // If true, the script will spawn decorations automatically when the game starts.
    // 如果勾选，游戏开始时自动生成装饰物
    public bool generateOnStart = true;

    // If true, draw the spawn area in Scene view.
    // 如果勾选，在 Scene 视图里显示生成范围
    public bool drawGizmos = true;

    // Store generated decorations.
    // 保存已经生成出来的装饰物，方便清理
    private List<GameObject> spawnedDecorations = new List<GameObject>();

    // Store selected positions to prevent overlap.
    // 保存已经选过的位置，用来避免装饰物互相重叠
    private List<Vector3> usedPositions = new List<Vector3>();

    private void Start()
    {
        // Generate decorations automatically when this object starts.
        // 物体开始运行时自动生成装饰物
        if (generateOnStart)
        {
            GenerateDecorations();
        }
    }

    // Generate all decorations inside the area.
    // 在指定区域内生成所有装饰物
    public void GenerateDecorations()
    {
        ClearDecorations();

        if (decorationPrefabs == null || decorationPrefabs.Count == 0)
        {
            return;
        }

        int spawnCount = Random.Range(minSpawnCount, maxSpawnCount + 1);

        for (int i = 0; i < spawnCount; i++)
        {
            TrySpawnOneDecoration();
        }
    }

    // Try to spawn one decoration.
    // 尝试生成一个装饰物
    private void TrySpawnOneDecoration()
    {
        for (int attempt = 0; attempt < maxAttemptsPerDecoration; attempt++)
        {
            Vector3 localPosition = GetRandomLocalPosition();
            Vector3 worldPosition = transform.TransformPoint(localPosition);

            if (!IsFarEnoughFromOtherDecorations(worldPosition))
            {
                continue;
            }
            if (avoidColliders && IsTooCloseToCollider(worldPosition))
            {
                continue;
            }

            GameObject decorationPrefab = GetRandomDecorationPrefab();

            if (decorationPrefab == null)
            {
                return;
            }

            SpawnDecoration(decorationPrefab, worldPosition);
            usedPositions.Add(worldPosition);

            return;
        }
    }

    // Get a random local position inside the spawn area.
    // 在生成区域内获得一个随机本地坐标
    private Vector3 GetRandomLocalPosition()
    {
        float randomX = Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f);
        float randomZ = Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f);

        return new Vector3(randomX, spawnHeightOffset, randomZ);
    }

    // Check whether the new position is far enough from existing decorations.
    // 检查新位置和已有装饰物之间的距离是否足够远
    private bool IsFarEnoughFromOtherDecorations(Vector3 newPosition)
    {
        for (int i = 0; i < usedPositions.Count; i++)
        {
            float distance = Vector3.Distance(newPosition, usedPositions[i]);

            if (distance < minimumDistance)
            {
                return false;
            }
        }

        return true;
    }

    // Pick one random decoration prefab.
    // 从装饰物 prefab 列表里随机选一个
    private GameObject GetRandomDecorationPrefab()
    {
        if (decorationPrefabs == null || decorationPrefabs.Count == 0)
        {
            return null;
        }

        float totalWeight = 0f;

        for (int i = 0; i < decorationPrefabs.Count; i++)
        {
            if (decorationPrefabs[i].decorationPrefab != null &&
                decorationPrefabs[i].weight > 0f)
            {
                totalWeight += decorationPrefabs[i].weight;
            }
        }

        if (totalWeight <= 0f)
        {
            return null;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < decorationPrefabs.Count; i++)
        {
            if (decorationPrefabs[i].decorationPrefab == null ||
                decorationPrefabs[i].weight <= 0f)
            {
                continue;
            }

            currentWeight += decorationPrefabs[i].weight;

            if (randomValue <= currentWeight)
            {
                return decorationPrefabs[i].decorationPrefab;
            }
        }

    return decorationPrefabs[0].decorationPrefab;
    }

    // Spawn one decoration object.
    // 生成一个装饰物物体
    private void SpawnDecoration(GameObject decorationPrefab, Vector3 worldPosition)
    {
        Quaternion rotation = Quaternion.identity;

        if (randomYRotation)
        {
            float randomY = Random.Range(0f, 360f);
            rotation = Quaternion.Euler(0f, randomY, 0f);
        }

        GameObject decoration = Instantiate(
            decorationPrefab,
            worldPosition,
            rotation,
            transform
        );

        if (randomScale)
        {
            float scaleMultiplier =
                Random.Range(minScaleMultiplier, maxScaleMultiplier);

            decoration.transform.localScale =
                decoration.transform.localScale * scaleMultiplier;
        }

        spawnedDecorations.Add(decoration);
    }

    // Check whether this position is too close to colliders.
// 检查当前位置是否离障碍物 collider 太近
        private bool IsTooCloseToCollider(Vector3 worldPosition)
        {
            Collider[] colliders = Physics.OverlapSphere(
                worldPosition,
                colliderCheckRadius,
                obstacleLayers
            );

            return colliders.Length > 0;
        }

    // Clear all spawned decorations.
    // 清理所有已经生成的装饰物
    public void ClearDecorations()
    {
        for (int i = spawnedDecorations.Count - 1; i >= 0; i--)
        {
            if (spawnedDecorations[i] != null)
            {
                Destroy(spawnedDecorations[i]);
            }
        }

        spawnedDecorations.Clear();
        usedPositions.Clear();
    }

    // Draw spawn area in Scene view.
    // 在 Scene 视图中画出生成区域，方便调试范围
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
        {
            return;
        }

        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.DrawWireCube(
            Vector3.zero,
            new Vector3(areaSize.x, 0.1f, areaSize.y)
        );
    }
}
using System.Collections.Generic;
using UnityEngine;

// This class stores one enemy prefab and its spawn weight.
// A higher weight means this enemy is more likely to be selected.
[System.Serializable]
public class WeightedEnemyPrefab
{
    public GameObject enemyPrefab;
    public float weight = 1f;
}

public class DungeonEnemySpawner : MonoBehaviour
{
    // List of enemy prefabs this spawner can choose from.
    public List<WeightedEnemyPrefab> enemyPrefabs = new List<WeightedEnemyPrefab>();

    // Minimum and maximum number of enemies this spawner can create.
    public int minEnemyCount = 1;
    public int maxEnemyCount = 3;

    // The size of the room this spawner belongs to.
    public float roomWidth = 20f;
    public float roomHeight = 20f;

    // Optional spawn positions prepared by DungeonGenerator.
    // These positions avoid spawning enemies inside inner walls.
    public List<Vector3> spawnPositions = new List<Vector3>();

    // Stores enemies created by this spawner.
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    public void SetupSpawner(
        List<WeightedEnemyPrefab> newEnemyPrefabs,
        int newMinEnemyCount,
        int newMaxEnemyCount,
        float newRoomWidth,
        float newRoomHeight,
        List<Vector3> newSpawnPositions
    )
    {
        // Store the enemy list and room information given by DungeonGenerator.
        enemyPrefabs = newEnemyPrefabs;
        minEnemyCount = newMinEnemyCount;
        maxEnemyCount = newMaxEnemyCount;
        roomWidth = newRoomWidth;
        roomHeight = newRoomHeight;
        spawnPositions = newSpawnPositions;
    }

    public int SpawnEnemies()
    {
        // If there are no enemy prefabs, this room will not spawn enemies.
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.Log("No enemy prefabs assigned to this spawner.");
            return 0;
        }

        // Pick how many enemies to spawn.
        int enemyCount = Random.Range(minEnemyCount, maxEnemyCount + 1);
        int spawnedCount = 0;

        for (int i = 0; i < enemyCount; i++)
        {
            // Pick a random enemy prefab based on weight.
            GameObject selectedEnemyPrefab = GetRandomEnemyPrefab();

            if (selectedEnemyPrefab == null)
            {
                continue;
            }

            // Pick a spawn position inside the room.
            Vector3 spawnPosition = GetRandomSpawnPosition();

            // Create the enemy in the scene.
            GameObject enemy = Instantiate(selectedEnemyPrefab, spawnPosition, Quaternion.identity);
            enemy.name = selectedEnemyPrefab.name;
            enemy.transform.parent = transform;

            // Let the enemy assign its own movement and weapon modules.
            // 让 enemy 自己从 EnemyCore 上的 list 里分配移动模块和武器模块
            EnemyCore enemyCore = enemy.GetComponent<EnemyCore>();

            if (enemyCore != null)
            {
                enemyCore.AssignRandomModules();
            }

            // Keep track of this enemy so the room can know when it is cleared.
            spawnedEnemies.Add(enemy);
            spawnedCount++;
        }

        return spawnedCount;
    }

    public void ClearSpawnedEnemies()
    {
        // Destroy all enemies created by this spawner.
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            if (spawnedEnemies[i] != null)
            {
                Destroy(spawnedEnemies[i]);
            }
        }

        spawnedEnemies.Clear();
    }

    public int GetAliveEnemyCount()
    {
        int aliveCount = 0;

        // Remove destroyed enemy references from the list.
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == null)
            {
                spawnedEnemies.RemoveAt(i);
            }
            else
            {
                aliveCount++;
            }
        }

        return aliveCount;
    }

    private GameObject GetRandomEnemyPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            return null;
        }

        float totalWeight = 0f;

        // Add up all valid weights.
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            if (enemyPrefabs[i].enemyPrefab != null && enemyPrefabs[i].weight > 0f)
            {
                totalWeight += enemyPrefabs[i].weight;
            }
        }

        if (totalWeight <= 0f)
        {
            return null;
        }

        // Pick a random value inside the total weight range.
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        // Walk through the list until the random value falls into one prefab's weight range.
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            if (enemyPrefabs[i].enemyPrefab == null || enemyPrefabs[i].weight <= 0f)
            {
                continue;
            }

            currentWeight += enemyPrefabs[i].weight;

            if (randomValue <= currentWeight)
            {
                return enemyPrefabs[i].enemyPrefab;
            }
        }

        return enemyPrefabs[0].enemyPrefab;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        // Prefer positions prepared by DungeonGenerator.
        if (spawnPositions != null && spawnPositions.Count > 0)
        {
            int randomIndex = Random.Range(0, spawnPositions.Count);
            Vector3 chosenPosition = spawnPositions[randomIndex];
            spawnPositions.RemoveAt(randomIndex);
            return chosenPosition;
        }

        // Fallback random position if no spawn positions were given.
        float randomX = Random.Range(-roomWidth * 0.3f, roomWidth * 0.3f);
        float randomZ = Random.Range(-roomHeight * 0.3f, roomHeight * 0.3f);

        Vector3 spawnPosition = transform.position + new Vector3(randomX, 1f, randomZ);

        return spawnPosition;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the spawn area in the Scene view when this object is selected.
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(roomWidth * 0.6f, 1f, roomHeight * 0.6f)
        );
    }
}
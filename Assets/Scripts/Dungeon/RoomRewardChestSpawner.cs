using UnityEngine;

public class RoomRewardChestSpawner : MonoBehaviour
{
    [Header("Room Type")]
    public bool isStartRoom = false;
    public bool isShopRoom = false;
    public bool isExitRoom = false;

    [Header("Enemy Check")]
    public string enemyTag = "Enemy";
    public bool requirePlayerEnteredRoom = true;
    public bool requireHadEnemies = true;
    public float checkInterval = 0.4f;

    [Header("Room Check Area")]
    public Vector3 roomCenterOffset = Vector3.zero;
    public Vector3 roomHalfExtents = new Vector3(8f, 4f, 8f);

    [Header("Chest Spawn")]
    public GameObject rewardChestPrefab;
    public Transform chestSpawnPoint;
    public Vector3 chestSpawnOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Player Detection")]
    public string playerTag = "Player";

    private bool playerEnteredRoom = false;
    private bool hadEnemies = false;
    private bool chestSpawned = false;
    private float checkTimer = 0f;

    private void Update()
    {
        if (chestSpawned)
        {
            return;
        }

        if (IsDisabledRoomType())
        {
            return;
        }

        if (requirePlayerEnteredRoom && !playerEnteredRoom)
        {
            return;
        }

        checkTimer -= Time.deltaTime;

        if (checkTimer > 0f)
        {
            return;
        }

        checkTimer = checkInterval;

        CheckRoomEnemies();
    }

    private void CheckRoomEnemies()
    {
        int enemyCount = CountEnemiesInRoom();

        if (enemyCount > 0)
        {
            hadEnemies = true;
            return;
        }

        if (requireHadEnemies && !hadEnemies)
        {
            return;
        }

        SpawnChest();
    }

    private int CountEnemiesInRoom()
    {
        Vector3 center = GetRoomCenter();
        Collider[] colliders = Physics.OverlapBox(center, roomHalfExtents, Quaternion.identity);

        int enemyCount = 0;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider currentCollider = colliders[i];

            if (currentCollider == null)
            {
                continue;
            }

            GameObject currentObject = currentCollider.gameObject;

            if (currentObject == null)
            {
                continue;
            }

            if (!currentObject.activeInHierarchy)
            {
                continue;
            }

            if (currentObject.CompareTag(enemyTag))
            {
                enemyCount++;
                continue;
            }

            Transform parent = currentObject.transform.parent;

            while (parent != null)
            {
                if (parent.CompareTag(enemyTag))
                {
                    enemyCount++;
                    break;
                }

                parent = parent.parent;
            }
        }

        return enemyCount;
    }

    private void SpawnChest()
    {
        if (rewardChestPrefab == null)
        {
            Debug.Log("Reward Chest Prefab is missing.");
            return;
        }

        chestSpawned = true;

        Vector3 spawnPosition = GetChestSpawnPosition();

        Instantiate(rewardChestPrefab, spawnPosition, Quaternion.identity);

        Debug.Log("Reward chest spawned in room: " + gameObject.name);
    }

    private Vector3 GetChestSpawnPosition()
    {
        if (chestSpawnPoint != null)
        {
            return chestSpawnPoint.position;
        }

        return GetRoomCenter() + chestSpawnOffset;
    }

    private Vector3 GetRoomCenter()
    {
        return transform.position + roomCenterOffset;
    }

    private bool IsDisabledRoomType()
    {
        if (isStartRoom)
        {
            return true;
        }

        if (isShopRoom)
        {
            return true;
        }

        if (isExitRoom)
        {
            return true;
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        playerEnteredRoom = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(GetRoomCenter(), roomHalfExtents * 2f);
    }
}
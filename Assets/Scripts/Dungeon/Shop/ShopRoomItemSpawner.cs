using System.Collections.Generic;
using UnityEngine;

public class ShopRoomItemSpawner : MonoBehaviour
{
    [Header("Required Heal Item")]

    // Heal potion prefab that always appears in the shop.
    // 商店里必定刷新的回血药 prefab
    public GameObject healPotionPrefab;

    // Spawn point for the heal potion.
    // 回血药生成点
    public Transform healPotionSpawnPoint;

    [Header("Random Weapon Items")]

    // Weapon item prefabs that can randomly appear in the shop.
    // 商店里可以随机刷新的武器商品 prefab
    public List<GameObject> weaponItemPrefabs = new List<GameObject>();

    // Spawn points for random weapon items.
    // 武器商品生成点
    public List<Transform> weaponItemSpawnPoints = new List<Transform>();

    // If true, one weapon item can only appear once in this shop.
    // 如果为 true，同一个武器商品在一个商店里最多出现一次
    public bool preventDuplicateWeapons = true;

    [Header("Spawn Settings")]

    // If true, items are spawned when this object starts.
    // 如果为 true，商店生成时自动生成商品
    public bool spawnOnStart = true;

    // Random Y rotation for spawned items.
    // 商品生成时随机 Y 轴旋转
    public bool randomYRotation = true;

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnShopItems();
        }
    }

    public void SpawnShopItems()
    {
        SpawnHealPotion();
        SpawnRandomWeapons();
    }

    private void SpawnHealPotion()
    {
        if (healPotionPrefab == null)
        {
            Debug.Log("ShopRoomItemSpawner has no heal potion prefab.");
            return;
        }

        if (healPotionSpawnPoint == null)
        {
            Debug.Log("ShopRoomItemSpawner has no heal potion spawn point.");
            return;
        }

        SpawnItemAtPoint(healPotionPrefab, healPotionSpawnPoint);
    }

    private void SpawnRandomWeapons()
    {
        if (weaponItemPrefabs == null || weaponItemPrefabs.Count == 0)
        {
            Debug.Log("ShopRoomItemSpawner has no weapon item prefabs.");
            return;
        }

        if (weaponItemSpawnPoints == null || weaponItemSpawnPoints.Count == 0)
        {
            Debug.Log("ShopRoomItemSpawner has no weapon item spawn points.");
            return;
        }

        List<GameObject> availableWeapons = new List<GameObject>(weaponItemPrefabs);

        for (int i = 0; i < weaponItemSpawnPoints.Count; i++)
        {
            if (weaponItemSpawnPoints[i] == null)
            {
                continue;
            }

            if (availableWeapons.Count == 0)
            {
                return;
            }

            GameObject selectedWeaponPrefab = GetRandomWeaponPrefab(availableWeapons);

            if (selectedWeaponPrefab == null)
            {
                continue;
            }

            SpawnItemAtPoint(selectedWeaponPrefab, weaponItemSpawnPoints[i]);

            if (preventDuplicateWeapons)
            {
                availableWeapons.Remove(selectedWeaponPrefab);
            }
        }
    }

    private GameObject GetRandomWeaponPrefab(List<GameObject> availableWeapons)
    {
        if (availableWeapons == null || availableWeapons.Count == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, availableWeapons.Count);
        return availableWeapons[randomIndex];
    }

    private void SpawnItemAtPoint(GameObject itemPrefab, Transform spawnPoint)
    {
        if (itemPrefab == null || spawnPoint == null)
        {
            return;
        }

        Quaternion spawnRotation = spawnPoint.rotation;

        if (randomYRotation)
        {
            spawnRotation = Quaternion.Euler(
                spawnPoint.eulerAngles.x,
                Random.Range(0f, 360f),
                spawnPoint.eulerAngles.z
            );
        }

        GameObject itemObject = Instantiate(
            itemPrefab,
            spawnPoint.position,
            spawnRotation,
            transform
        );

        itemObject.name = itemPrefab.name;
    }
}
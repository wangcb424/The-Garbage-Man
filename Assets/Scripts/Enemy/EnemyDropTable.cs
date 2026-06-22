using UnityEngine;

public class EnemyDropTable : MonoBehaviour
{
    // Gold pickup prefab.
    // 金币掉落物 prefab
    public GameObject goldPickupPrefab;

    // Trash pickup prefab.
    // 垃圾掉落物 prefab
    public GameObject trashPickupPrefab;

    [Header("Gold Drop")]

    // Chance to drop gold when this enemy dies.
    // 敌人死亡时掉落金币的概率
    [Range(0f, 1f)]
    public float goldDropChance = 0.8f;

    // Minimum gold amount.
    // 最少掉落金币数量
    public int minGoldAmount = 1;

    // Maximum gold amount.
    // 最多掉落金币数量
    public int maxGoldAmount = 3;

    [Header("Trash Drop")]

    // Chance to drop trash when this enemy dies.
    // 敌人死亡时掉落垃圾的概率
    [Range(0f, 1f)]
    public float trashDropChance = 0.4f;

    // Minimum trash amount.
    // 最少掉落垃圾数量
    public int minTrashAmount = 1;

    // Maximum trash amount.
    // 最多掉落垃圾数量
    public int maxTrashAmount = 2;

    [Header("Drop Position")]

    // Random spread around enemy position.
    // 掉落物在敌人周围随机散开的范围
    public float dropSpreadRadius = 0.8f;

    // Drop height.
    // 掉落物生成高度
    public float dropHeight = 0.5f;

    public void DropResources()
    {
        DropGold();
        DropTrash();
    }

    private void DropGold()
    {
        if (goldPickupPrefab == null)
        {
            return;
        }

        if (Random.value > goldDropChance)
        {
            return;
        }

        int goldAmount = Random.Range(minGoldAmount, maxGoldAmount + 1);

        for (int i = 0; i < goldAmount; i++)
        {
            CreatePickup(goldPickupPrefab);
        }
    }

    private void DropTrash()
    {
        if (trashPickupPrefab == null)
        {
            return;
        }

        if (Random.value > trashDropChance)
        {
            return;
        }

        int trashAmount = Random.Range(minTrashAmount, maxTrashAmount + 1);

        for (int i = 0; i < trashAmount; i++)
        {
            CreatePickup(trashPickupPrefab);
        }
    }

    private void CreatePickup(GameObject pickupPrefab)
    {
        Vector2 randomCircle = Random.insideUnitCircle * dropSpreadRadius;

        Vector3 spawnPosition = transform.position + new Vector3(
            randomCircle.x,
            dropHeight,
            randomCircle.y
        );

        GameObject pickup = Instantiate(pickupPrefab, spawnPosition, Quaternion.identity);
        pickup.name = pickupPrefab.name;
    }
}
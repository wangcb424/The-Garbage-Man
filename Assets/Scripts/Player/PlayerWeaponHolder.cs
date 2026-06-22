using UnityEngine;

public class PlayerWeaponHolder : MonoBehaviour
{
    // Where the player weapon will be attached.
    // 玩家武器会挂到这个位置
    public Transform weaponSocket;

    // Starting weapon prefab.
    // 初始武器 prefab
    public GameObject startingWeaponPrefab;

    // Current weapon object in the scene.
    // 当前生成出来的武器物体
    private GameObject currentWeaponObject;

    // Current weapon script.
    // 当前武器脚本
    private PlayerWeapon currentWeapon;

    private void Start()
    {
        // Equip starting weapon when player starts.
        // 游戏开始时装备初始武器
        if (startingWeaponPrefab != null)
        {
            EquipWeapon(startingWeaponPrefab);
        }
    }

    private void Update()
    {
        // Let the current weapon run its attack logic.
        // 让当前武器执行攻击逻辑
        if (currentWeapon != null)
        {
            currentWeapon.TickWeapon();
        }
    }

    public void EquipWeapon(GameObject weaponPrefab)
    {
        if (weaponPrefab == null)
        {
            return;
        }

        // Remove old weapon first.
        // 先删除旧武器
        if (currentWeaponObject != null)
        {
            Destroy(currentWeaponObject);
        }

        Transform parentTransform = transform;

        if (weaponSocket != null)
        {
            parentTransform = weaponSocket;
        }

        // Create new weapon as child of weapon socket.
        // 把新武器生成到 weaponSocket 下面
        currentWeaponObject = Instantiate(weaponPrefab, parentTransform);
        currentWeaponObject.name = weaponPrefab.name;

        currentWeaponObject.transform.localPosition = Vector3.zero;
        currentWeaponObject.transform.localRotation = Quaternion.identity;
        currentWeaponObject.transform.localScale = Vector3.one;

        currentWeapon = currentWeaponObject.GetComponent<PlayerWeapon>();

        if (currentWeapon != null)
        {
            currentWeapon.Initialize(this);
        }
        else
        {
            Debug.Log(weaponPrefab.name + " does not have PlayerWeapon.");
        }
    }

    public Transform GetPlayerTransform()
    {
        return transform;
    }
}
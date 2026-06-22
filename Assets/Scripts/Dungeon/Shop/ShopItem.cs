using UnityEngine;

public class ShopItem : MonoBehaviour
{
    public enum ShopItemType
    {
        HealPotion,
        Weapon
    }

    [Header("Item Info")]

    public ShopItemType itemType = ShopItemType.HealPotion;

    public string itemName = "Item";

    [Header("Price Settings")]

    public int baseTrashCost = 3;
    public float priceGrowthMultiplier = 1.15f;
    public bool scalePriceWithLevel = true;

    [Header("Heal Potion Settings")]

    // How many potions this shop item gives.
    // 买这个商品给几瓶回血药
    public int healPotionAddAmount = 1;

    [Header("Weapon Settings")]

    public GameObject weaponPrefab;

    [Header("Buy Settings")]

    public bool destroyAfterBuy = true;
    public string playerTag = "Player";

    [Header("Info Panel")]

    public GameObject itemInfoPanelPrefab;
    public Vector3 itemInfoPanelOffset = new Vector3(0f, 1.8f, 0f);

    private bool playerInside = false;
    private bool isBought = false;

    private PlayerWeaponHolder playerWeaponHolder;

    private GameObject itemInfoPanelObject;
    private ShopItemInfoPanel itemInfoPanel;

    private void Update()
    {
        if (isBought)
        {
            return;
        }

        if (!playerInside)
        {
            return;
        }

        UpdateItemInfoPanel();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryBuyItem();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        playerInside = true;

        playerWeaponHolder = other.GetComponent<PlayerWeaponHolder>();

        if (playerWeaponHolder == null)
        {
            playerWeaponHolder = other.GetComponentInChildren<PlayerWeaponHolder>();
        }

        ShowItemInfoPanel();

        Debug.Log("Press E to buy " + itemName + " for " + GetFinalTrashCost() + " trash.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        playerInside = false;
        playerWeaponHolder = null;

        HideItemInfoPanel();
    }

    public int GetFinalTrashCost()
    {
        int finalCost = baseTrashCost;

        if (!scalePriceWithLevel)
        {
            return finalCost;
        }

        if (GlobalRunManager.Instance == null)
        {
            return finalCost;
        }

        int globalLevelNumber = GlobalRunManager.Instance.GetGlobalSmallLevelNumber();

        if (globalLevelNumber < 1)
        {
            globalLevelNumber = 1;
        }

        float scaledCost = baseTrashCost * Mathf.Pow(
            priceGrowthMultiplier,
            globalLevelNumber - 1
        );

        finalCost = Mathf.CeilToInt(scaledCost);

        if (finalCost < 0)
        {
            finalCost = 0;
        }

        return finalCost;
    }

    private void ShowItemInfoPanel()
    {
        if (itemInfoPanelPrefab == null)
        {
            Debug.Log(gameObject.name + " has no item info panel prefab.");
            return;
        }

        if (itemInfoPanelObject != null)
        {
            itemInfoPanelObject.SetActive(true);
            UpdateItemInfoPanel();
            return;
        }

        itemInfoPanelObject = Instantiate(
            itemInfoPanelPrefab,
            transform.position + itemInfoPanelOffset,
            Quaternion.identity
        );

        itemInfoPanelObject.name = itemInfoPanelPrefab.name;

        itemInfoPanel = itemInfoPanelObject.GetComponent<ShopItemInfoPanel>();

        if (itemInfoPanel == null)
        {
            Debug.Log(itemInfoPanelPrefab.name + " does not have ShopItemInfoPanel.");
            return;
        }

        itemInfoPanel.SetTarget(transform);
        itemInfoPanel.SetOffset(itemInfoPanelOffset);
        UpdateItemInfoPanel();
    }

    private void HideItemInfoPanel()
    {
        if (itemInfoPanelObject != null)
        {
            Destroy(itemInfoPanelObject);
            itemInfoPanelObject = null;
            itemInfoPanel = null;
        }
    }

    private void UpdateItemInfoPanel()
    {
        if (itemInfoPanel == null)
        {
            return;
        }

        itemInfoPanel.UpdatePanel(itemName, GetFinalTrashCost());
    }

    private void TryBuyItem()
    {
        if (GlobalRunManager.Instance == null)
        {
            Debug.Log("GlobalRunManager was not found.");
            return;
        }

        if (!CanBuyItem())
        {
            return;
        }

        int finalTrashCost = GetFinalTrashCost();

        if (!GlobalRunManager.Instance.SpendRunTrash(finalTrashCost))
        {
            Debug.Log("Not enough trash. Need " + finalTrashCost + " trash.");
            return;
        }

        ApplyItemEffect();

        isBought = true;

        HideItemInfoPanel();

        if (destroyAfterBuy)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private bool CanBuyItem()
    {
        if (itemType == ShopItemType.Weapon && weaponPrefab == null)
        {
            Debug.Log("This weapon shop item has no weapon prefab.");
            return false;
        }

        return true;
    }

    private void ApplyItemEffect()
    {
        if (itemType == ShopItemType.HealPotion)
        {
            BuyHealPotionItem();
        }
        else if (itemType == ShopItemType.Weapon)
        {
            BuyWeapon();
        }
    }

    private void BuyHealPotionItem()
    {
        if (GlobalRunManager.Instance == null)
        {
            return;
        }

        GlobalRunManager.Instance.AddHealPotion(healPotionAddAmount);

        Debug.Log("Bought heal potion item. Potion count +" + healPotionAddAmount);
    }

    private void BuyWeapon()
    {
        if (weaponPrefab == null)
        {
            return;
        }

        if (playerWeaponHolder == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

            if (playerObject != null)
            {
                playerWeaponHolder = playerObject.GetComponent<PlayerWeaponHolder>();
            }
        }

        if (playerWeaponHolder == null)
        {
            Debug.Log("PlayerWeaponHolder was not found.");
            return;
        }

        playerWeaponHolder.EquipWeapon(weaponPrefab);

        if (GlobalRunManager.Instance != null)
        {
            GlobalRunManager.Instance.SetWeapon(weaponPrefab);
        }

        Debug.Log("Bought weapon: " + weaponPrefab.name);
    }

    private void OnDestroy()
    {
        HideItemInfoPanel();
    }
}
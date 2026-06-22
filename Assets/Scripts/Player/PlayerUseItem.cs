using UnityEngine;

public class PlayerUseItem : MonoBehaviour
{
    public static event System.Action OnItemUsed;

    private PlayerInputActions inputActions;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        inputActions = new PlayerInputActions();

        playerHealth = GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            playerHealth = GetComponentInChildren<PlayerHealth>();
        }
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Update()
    {
        if (inputActions.Player.UseItem.WasPressedThisFrame())
        {
            TryUseItem();
        }
    }

    private void TryUseItem()
    {
        Debug.Log("TryUseItem called");

        if (GlobalRunManager.Instance == null)
        {
            Debug.Log("GlobalRunManager is null");
            return;
        }

        if (playerHealth == null)
        {
            Debug.Log("PlayerHealth was not found.");
            return;
        }

        // No heal potion, cannot use.
        // 没有回血药，不能喝
        if (GlobalRunManager.Instance.healPotionCount <= 0)
        {
            Debug.Log("No heal potion left.");
            return;
        }

        // Full health, do not waste potion.
        // 满血时不能喝，不消耗回血药
        if (GlobalRunManager.Instance.currentHealth >= GlobalRunManager.Instance.maxHealth)
        {
            Debug.Log("Health is already full.");
            return;
        }

        // Consume one heal potion.
        // 消耗一瓶回血药
        GlobalRunManager.Instance.healPotionCount--;

        // Heal through PlayerHealth.
        // 通过 PlayerHealth 回血
        playerHealth.Heal(GlobalRunManager.Instance.itemHealAmount);

        Debug.Log(
            "Used heal potion. Remaining: " +
            GlobalRunManager.Instance.healPotionCount
        );

        OnItemUsed?.Invoke();
    }
}
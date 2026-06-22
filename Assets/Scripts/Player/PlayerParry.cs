using UnityEngine;

public class PlayerParry : MonoBehaviour
{
    public GameObject parryShield;
    public float parryDuration = 0.3f;
    public float parryCooldown = 1f;

    public static event System.Action OnParrySuccess;
    public bool isParrying { get; private set; }

    private PlayerInputActions inputActions;
    private PlayerBlock playerBlock;
    private float parryTimer;
    private float cooldownTimer;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        playerBlock = GetComponent<PlayerBlock>();
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
        cooldownTimer -= Time.deltaTime;

        if (inputActions.Player.Parry.WasPressedThisFrame() && !isParrying && cooldownTimer <= 0f)
        {
            StartParry();
        }

        if (isParrying)
        {
            parryTimer -= Time.deltaTime;
            if (parryTimer <= 0f)
            {
                EndParry();
            }
        }
    }

    private void StartParry()
    {
        isParrying = true;
        parryTimer = parryDuration;
        cooldownTimer = parryCooldown;

        if (parryShield != null)
        {
            parryShield.SetActive(true);
        }
    }

    private void EndParry()
    {
        isParrying = false;

        if (parryShield != null && (playerBlock == null || !playerBlock.isBlocking))
        {
            parryShield.SetActive(false);
        }
    }

    public void TriggerParrySuccess()
    {
        OnParrySuccess?.Invoke();
    }
}
using UnityEngine;

public class PlayerBlock : MonoBehaviour
{
    public bool isBlocking { get; private set; }
    public GameObject blockShield;

    private PlayerInputActions inputActions;
    private PlayerParry playerParry;
    private bool wasBlocking = false;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        playerParry = GetComponent<PlayerParry>();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();

        if (blockShield != null && (playerParry == null || !playerParry.isParrying))
        {
            blockShield.SetActive(false);
        }
    }

    private void Update()
    {
        isBlocking = inputActions.Player.Block.IsPressed();

        if (isBlocking && !wasBlocking)
        {
            if (blockShield != null)
            {
                blockShield.SetActive(true);
            }
        }
        else if (!isBlocking && wasBlocking)
        {
            if (blockShield != null && (playerParry == null || !playerParry.isParrying))
            {
                blockShield.SetActive(false);
            }
        }

        wasBlocking = isBlocking;
    }
}
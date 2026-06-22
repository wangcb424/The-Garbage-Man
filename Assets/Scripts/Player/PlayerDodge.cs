using UnityEngine;

public class PlayerDodge : MonoBehaviour
{
    public float dodgeSpeed = 2f;
    public float dodgeDuration = 0.5f;
    public float dodgeCoolDown = 1f;
    [HideInInspector] public bool isDodging = false;

    private CharacterController characterController;
    private PlayerMovement playerMovement;
    private PlayerInputActions inputActions;
    private PlayerHealth playerHealth;
    private bool dodgePressed;
    private float dodgeTimer;
    private float cooldownTimer;
    private Vector3 dodgeDirection;

    public static event System.Action OnDodgeStart;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();
        inputActions = new PlayerInputActions();
    }

    void Update()
    {
        ReadInput();

        cooldownTimer -= Time.deltaTime;
        if (dodgePressed && !isDodging && cooldownTimer <= 0)
        {
            StartDodge();
        }

        if (isDodging)
        {
            ApplyDodge();
        }
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        
        if (playerHealth != null)
        {
            playerHealth.canTakeDamage = true;
        }
    }

    void ReadInput()
    {
        dodgePressed = inputActions.Player.Dodge.WasPressedThisFrame();
    }

    void StartDodge()
    {
        isDodging = true;
        playerHealth.canTakeDamage = false;
        float speed = playerMovement.moveSpeed * dodgeSpeed;
        OnDodgeStart?.Invoke();

        dodgeTimer = dodgeDuration;
        cooldownTimer = dodgeCoolDown;

        if (playerMovement.moveInput.sqrMagnitude > 0.001f)
        {
            Vector3 inputDirection = new Vector3(playerMovement.moveInput.x, 0f, playerMovement.moveInput.y);
            Vector3 cameraRelative = Camera.main.transform.TransformDirection(inputDirection);
            cameraRelative.y = 0f;
            dodgeDirection = cameraRelative.normalized * speed;
        }
        else
        {
            dodgeDirection = transform.forward * speed;
        }
    }

    void ApplyDodge()
    {
        dodgeTimer -= Time.deltaTime;

        Vector3 move = dodgeDirection;
        move.y = playerMovement.verticalVelocity;
        characterController.Move(move * Time.deltaTime);

        if (dodgeTimer <= 0f)
        {
            isDodging = false;
            playerHealth.canTakeDamage = true;
        }
    }
}

using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Gravity")]
    public float gravity = -20f;
    public float groundedForce = -2f;

    [Header("Rotation")]
    public float rotationSpeed = 5f;

    [Header("Movement")]
    public float moveSpeed = 10f;

    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public float verticalVelocity;

    private CharacterController characterController;
    private PlayerInputActions inputActions;
    private PlayerDodge playerDodge;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();
        playerDodge = GetComponent<PlayerDodge>();
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
        ReadInput();
        ApplyGravity();
        PlayerDirection();

        if (playerDodge == null || !playerDodge.isDodging)
        {
            MovePlayer();
        }
    }

    private void ReadInput()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
    }

    private void ApplyGravity()
    {
        if (characterController == null)
        {
            return;
        }

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedForce;
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    // Player faces the mouse direction.
    // 玩家视角始终面向鼠标方向
    private void PlayerDirection()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            return;
        }

        Plane groundPlane = new Plane(Vector3.up, transform.position);
        Ray mouseRay = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (groundPlane.Raycast(mouseRay, out float distance))
        {
            Vector3 hitPoint = mouseRay.GetPoint(distance);
            Vector3 direction = hitPoint - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                float finalRotationSpeed =
                    rotationSpeed * MouseSensitivitySettings.CurrentSensitivity;

                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    targetRotation,
                    finalRotationSpeed * Time.deltaTime
                );
            }
        }
    }

    private void MovePlayer()
    {
        if (characterController == null)
        {
            return;
        }

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            return;
        }

        float speed = GetMoveSpeed();

        Vector3 inputDirection = new Vector3(
            moveInput.x,
            0f,
            moveInput.y
        );

        Vector3 cameraRelative = mainCamera.transform.TransformDirection(inputDirection);
        cameraRelative.y = 0f;

        Vector3 move = cameraRelative.normalized * speed;
        move.y = verticalVelocity;

        characterController.Move(move * Time.deltaTime);
    }

    private float GetMoveSpeed()
    {
        if (GlobalRunManager.Instance != null)
        {
            return GlobalRunManager.Instance.moveSpeed;
        }

        return moveSpeed;
    }
}
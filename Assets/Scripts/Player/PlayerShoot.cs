using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public bool isFireHeld { get; private set; }
    public bool isFirePressed { get; private set; }

    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
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
        isFireHeld = inputActions.Player.Fire.IsPressed();
        isFirePressed = inputActions.Player.Fire.WasPressedThisFrame();
    }
}

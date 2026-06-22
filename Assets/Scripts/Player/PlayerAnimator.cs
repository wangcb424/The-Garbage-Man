using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private PlayerMovement playerMovement;
    private PlayerDodge playerDodge;
    private PlayerShoot playerShoot;
    private PlayerBlock playerBlock;
    private PlayerParry playerParry;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        playerDodge = GetComponent<PlayerDodge>();
        playerShoot = GetComponent<PlayerShoot>();
        playerBlock = GetComponent<PlayerBlock>();
        playerParry = GetComponent<PlayerParry>();
    }

    private void OnEnable()
    {
        PlayerUseItem.OnItemUsed += HandleItemUsed;
        PlayerHealth.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        PlayerUseItem.OnItemUsed -= HandleItemUsed;
        PlayerHealth.OnDeath -= HandleDeath;
    }

    private void Update()
    {
        animator.SetBool("isMoving", playerMovement.moveInput.sqrMagnitude > 0.001f);
        animator.SetFloat("speed", playerMovement.moveInput.magnitude);
        animator.SetBool("isDodging", playerDodge.isDodging);
        animator.SetBool("isBlocking", playerBlock.isBlocking);
        animator.SetBool("isParrying", playerParry.isParrying);
        animator.SetBool("isShooting", playerShoot.isFireHeld);
    }

    private void HandleItemUsed()
    {
        Debug.Log("UseItem triggered");
        animator.SetTrigger("useItem");
    }

    private void HandleDeath()
    {
        animator.SetBool("isDead", true);
    }
}
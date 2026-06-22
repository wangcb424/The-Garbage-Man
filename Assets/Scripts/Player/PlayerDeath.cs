using UnityEngine;
using System.Collections;

public class PlayerDeath : MonoBehaviour
{
    public float deathAnimationLength = 2f;

    private PlayerMovement playerMovement;
    private PlayerDodge playerDodge;
    private PlayerShoot playerShoot;
    private PlayerBlock playerBlock;
    private PlayerParry playerParry;
    private PlayerUseItem playerUseItem;
    private PlayerAnimator playerAnimator;
    private PlayerWeaponHolder playerWeaponHolder;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerDodge = GetComponent<PlayerDodge>();
        playerShoot = GetComponent<PlayerShoot>();
        playerBlock = GetComponent<PlayerBlock>();
        playerParry = GetComponent<PlayerParry>();
        playerUseItem = GetComponent<PlayerUseItem>();
        playerAnimator = GetComponent<PlayerAnimator>();
        playerWeaponHolder = GetComponent<PlayerWeaponHolder>();
    }

    private void OnEnable()
    {
        PlayerHealth.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        PlayerHealth.OnDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        DisableAllInput();
        StartCoroutine(FreezeAnimatorAfterDeath());
        StartCoroutine(WaitAndLoadHub());
    }

    private void DisableAllInput()
    {
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerDodge != null) playerDodge.enabled = false;
        if (playerShoot != null) playerShoot.enabled = false;
        if (playerBlock != null) playerBlock.enabled = false;
        if (playerParry != null) playerParry.enabled = false;
        if (playerUseItem != null) playerUseItem.enabled = false;
        if (playerAnimator != null) playerAnimator.enabled = false;
        if (playerWeaponHolder != null) playerWeaponHolder.enabled = false;
    }

    private void ReEnableAllInput()
    {
        if (playerMovement != null) playerMovement.enabled = true;
        if (playerDodge != null) playerDodge.enabled = true;
        if (playerShoot != null) playerShoot.enabled = true;
        if (playerBlock != null) playerBlock.enabled = true;
        if (playerParry != null) playerParry.enabled = true;
        if (playerUseItem != null) playerUseItem.enabled = true;
        if (playerAnimator != null) playerAnimator.enabled = true;
        if (playerWeaponHolder != null) playerWeaponHolder.enabled = true;
    }

    private IEnumerator FreezeAnimatorAfterDeath()
    {
        yield return new WaitForSeconds(deathAnimationLength);

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.speed = 0f;
        }
    }

    private IEnumerator WaitAndLoadHub()
    {
        yield return new WaitForSeconds(deathAnimationLength + 0.5f);

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.speed = 1f;
            animator.SetBool("isDead", false);
        }

        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.ResetDeath();
        }

        if (GlobalRunManager.Instance != null)
        {
            ReEnableAllInput();
            if (animator != null)
            {
                animator.speed = 1f;
                animator.SetBool("isDead", false);
                animator.Update(0f);
            }
            GlobalRunManager.Instance.LoadHubScene();
        }
    }
}

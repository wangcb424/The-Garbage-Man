using UnityEngine;

public class PlayerHitResponder : MonoBehaviour
{
    private HitFlash hitFlash;

    private void Awake()
    {
        hitFlash = GetComponent<HitFlash>();
    }

    private void OnEnable()
    {
        PlayerHealth.OnPlayerHit += HandlePlayerHit;
    }

    private void OnDisable()
    {
        PlayerHealth.OnPlayerHit -= HandlePlayerHit;
    }

    private void HandlePlayerHit()
    {
        if (hitFlash != null)
        {
            hitFlash.Flash();
        }
    }
}
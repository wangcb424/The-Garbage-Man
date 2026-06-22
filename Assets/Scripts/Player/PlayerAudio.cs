using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Action Sounds")]
    public AudioClip dodgeClip;
    public AudioClip blockClip;
    public AudioClip parryClip;
    public AudioClip useItemClip;
    public AudioClip deathClip;

    [Header("Footstep")]
    public AudioClip[] footstepClips;

    [Header("General SFX")]
    public AudioClip playerHitClip;

    [Header("Settings")]
    public float footstepVolume = 0.5f;
    public float actionVolume = 1f;

    private AudioSource audioSource;
    private AudioSource footstepSource;
    private int lastFootstepIndex = -1;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.playOnAwake = false;
        footstepSource.loop = true;
        footstepSource.volume = footstepVolume;
    }

    private void OnEnable()
    {
        PlayerDodge.OnDodgeStart += HandleDodge;
        ShieldTrigger.OnBlockHit += HandleBlock;
        PlayerParry.OnParrySuccess += HandleParry;
        PlayerUseItem.OnItemUsed += HandleUseItem;
        PlayerHealth.OnDeath += HandleDeath;
        PlayerHealth.OnPlayerHit += HandlePlayerHit;
    }

    private void OnDisable()
    {
        PlayerDodge.OnDodgeStart -= HandleDodge;
        ShieldTrigger.OnBlockHit -= HandleBlock;
        PlayerParry.OnParrySuccess -= HandleParry;
        PlayerUseItem.OnItemUsed -= HandleUseItem;
        PlayerHealth.OnDeath -= HandleDeath;
        PlayerHealth.OnPlayerHit -= HandlePlayerHit;
    }

    private void Update()
    {
        
    }

    public void OnFootstep()
    {
        if (footstepClips.Length == 0)
        {
            return;
        }

        if (footstepClips.Length == 1)
        {
            PlaySound(footstepClips[0]);
            return;
        }

        int randomIndex;

        do
        {
            randomIndex = Random.Range(0, footstepClips.Length);
        }
        while (randomIndex == lastFootstepIndex);

            lastFootstepIndex = randomIndex;
            PlaySound(footstepClips[randomIndex]);
        }

    private void HandleDodge()
    {
        PlaySound(dodgeClip);
    }

    private void HandleBlock()
    {
        PlaySound(blockClip);
    }

    private void HandleParry()
    {
        PlaySound(parryClip);
    }

    private void HandleUseItem()
    {
        PlaySound(useItemClip);
    }

    private void HandleDeath()
    {
        PlaySound(deathClip);
        footstepSource.Stop();
    }

    private void HandlePlayerHit()
    {
        PlaySound(playerHitClip);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, actionVolume);
    }
}
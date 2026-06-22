using UnityEngine;

public class EnemyAudio : MonoBehaviour
{
    public AudioClip hitClip;
    public AudioClip deathClip;

    [Range(0f, 1f)]
    public float volume = 1f;

    private AudioSource audioSource;
    private EnemyCore enemyCore;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.maxDistance = 20f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        enemyCore = GetComponent<EnemyCore>();
    }

    private void OnEnable()
    {
        if (enemyCore != null)
        {
            enemyCore.OnHit += HandleHit;
            enemyCore.OnDied += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (enemyCore != null)
        {
            enemyCore.OnHit -= HandleHit;
            enemyCore.OnDied -= HandleDeath;
        }
    }

    private void HandleHit(int damage)
    {
        Debug.Log(gameObject.name + " HandleHit called, hitClip is: " + hitClip);
        PlaySound(hitClip);
    }

    private void HandleDeath()
    {
        if (deathClip != null)
        {
            AudioSource.PlayClipAtPoint(deathClip, transform.position, volume);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, volume);
    }
}
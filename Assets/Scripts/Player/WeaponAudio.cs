using UnityEngine;

public class WeaponAudio : MonoBehaviour
{
    public AudioClip shootClip;

    [Range(0f, 1f)]
    public float volume = 1f;

    public float minTimeBetweenSounds = 0.05f;

    private AudioSource audioSource;
    private float lastPlayTime = -999f;
    private PlayerWeapon playerWeapon;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = volume;

        playerWeapon = GetComponent<PlayerWeapon>();

        Debug.Log(gameObject.name + " playerWeapon is: " + playerWeapon);
    }

    private void OnEnable()
    {
        if (playerWeapon != null)
        {
            playerWeapon.OnWeaponShoot += HandleShoot;
        }
    }

    private void OnDisable()
    {
        if (playerWeapon != null)
        {
            playerWeapon.OnWeaponShoot -= HandleShoot;
        }
    }

    private void HandleShoot()
    {
        if (shootClip == null)
        {
            return;
        }

        if (Time.time - lastPlayTime < minTimeBetweenSounds)
        {
            return;
        }

        lastPlayTime = Time.time;
        audioSource.clip = shootClip;
        audioSource.Play();
    }
}
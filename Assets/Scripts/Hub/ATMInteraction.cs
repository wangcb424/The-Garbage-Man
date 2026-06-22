using UnityEngine;

public class ATMInteraction : MonoBehaviour
{
    public AudioClip depositClip;
    public AudioClip milestoneClip;

    [Range(0f, 1f)]
    public float volume = 1f;

    private AudioSource audioSource;
    private bool playerInRange = false;
    private PlayerInputActions inputActions;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        GlobalRunManager.OnMilestoneReached += HandleMilestoneReached;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        GlobalRunManager.OnMilestoneReached -= HandleMilestoneReached;
    }

    private void Update()
    {
        if (!playerInRange)
        {
            return;
        }

        if (inputActions.Player.Interact.WasPressedThisFrame())
        {
            Debug.Log("Interact pressed, playerInRange: " + playerInRange);
            TryDeposit();
        }
    }

    private void TryDeposit()
    {
        if (GlobalRunManager.Instance == null)
        {
            return;
        }

        if (GlobalRunManager.Instance.totalGold <= 0)
        {
            Debug.Log("No gold to deposit.");
            return;
        }

        GlobalRunManager.Instance.DepositAllRunGold();
        PlaySound(depositClip);
    }

    private void HandleMilestoneReached(int milestoneIndex)
    {
        PlaySound(milestoneClip);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, volume);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
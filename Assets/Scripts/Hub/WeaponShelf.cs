using UnityEngine;

public class WeaponShelf : MonoBehaviour
{
    public GameObject weaponDisplayObject;
    public GameObject weaponPrefab;
    public int requiredMilestone = 0;

    public AudioClip equipClip;
    private AudioSource audioSource;

    private bool playerInRange = false;
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
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

    private void Start()
    {
        UpdateWeaponDisplay();
    }

    private void Update()
    {
        if (!playerInRange)
        {
            return;
        }

        if (!IsUnlocked())
        {
            return;
        }

        if (inputActions.Player.Interact.WasPressedThisFrame())
        {
            EquipWeapon();
        }
    }

    private bool IsUnlocked()
    {
        if (requiredMilestone == 0)
        {
            return true;
        }

        if (GlobalRunManager.Instance == null)
        {
            return false;
        }

        return GlobalRunManager.Instance.milestonesReached >= requiredMilestone;
    }

    private void UpdateWeaponDisplay()
    {
        if (weaponDisplayObject == null)
        {
            return;
        }

        weaponDisplayObject.SetActive(IsUnlocked());
    }

    private void EquipWeapon()
    {
        if (weaponPrefab == null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            return;
        }

        PlayerWeaponHolder weaponHolder = playerObject.GetComponent<PlayerWeaponHolder>();

        if (weaponHolder == null)
        {
            return;
        }

        weaponHolder.EquipWeapon(weaponPrefab);
        if (equipClip != null)
        {
            audioSource.PlayOneShot(equipClip);
        }

        Debug.Log("Equipped weapon: " + weaponPrefab.name);
    }

    private void HandleMilestoneReached(int milestoneIndex)
    {
        UpdateWeaponDisplay();
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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmallBuffPickup : MonoBehaviour
{
    [Header("Buff Data")]
    public BuffData buffData;

    [Header("UI")]
    public Image iconImage;
    public TMP_Text nameText;

    [Header("Player Detection")]
    public string playerTag = "Player";

    [Header("Pickup Settings")]
    public float pickupDistance = 0.75f;

    [Header("Auto Attract Settings")]
    public bool autoAttract = true;
    public float attractRadius = 4f;
    public float attractStartDelay = 0.25f;
    public float attractSpeed = 6f;
    public float attractAcceleration = 18f;
    public LayerMask playerLayerMask = ~0;

    [Header("Floating Movement")]
    public bool floatUpDown = true;
    public float floatHeight = 0.15f;
    public float floatSpeed = 2f;

    [Header("Pickup Effect")]
    public GameObject pickupEffectPrefab;

    [Header("Audio")]
    public AudioClip collectClip;
    private AudioSource audioSource;

    private Vector3 startPosition;
    private Transform targetPlayer;
    private float currentAttractSpeed = 0f;
    private float spawnTimer = 0f;
    private bool pickedUp = false;

    private void Start()
    {
        startPosition = transform.position;
        spawnTimer = attractStartDelay;
        UpdateVisualFromBuffData();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private void Update()
    {
        if (pickedUp)
        {
            return;
        }

        if (spawnTimer > 0f)
        {
            spawnTimer -= Time.deltaTime;
            UpdateFloating();
            return;
        }

        if (autoAttract)
        {
            UpdateAutoAttract();
        }

        if (targetPlayer == null)
        {
            UpdateFloating();
        }
    }

    public void SetBuffData(BuffData newBuffData)
    {
        buffData = newBuffData;
        UpdateVisualFromBuffData();
    }

    private void UpdateVisualFromBuffData()
    {
        if (buffData == null)
        {
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = buffData.icon;
            iconImage.enabled = buffData.icon != null;
        }

        if (nameText != null)
        {
            nameText.text = buffData.buffName;
        }
    }

    private void UpdateAutoAttract()
    {
        if (targetPlayer == null)
        {
            targetPlayer = FindPlayerInAttractRange();
        }

        if (targetPlayer == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, targetPlayer.position);

        if (distance <= pickupDistance)
        {
            PickUp();
            return;
        }

        currentAttractSpeed += attractAcceleration * Time.deltaTime;

        if (currentAttractSpeed > attractSpeed)
        {
            currentAttractSpeed = attractSpeed;
        }

        Vector3 targetPosition = targetPlayer.position + Vector3.up * 0.8f;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            currentAttractSpeed * Time.deltaTime
        );
    }

    private Transform FindPlayerInAttractRange()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            attractRadius,
            playerLayerMask
        );

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider currentCollider = colliders[i];

            if (currentCollider == null)
            {
                continue;
            }

            if (currentCollider.CompareTag(playerTag))
            {
                return currentCollider.transform;
            }

            Transform parent = currentCollider.transform.parent;

            while (parent != null)
            {
                if (parent.CompareTag(playerTag))
                {
                    return parent;
                }

                parent = parent.parent;
            }
        }

        return null;
    }

    private void UpdateFloating()
    {
        if (!floatUpDown)
        {
            return;
        }

        float offsetY = Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        transform.position = new Vector3(
            startPosition.x,
            startPosition.y + offsetY,
            startPosition.z
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (pickedUp)
        {
            return;
        }

        if (!other.CompareTag(playerTag))
        {
            return;
        }

        PickUp();
    }

    private void PickUp()
    {
        if (pickedUp)
        {
            return;
        }

        pickedUp = true;

        if (buffData == null)
        {
            Debug.Log("SmallBuffPickup has no BuffData.");
            Destroy(gameObject);
            return;
        }

        buffData.ApplyBuff();

        if (pickupEffectPrefab != null)
        {
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
        }

        Debug.Log("Picked up small buff: " + buffData.buffName);

        if (collectClip != null)
        {
            AudioSource.PlayClipAtPoint(collectClip, transform.position);
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attractRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);
    }
}
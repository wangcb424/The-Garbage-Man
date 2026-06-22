using System.Collections;
using UnityEngine;

public class LevelRewardChest : MonoBehaviour
{
    [Header("Player Detection")]
    public string playerTag = "Player";

    [Header("Drop Settings")]
    public SmallBuffPickup smallBuffPickupPrefab;
    public BuffData[] smallBuffPool;
    public int dropCount = 3;
    public float dropRadius = 1.4f;
    public float dropHeight = 0.35f;

    [Header("Lid Animation")]
    public Transform lidPivot;

    // Closed lid rotation.
    // 盖子关闭时的本地旋转
    public Vector3 closedLidLocalEuler = Vector3.zero;

    // Opened lid rotation.
    // 盖子打开后的本地旋转
    public Vector3 openedLidLocalEuler = new Vector3(-100f, 0f, 0f);

    public float openDuration = 0.45f;

    [Header("Drop Timing")]
    public bool dropAfterAnimation = true;
    public float extraDropDelay = 0.15f;

    [Header("Effects")]
    public GameObject openEffectPrefab;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSound;

    [Header("After Open")]
    public bool destroyAfterOpen = false;
    public float destroyDelay = 2f;

    private bool opened = false;
    private Collider chestCollider;

    private void Start()
    {
        chestCollider = GetComponent<Collider>();

        if (lidPivot != null)
        {
            lidPivot.localRotation = Quaternion.Euler(closedLidLocalEuler);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (opened)
        {
            return;
        }

        if (!other.CompareTag(playerTag))
        {
            return;
        }

        OpenChest();
    }

    private void OpenChest()
    {
        if (opened)
        {
            return;
        }

        opened = true;

        if (chestCollider != null)
        {
            chestCollider.enabled = false;
        }

        PlayOpenSound();
        SpawnOpenEffect();

        StartCoroutine(OpenChestRoutine());
    }

    private IEnumerator OpenChestRoutine()
    {
        if (!dropAfterAnimation)
        {
            DropSmallBuffs();
        }

        yield return StartCoroutine(AnimateLidOpen());

        if (dropAfterAnimation)
        {
            yield return new WaitForSeconds(extraDropDelay);
            DropSmallBuffs();
        }

        Debug.Log("Reward chest opened.");

        if (destroyAfterOpen)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    private IEnumerator AnimateLidOpen()
    {
        if (lidPivot == null)
        {
            yield break;
        }

        Quaternion startRotation = Quaternion.Euler(closedLidLocalEuler);
        Quaternion endRotation = Quaternion.Euler(openedLidLocalEuler);

        float timer = 0f;

        while (timer < openDuration)
        {
            timer += Time.deltaTime;

            float t = timer / openDuration;
            t = Mathf.Clamp01(t);

            // Smooth open animation.
            // 平滑开盖
            t = Mathf.SmoothStep(0f, 1f, t);

            lidPivot.localRotation = Quaternion.Slerp(
                startRotation,
                endRotation,
                t
            );

            yield return null;
        }

        lidPivot.localRotation = endRotation;
    }

    private void DropSmallBuffs()
    {
        if (smallBuffPickupPrefab == null)
        {
            Debug.Log("Small Buff Pickup Prefab is missing.");
            return;
        }

        if (smallBuffPool == null || smallBuffPool.Length == 0)
        {
            Debug.Log("Small Buff Pool is empty.");
            return;
        }

        for (int i = 0; i < dropCount; i++)
        {
            BuffData randomBuff = GetRandomBuff();

            if (randomBuff == null)
            {
                continue;
            }

            Vector3 dropPosition = GetDropPosition(i);

            SmallBuffPickup pickup = Instantiate(
                smallBuffPickupPrefab,
                dropPosition,
                Quaternion.identity
            );

            pickup.SetBuffData(randomBuff);
        }
    }

    private BuffData GetRandomBuff()
    {
        int randomIndex = Random.Range(0, smallBuffPool.Length);
        return smallBuffPool[randomIndex];
    }

    private Vector3 GetDropPosition(int index)
    {
        float angle = 360f / dropCount * index;
        float angleInRadians = angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            Mathf.Cos(angleInRadians) * dropRadius,
            dropHeight,
            Mathf.Sin(angleInRadians) * dropRadius
        );

        return transform.position + offset;
    }

    private void SpawnOpenEffect()
    {
        if (openEffectPrefab == null)
        {
            return;
        }

        Instantiate(openEffectPrefab, transform.position, Quaternion.identity);
    }

    private void PlayOpenSound()
    {
        if (audioSource == null)
        {
            return;
        }

        if (openSound == null)
        {
            return;
        }

        audioSource.PlayOneShot(openSound);
    }
}
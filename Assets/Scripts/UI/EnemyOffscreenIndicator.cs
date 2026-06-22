using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyOffscreenIndicator : MonoBehaviour
{
    [Header("UI")]
    public RectTransform rectTransform;
    public Image arrowImage;
    public TMP_Text distanceText;

    [Header("Target")]
    public Transform target;

    [Header("Settings")]
    public bool showDistance = false;

    private Camera targetCamera;

    private void Awake()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (arrowImage == null)
        {
            arrowImage = GetComponent<Image>();
        }
    }

    public void Setup(Transform newTarget, Camera newCamera)
    {
        target = newTarget;
        targetCamera = newCamera;

        gameObject.SetActive(true);
    }

    public void Clear()
    {
        target = null;
        gameObject.SetActive(false);

        if (distanceText != null)
        {
            distanceText.text = "";
        }
    }

    public bool HasValidTarget()
    {
        if (target == null)
        {
            return false;
        }

        if (!target.gameObject.activeInHierarchy)
        {
            return false;
        }

        return true;
    }

    public void UpdateDistanceText(Transform player)
    {
        if (!showDistance)
        {
            if (distanceText != null)
            {
                distanceText.text = "";
            }

            return;
        }

        if (distanceText == null)
        {
            return;
        }

        if (player == null || target == null)
        {
            distanceText.text = "";
            return;
        }

        float distance = Vector3.Distance(player.position, target.position);
        distanceText.text = Mathf.RoundToInt(distance).ToString();
    }
}
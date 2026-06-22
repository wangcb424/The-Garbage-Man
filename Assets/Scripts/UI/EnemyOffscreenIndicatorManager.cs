using System.Collections.Generic;
using UnityEngine;

public class EnemyOffscreenIndicatorManager : MonoBehaviour
{
    [Header("Canvas")]
    public Canvas targetCanvas;
    public RectTransform indicatorParent;

    [Header("Camera")]
    public Camera targetCamera;

    [Header("Player")]
    public Transform player;
    public string playerTag = "Player";
    public bool autoFindPlayer = true;

    [Header("Enemy Search")]
    public string enemyTag = "Enemy";
    public float enemySearchInterval = 0.25f;
    public int maxIndicators = 12;

    [Header("Indicator Prefab")]
    public EnemyOffscreenIndicator indicatorPrefab;

    [Header("Screen Edge Settings")]
    public float screenEdgePadding = 60f;
    public bool hideWhenEnemyVisible = true;

    [Header("Arrow Rotation")]
    public float arrowRotationOffset = -90f;

    private readonly List<Transform> enemies = new List<Transform>();
    private readonly List<EnemyOffscreenIndicator> indicators = new List<EnemyOffscreenIndicator>();

    private float searchTimer = 0f;

    private void Start()
    {
        SetupReferences();
        SearchEnemies();
    }

    private void Update()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;

            if (targetCamera == null)
            {
                targetCamera = FindAnyObjectByType<Camera>();
            }
        }

        if (player == null && autoFindPlayer)
        {
            TryFindPlayer();
        }

        searchTimer -= Time.deltaTime;

        if (searchTimer <= 0f)
        {
            searchTimer = enemySearchInterval;
            SearchEnemies();
        }

        UpdateIndicators();
    }

    private void SetupReferences()
    {
        if (targetCanvas == null)
        {
            targetCanvas = GetComponentInParent<Canvas>();
        }

        if (targetCanvas == null)
        {
            targetCanvas = FindAnyObjectByType<Canvas>();
        }

        if (indicatorParent == null && targetCanvas != null)
        {
            indicatorParent = targetCanvas.GetComponent<RectTransform>();
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            targetCamera = FindAnyObjectByType<Camera>();
        }

        if (player == null && autoFindPlayer)
        {
            TryFindPlayer();
        }
    }

    private void TryFindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void SearchEnemies()
    {
        enemies.Clear();

        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag(enemyTag);

        for (int i = 0; i < enemyObjects.Length; i++)
        {
            if (enemyObjects[i] == null)
            {
                continue;
            }

            if (!enemyObjects[i].activeInHierarchy)
            {
                continue;
            }

            enemies.Add(enemyObjects[i].transform);
        }
    }

    private void UpdateIndicators()
    {
        if (targetCamera == null)
        {
            HideAllIndicators();
            return;
        }

        if (targetCanvas == null)
        {
            HideAllIndicators();
            return;
        }

        if (indicatorParent == null)
        {
            HideAllIndicators();
            return;
        }

        if (indicatorPrefab == null)
        {
            HideAllIndicators();
            return;
        }

        int activeIndicatorIndex = 0;

        for (int i = 0; i < enemies.Count; i++)
        {
            Transform enemy = enemies[i];

            if (enemy == null)
            {
                continue;
            }

            if (!enemy.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (activeIndicatorIndex >= maxIndicators)
            {
                break;
            }

            Vector3 viewportPosition = targetCamera.WorldToViewportPoint(enemy.position);

            bool enemyIsBehindCamera = viewportPosition.z < 0f;
            bool enemyIsVisible = IsViewportPositionVisible(viewportPosition);

            if (hideWhenEnemyVisible && enemyIsVisible && !enemyIsBehindCamera)
            {
                continue;
            }

            EnemyOffscreenIndicator indicator = GetIndicator(activeIndicatorIndex);
            indicator.Setup(enemy, targetCamera);

            UpdateIndicatorPositionAndRotation(
                indicator,
                enemy.position,
                enemyIsBehindCamera
            );

            indicator.UpdateDistanceText(player);

            activeIndicatorIndex++;
        }

        for (int i = activeIndicatorIndex; i < indicators.Count; i++)
        {
            indicators[i].Clear();
        }
    }

    private bool IsViewportPositionVisible(Vector3 viewportPosition)
    {
        if (viewportPosition.z < 0f)
        {
            return false;
        }

        if (viewportPosition.x < 0f)
        {
            return false;
        }

        if (viewportPosition.x > 1f)
        {
            return false;
        }

        if (viewportPosition.y < 0f)
        {
            return false;
        }

        if (viewportPosition.y > 1f)
        {
            return false;
        }

        return true;
    }

    private EnemyOffscreenIndicator GetIndicator(int index)
    {
        while (indicators.Count <= index)
        {
            EnemyOffscreenIndicator newIndicator = Instantiate(
                indicatorPrefab,
                indicatorParent
            );

            newIndicator.gameObject.SetActive(false);
            indicators.Add(newIndicator);
        }

        return indicators[index];
    }

    private void UpdateIndicatorPositionAndRotation(
        EnemyOffscreenIndicator indicator,
        Vector3 worldPosition,
        bool enemyIsBehindCamera
    )
    {
        Vector3 screenPosition = targetCamera.WorldToScreenPoint(worldPosition);

        if (enemyIsBehindCamera)
        {
            screenPosition.x = Screen.width - screenPosition.x;
            screenPosition.y = Screen.height - screenPosition.y;
        }

        Vector2 screenCenter = new Vector2(
            Screen.width * 0.5f,
            Screen.height * 0.5f
        );

        Vector2 directionFromCenter = new Vector2(
            screenPosition.x,
            screenPosition.y
        ) - screenCenter;

        if (directionFromCenter.sqrMagnitude < 0.01f)
        {
            directionFromCenter = Vector2.up;
        }

        directionFromCenter.Normalize();

        Vector2 edgePosition = GetScreenEdgePosition(
            screenCenter,
            directionFromCenter
        );

        RectTransform canvasRect = targetCanvas.GetComponent<RectTransform>();

        Vector2 localPoint;

        Camera uiCamera = null;

        if (targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = targetCanvas.worldCamera;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            edgePosition,
            uiCamera,
            out localPoint
        );

        indicator.rectTransform.anchoredPosition = localPoint;

        float angle = Mathf.Atan2(
            directionFromCenter.y,
            directionFromCenter.x
        ) * Mathf.Rad2Deg;

        indicator.rectTransform.localRotation = Quaternion.Euler(
            0f,
            0f,
            angle + arrowRotationOffset
        );
    }

    private Vector2 GetScreenEdgePosition(Vector2 screenCenter, Vector2 direction)
    {
        float minX = screenEdgePadding;
        float maxX = Screen.width - screenEdgePadding;
        float minY = screenEdgePadding;
        float maxY = Screen.height - screenEdgePadding;

        float tX = float.MaxValue;
        float tY = float.MaxValue;

        if (Mathf.Abs(direction.x) > 0.001f)
        {
            if (direction.x > 0f)
            {
                tX = (maxX - screenCenter.x) / direction.x;
            }
            else
            {
                tX = (minX - screenCenter.x) / direction.x;
            }
        }

        if (Mathf.Abs(direction.y) > 0.001f)
        {
            if (direction.y > 0f)
            {
                tY = (maxY - screenCenter.y) / direction.y;
            }
            else
            {
                tY = (minY - screenCenter.y) / direction.y;
            }
        }

        float t = Mathf.Min(tX, tY);

        return screenCenter + direction * t;
    }

    private void HideAllIndicators()
    {
        for (int i = 0; i < indicators.Count; i++)
        {
            indicators[i].Clear();
        }
    }
}
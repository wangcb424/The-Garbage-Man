using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungeonMinimapManager : MonoBehaviour
{
    [System.Serializable]
    private class MinimapRoom
    {
        public int roomIndex;
        public RectTransform rectTransform;
        public Image image;
        public TMP_Text markerText;

        public Vector3 worldCenter;
        public float worldWidth;
        public float worldHeight;

        public bool explored;
        public bool isStartRoom;
        public bool isShopRoom;
        public bool isExitRoom;
    }

    [Header("UI References")]
    public RectTransform mapContent;
    public RectTransform playerMarker;
    public Image roomIconPrefab;
    public Image corridorIconPrefab;

    [Header("Player")]
    public Transform player;
    public string playerTag = "Player";
    public bool autoFindPlayer = true;

    [Header("Map Movement")]
    public float worldToMapScale = 1.4f;
    public bool centerOnPlayer = true;
    public float minimumRoomIconSize = 18f;
    public float corridorThickness = 8f;

    [Header("Room Explore Colors")]
    public Color unexploredRoomColor = new Color(1f, 1f, 1f, 0.95f);
    public Color exploredRoomColor = new Color(0.16f, 0.16f, 0.16f, 0.75f);

    [Header("Corridor Color")]
    public Color corridorColor = new Color(0.65f, 0.65f, 0.65f, 0.85f);

    [Header("Special Room Symbols")]
    public bool showSpecialRoomSymbols = true;
    public string specialRoomSymbol = "!";
    public string exitRoomSymbol = "◎";
    public string startRoomSymbol = "";
    public Color symbolColor = Color.black;
    public int symbolFontSize = 24;

    private Dictionary<int, MinimapRoom> rooms = new Dictionary<int, MinimapRoom>();
    private List<Image> corridorIcons = new List<Image>();

    private Vector3 originWorldPosition;
    private bool hasOrigin = false;

    private void Start()
    {
        TryFindPlayer();
    }

    private void Update()
    {
        if (player == null && autoFindPlayer)
        {
            TryFindPlayer();
        }

        UpdatePlayerMarkerAndMapMovement();
    }

    public void ClearMap()
    {
        if (mapContent != null)
        {
            for (int i = mapContent.childCount - 1; i >= 0; i--)
            {
                Destroy(mapContent.GetChild(i).gameObject);
            }
        }

        rooms.Clear();
        corridorIcons.Clear();

        hasOrigin = false;
    }

    public void RegisterRoom(
        int roomIndex,
        Vector3 worldCenter,
        float worldWidth,
        float worldHeight,
        bool isStartRoom,
        bool isShopRoom,
        bool isExitRoom
    )
    {
        if (mapContent == null)
        {
            Debug.Log("DungeonMinimapManager needs Map Content.");
            return;
        }

        if (roomIconPrefab == null)
        {
            Debug.Log("DungeonMinimapManager needs Room Icon Prefab.");
            return;
        }

        if (!hasOrigin)
        {
            originWorldPosition = worldCenter;
            hasOrigin = true;
        }

        if (rooms.ContainsKey(roomIndex))
        {
            return;
        }

        Image newRoomImage = Instantiate(roomIconPrefab, mapContent);
        newRoomImage.gameObject.name = "MinimapRoom_" + roomIndex;
        newRoomImage.gameObject.SetActive(true);
        newRoomImage.raycastTarget = false;

        RectTransform roomRect = newRoomImage.GetComponent<RectTransform>();

        roomRect.anchoredPosition = WorldToMapPosition(worldCenter);

        float iconWidth = Mathf.Max(minimumRoomIconSize, worldWidth * worldToMapScale);
        float iconHeight = Mathf.Max(minimumRoomIconSize, worldHeight * worldToMapScale);

        roomRect.sizeDelta = new Vector2(iconWidth, iconHeight);

        MinimapRoom minimapRoom = new MinimapRoom();

        minimapRoom.roomIndex = roomIndex;
        minimapRoom.rectTransform = roomRect;
        minimapRoom.image = newRoomImage;
        minimapRoom.worldCenter = worldCenter;
        minimapRoom.worldWidth = worldWidth;
        minimapRoom.worldHeight = worldHeight;
        minimapRoom.isStartRoom = isStartRoom;
        minimapRoom.isShopRoom = isShopRoom;
        minimapRoom.isExitRoom = isExitRoom;

        // Start room starts as explored.
        minimapRoom.explored = isStartRoom;

        minimapRoom.markerText = CreateRoomSymbolText(newRoomImage.transform, minimapRoom);

        rooms.Add(roomIndex, minimapRoom);

        RefreshRoomVisual(roomIndex);
    }

    public void RegisterCorridor(Vector3 startWorldPosition, Vector3 endWorldPosition)
    {
        if (mapContent == null)
        {
            return;
        }

        if (corridorIconPrefab == null)
        {
            corridorIconPrefab = roomIconPrefab;
        }

        if (corridorIconPrefab == null)
        {
            Debug.Log("DungeonMinimapManager needs Corridor Icon Prefab or Room Icon Prefab.");
            return;
        }

        if (!hasOrigin)
        {
            originWorldPosition = startWorldPosition;
            hasOrigin = true;
        }

        Vector2 startMapPosition = WorldToMapPosition(startWorldPosition);
        Vector2 endMapPosition = WorldToMapPosition(endWorldPosition);

        Vector2 middlePosition = (startMapPosition + endMapPosition) * 0.5f;
        Vector2 direction = endMapPosition - startMapPosition;

        float length = direction.magnitude;

        if (length <= 0.01f)
        {
            return;
        }

        Image corridorImage = Instantiate(corridorIconPrefab, mapContent);
        corridorImage.gameObject.name = "MinimapCorridor";
        corridorImage.gameObject.SetActive(true);
        corridorImage.color = corridorColor;
        corridorImage.raycastTarget = false;

        RectTransform corridorRect = corridorImage.GetComponent<RectTransform>();

        corridorRect.anchoredPosition = middlePosition;
        corridorRect.sizeDelta = new Vector2(length, corridorThickness);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        corridorRect.localRotation = Quaternion.Euler(0f, 0f, angle);

        // Corridors stay behind rooms.
        corridorRect.SetAsFirstSibling();

        corridorIcons.Add(corridorImage);
    }

    public void RegisterCorridorLShape(
        Vector3 startWorldPosition,
        Vector3 cornerWorldPosition,
        Vector3 endWorldPosition
    )
    {
        RegisterCorridor(startWorldPosition, cornerWorldPosition);
        RegisterCorridor(cornerWorldPosition, endWorldPosition);
    }

    public void MarkRoomExplored(int roomIndex)
    {
        if (!rooms.ContainsKey(roomIndex))
        {
            return;
        }

        rooms[roomIndex].explored = true;

        RefreshRoomVisual(roomIndex);
    }

    public void ClearCurrentRoom(int roomIndex)
    {
        // This is intentionally empty now.
        // Room color is based only on explored / unexplored.
    }

    private void RefreshRoomVisual(int roomIndex)
    {
        if (!rooms.ContainsKey(roomIndex))
        {
            return;
        }

        MinimapRoom room = rooms[roomIndex];

        if (room.image == null)
        {
            return;
        }

        if (room.explored)
        {
            room.image.color = exploredRoomColor;
        }
        else
        {
            room.image.color = unexploredRoomColor;
        }

        if (room.markerText != null)
        {
            room.markerText.color = symbolColor;
        }
    }

    private TMP_Text CreateRoomSymbolText(Transform parent, MinimapRoom room)
    {
        if (!showSpecialRoomSymbols)
        {
            return null;
        }

        string symbol = GetRoomSymbol(room);

        if (string.IsNullOrEmpty(symbol))
        {
            return null;
        }

        GameObject symbolObject = new GameObject("RoomSymbol");
        symbolObject.transform.SetParent(parent, false);

        RectTransform symbolRect = symbolObject.AddComponent<RectTransform>();

        symbolRect.anchorMin = Vector2.zero;
        symbolRect.anchorMax = Vector2.one;
        symbolRect.offsetMin = Vector2.zero;
        symbolRect.offsetMax = Vector2.zero;

        TMP_Text text = symbolObject.AddComponent<TextMeshProUGUI>();

        text.text = symbol;
        text.fontSize = symbolFontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = symbolColor;
        text.raycastTarget = false;

        return text;
    }

    private string GetRoomSymbol(MinimapRoom room)
    {
        if (room.isExitRoom)
        {
            return exitRoomSymbol;
        }

        if (room.isShopRoom)
        {
            return specialRoomSymbol;
        }

        if (room.isStartRoom)
        {
            return startRoomSymbol;
        }

        return "";
    }

    private void UpdatePlayerMarkerAndMapMovement()
    {
        if (player == null)
        {
            return;
        }

        if (playerMarker == null)
        {
            return;
        }

        if (mapContent == null)
        {
            return;
        }

        if (!hasOrigin)
        {
            return;
        }

        Vector2 playerMapPosition = WorldToMapPosition(player.position);

        if (centerOnPlayer)
        {
            // MinimapPanel frame does not move.
            // PlayerMarker stays in the center.
            // Only MapContent moves.
            playerMarker.anchoredPosition = Vector2.zero;
            mapContent.anchoredPosition = -playerMapPosition;
        }
        else
        {
            mapContent.anchoredPosition = Vector2.zero;
            playerMarker.anchoredPosition = playerMapPosition;
        }
    }

    private Vector2 WorldToMapPosition(Vector3 worldPosition)
    {
        Vector3 offset = worldPosition - originWorldPosition;

        return new Vector2(
            offset.x * worldToMapScale,
            offset.z * worldToMapScale
        );
    }

    private void TryFindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }
}
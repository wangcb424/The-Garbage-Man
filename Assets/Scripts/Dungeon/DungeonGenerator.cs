using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//This class stores the offset of a single floor prefab in a group floor prefab
//这个class用来保存单个地板在一个大地板组合的位置
[System.Serializable]
public class FloorTileOffset
{
    public Vector2Int offset;
}

// This class stores one floor prefab and its spawn weight.
// 这个 class 用来保存一个地板 prefab 和它的生成权重
[System.Serializable]
public class WeightedFloorPrefab
{
    public GameObject floorPrefab;
    public float weight = 1f;

    // If true, this prefab is treated as a special floor.
    // 如果为 true，这个 prefab 会被当成特殊地板处理
    // Special floors only appear after the level difficulty starts increasing.
    // 特殊地板只会在关卡难度开始提升后出现
    public bool scalesWithDifficulty = false;

    // If this prefab is a group, list all tile offsets it occupies.
    public List<FloorTileOffset> groupOffsets = new List<FloorTileOffset>();
}

// This class stores one wall prefab and its spawn weight.
// 这个 class 用来保存一个墙体 prefab 和它的生成权重
[System.Serializable]
public class WeightedWallPrefab
{
    public GameObject wallPrefab;
    public float weight = 1f;

    // If true, this prefab becomes more common as level difficulty increases.
    // 如果为 true，这个 prefab 会随着关卡难度提高而更常出现
    public bool scalesWithDifficulty = false;
}

public class DungeonGenerator : MonoBehaviour
{
    // Floor prefabs are selected based on room rules and difficulty.
    // 地板 prefab 会根据房间规则和难度来选择
    public List<WeightedFloorPrefab> floorPrefabs = new List<WeightedFloorPrefab>();

    // Wall prefabs are selected randomly based on weight and difficulty.
    // 墙体 prefab 会根据权重和难度随机选择
    public List<WeightedWallPrefab> wallPrefabs = new List<WeightedWallPrefab>();

    // The portal prefab placed in the final room.
    // 放在最后一个房间里的传送门 prefab
    public GameObject exitPortalPrefab;

    // The door blocker prefab used to close rooms during fights.
    // 战斗时用于封住房间门口的门 prefab
    public GameObject doorBlockerPrefab;

    [Header("Reward Chest")]

    // The reward chest prefab spawned after a normal combat room is cleared.
    // 普通战斗房清完敌人后生成的奖励宝箱 prefab
    public GameObject rewardChestPrefab;

    // If true, generated rooms can spawn reward chests after enemies are cleared.
    // 如果为 true，生成出来的普通战斗房清完敌人后会生成奖励宝箱
    public bool enableRewardChestSpawning = true;

    // Enemy tag used by the reward chest spawner when checking if the room is cleared.
    // 奖励宝箱刷新器检测房间敌人时使用的 enemy tag
    public string enemyTagForRewardChest = "Enemy";

    // Chest spawn offset from the room center.
    // 宝箱相对房间中心的生成偏移
    public Vector3 rewardChestSpawnOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Minimap")]

    // If true, generated rooms will be registered on the minimap.
    // 如果为 true，生成出来的房间会注册到小地图上
    public bool enableMinimap = true;

    // Minimap manager in the dungeon HUD. Can be assigned in the Inspector.
    // Dungeon HUD 里的小地图管理器，可以在 Inspector 里拖入
    public DungeonMinimapManager minimapManager;

    // Player tag used by minimap room trackers.
    // 小地图房间检测玩家时使用的 tag
    public string playerTagForMinimap = "Player";

    // Enemy prefabs passed into each room spawner.
    // 传给每个房间 enemy spawner 的敌人 prefab 列表
    public List<WeightedEnemyPrefab> enemyPrefabs = new List<WeightedEnemyPrefab>();

    [Header("Special Rooms")]

    // Shop object placed inside the shop room.
    // 放在商店房间里的商店 prefab
    public GameObject shopRoomPrefab;

    // Minimum room steps from start room for shop and exit.
    // 商店和出口距离初始房间的最小房间步数
    public int minSpecialRoomStepsFromStart = 2;

    // If true, shop room will not spawn enemies.
    // 如果为 true，商店房间不会生成敌人
    public bool shopRoomIsSafe = true;

    // If true, exit room will not spawn enemies.
    // 如果为 true，出口房间不会生成敌人
    public bool exitRoomIsSafe = true;

    // Room index for shop room.
    // 商店房间 index
    private int shopRoomIndex = -1;

    // Room index for exit room.
    // 出口房间 index
    private int exitRoomIndex = -1;

    // Dungeon layout settings.
    // 地牢布局设置
    // The generator randomly chooses how many rooms to create.
    // 生成器会随机决定这一层要创建多少个房间
    public int minRoomCount = 4;
    public int maxRoomCount = 9;

    // All rooms use the same size.
    // 所有房间使用相同大小
    public int roomWidth = 10;
    public int roomHeight = 10;

    // Distance between room centers.
    // 房间中心点之间的距离
    // This should be larger than roomWidth and roomHeight so rooms do not overlap.
    // 这个值应该大于房间宽高，避免房间互相重叠
    public int roomSpacing = 22;

    // Size of each grid tile in world units.
    // 每个网格格子对应 Unity 世界坐标里的大小
    public float tileSize = 3f;

    // Chance for nearby rooms to get an extra connection.
    // 相邻房间额外连接的概率
    // Higher value means more branching and looping paths.
    // 数值越高，地图分支和环路越多
    public float extraConnectionChance = 0.35f;

    // Level difficulty settings.
    // 关卡难度设置
    // If true, the script uses the current scene build index as the level number.
    // 如果为 true，脚本会用当前 scene 的 build index 当作关卡数
    public bool useSceneIndexAsLevelNumber = true;

    // If useSceneIndexAsLevelNumber is false, this value is used instead.
    // 如果 useSceneIndexAsLevelNumber 为 false，就使用这个手动关卡数
    public int currentLevelNumber = 1;

    // Special effects start from this level.
    // 特殊效果从这一关开始出现
    // If this is 2, Level 1 has no special effects.
    // 如果这个值是 2，那么 Level 1 完全没有特殊效果
    public int firstSpecialEffectLevel = 2;

    // Difficulty reaches the maximum at this level.
    // 难度会在这一关达到上限
    // After this level, difficulty stays capped at 1.
    // 这一关之后，难度会保持在最高值
    public int maxDifficultyLevel = 5;

    // Normal floor/wall weight becomes weaker at max difficulty.
    // 最大难度时，普通地板和普通墙的权重会降低
    public float normalWeightAtMaxDifficulty = 0.25f;

    // Special floor/wall weight becomes stronger at max difficulty.
    // 最大难度时，特殊地板和特殊墙的权重会提高
    public float specialWeightAtMaxDifficulty = 4f;

    // Special floor limit settings.
    // 特殊地板数量限制设置
    // Corridors will always use normal floor.
    // 走廊永远使用普通地板
    // Rooms only get a limited number of special floor tiles.
    // 房间里的特殊地板数量会被限制
    public int minSpecialFloorsPerRoom = 0;
    public int maxSpecialFloorsPerRoom = 15;

    // Special floors should not appear too close to room edges or center.
    // 特殊地板不要太靠近房间边缘或房间中心
    public int specialFloorEdgePadding = 1;
    public int specialFloorCenterClearRadius = 1;

    // Enemy count settings.
    // 敌人数量设置
    public int minEnemiesPerRoom = 1;
    public int maxEnemiesPerRoom = 3;
    public int extraEnemiesAtMaxDifficulty = 4;
    public int enemySpawnSpacing = 2;

    // First room settings.
    // 第一个房间的设置
    public bool firstRoomIsSafe = true;

    // Room inner wall settings.
    // 房间内部墙体设置
    public int minInnerWallsPerRoom = 0;
    public int maxInnerWallsPerRoom = 8;
    public bool firstRoomHasInnerWalls = false;

    // Keep obstacles away from room edges and room center.
    // 让障碍物远离房间边缘和房间中心
    public int innerWallEdgePadding = 2;
    public int centerClearRadius = 2;

    // Random seed settings.
    // 随机种子设置
    public bool useRandomSeed = true;
    public int seed = 0;

    // If true, this generator creates a dungeon by itself when the scene starts.
    public bool generateOnStart = false;

    // Stores all grid positions where floor exists.
    // 保存所有有地板的网格坐标
    private HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();

    // Stores floor positions that belong to rooms.
    // 保存属于房间区域的地板坐标
    // Corridors are not included here.
    // 走廊地板不会存到这里
    private HashSet<Vector2Int> roomFloorPositions = new HashSet<Vector2Int>();

    // Stores floor positions that should become special floor tiles.
    // 保存应该变成特殊地板的格子坐标
    private HashSet<Vector2Int> specialFloorPositions = new HashSet<Vector2Int>();

    // Stores internal wall positions inside rooms.
    // 保存房间内部墙体的坐标
    private HashSet<Vector2Int> innerWallPositions = new HashSet<Vector2Int>();

    // Difficulty value for each floor position.
    // 保存每个地板位置对应的难度值
    private Dictionary<Vector2Int, float> floorDifficultyByPosition = new Dictionary<Vector2Int, float>();

    // Difficulty value for each wall position.
    // 保存每个墙体位置对应的难度值
    private Dictionary<Vector2Int, float> wallDifficultyByPosition = new Dictionary<Vector2Int, float>();

    // Stores room data created during generation.
    // 保存生成过程中创建的房间数据
    private List<RoomData> rooms = new List<RoomData>();

    // Stores all room connections.
    // 保存所有房间连接关系
    // A room can have more than one connection.
    // 一个房间可以有多个连接
    private List<RoomConnection> roomConnections = new List<RoomConnection>();

    // Stores generated enemy spawners.
    // 保存生成出来的 enemy spawner
    private List<DungeonEnemySpawner> enemySpawners = new List<DungeonEnemySpawner>();

    // Stores generated room controllers.
    // 保存生成出来的 room controller
    private List<DungeonRoomController> roomControllers = new List<DungeonRoomController>();

    // Four grid directions used to place rooms and check neighboring tiles.
    // 用于生成房间和检查相邻格子的四个方向
    private Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    // Simple data class for room information.
    // 用来保存房间信息的简单 class
    private class RoomData
    {
        public Vector2Int center;
        public int width;
        public int height;

        public RoomData(Vector2Int newCenter, int newWidth, int newHeight)
        {
            center = newCenter;
            width = newWidth;
            height = newHeight;
        }
    }

    // Stores a connection between two rooms.
    // 保存两个房间之间的一条连接
    // This is used to create corridors and doors.
    // 这个会被用来生成走廊和门
    private class RoomConnection
    {
        public int roomAIndex;
        public int roomBIndex;

        public RoomConnection(int newRoomAIndex, int newRoomBIndex)
        {
            roomAIndex = newRoomAIndex;
            roomBIndex = newRoomBIndex;
        }
    }

    private void Start()
    {
        // Usually this should be false when using DungeonRunManager.
        // 使用 DungeonRunManager 时通常应该设成 false
        if (generateOnStart)
        {
            GenerateDungeon();
        }
    }
    public void GenerateDungeon()
    {
        // Remove previously generated dungeon objects.
        // 删除之前生成出来的旧地牢物体
        ClearOldDungeon();

        // Setup and clear the minimap before registering the newly generated rooms.
        // 生成新房间前，先找到并清空小地图
        SetupMinimapManager();

        if (minimapManager != null)
        {
            minimapManager.ClearMap();
        }

        // If useRandomSeed is false, use a fixed seed for repeatable maps.
        // 如果不使用随机种子，就用固定 seed 生成可重复地图
        if (!useRandomSeed)
        {
            Random.InitState(seed);
        }

        // Reset all stored generation data.
        // 清理所有之前的地牢信息
        floorPositions.Clear();
        roomFloorPositions.Clear();
        specialFloorPositions.Clear();
        innerWallPositions.Clear();
        floorDifficultyByPosition.Clear();
        wallDifficultyByPosition.Clear();
        rooms.Clear();
        roomConnections.Clear();
        enemySpawners.Clear();
        roomControllers.Clear();

        // Reset special room data.
        // 重置特殊房间数据
        shopRoomIndex = -1;
        exitRoomIndex = -1;

        // Main dungeon generation sequence.
        // 地牢生成的主要顺序
        CreateRooms();
        ConnectRooms();
        //CreateInnerWalls();
        CreateSpecialFloors();
        CreateFloorTiles();
        CreateWallTiles();
        //CreateInnerWallTiles();
        CreateEnemySpawners();
        CreateRoomControllers();
        CreateDoors();
        // CreateShopRoomObject();
        CreateExitPortal();
    }

    private void ClearOldDungeon()
    {
        List<GameObject> oldObjects = new List<GameObject>();

        // Store children first so we do not modify the transform while looping.
        // 先存子物体，防止一边遍历一边删除出问题
        foreach (Transform child in transform)
        {
            oldObjects.Add(child.gameObject);
        }

        // Destroy all old generated children.
        // 删除所有旧的生成物体
        for (int i = 0; i < oldObjects.Count; i++)
        {
            Destroy(oldObjects[i]);
        }
    }

    private void CreateRooms()
    {
        // Pick a random room count for this dungeon.
        // 随机决定这一层的房间数量
        int generatedRoomCount = Random.Range(minRoomCount, maxRoomCount + 1);

        // Prevent two rooms from using the same center position.
        // 记录每个房间中心，防止重复生成在同一个位置
        HashSet<Vector2Int> usedCenters = new HashSet<Vector2Int>();

        // The first room always starts at the center.
        // 确保起始房间在地图中心
        RoomData firstRoom = new RoomData(Vector2Int.zero, roomWidth, roomHeight);
        rooms.Add(firstRoom);
        usedCenters.Add(firstRoom.center);
        AddRoomFloor(firstRoom, 0);

        // Avoid looping forever.
        // 避免随机生成一直失败导致死循环
        int safetyCount = 0;

        // Keep creating rooms until we reach the chosen room count.
        // 持续创建房间，直到达到随机选出的房间数量
        while (rooms.Count < generatedRoomCount && safetyCount < generatedRoomCount * 100)
        {
            // Pick an existing room as the parent room.
            // 随机选择一个已有房间作为父房间
            int parentRoomIndex = Random.Range(0, rooms.Count);
            RoomData parentRoom = rooms[parentRoomIndex];

            // Do not let the start room connect to more than one room.
            // 不让初始房间连接超过一个房间
            if (parentRoomIndex == 0 && CountRoomConnections(0) >= 1)
            {
                safetyCount++;
                continue;
            }

            // Pick a random direction from the parent room.
            // 从父房间随机选择一个生成方向
            Vector2Int direction = directions[Random.Range(0, directions.Length)];

            // Decide the new room center.
            // 根据方向和房间间距决定新房间中心
            Vector2Int newCenter = parentRoom.center + direction * roomSpacing;

            // If this position is already used, try again.
            // 如果这个位置已经被使用，就跳过这次尝试
            if (usedCenters.Contains(newCenter))
            {
                safetyCount++;
                continue;
            }

            // Create the new room.
            // 创建新房间
            RoomData newRoom = new RoomData(newCenter, roomWidth, roomHeight);
            rooms.Add(newRoom);
            usedCenters.Add(newCenter);

            int newRoomIndex = rooms.Count - 1;
            AddRoomFloor(newRoom, newRoomIndex);

            // Connect the new room to its parent.
            // 把新房间和它的父房间连接起来
            AddRoomConnection(parentRoomIndex, newRoomIndex);

            safetyCount++;
        }

        // Add optional extra connections after all rooms are created.
        // 添加额外房间连接，后面会根据这些连接生成道路和门
        CreateExtraRoomConnections();

        // Select special rooms after the room graph is finished.
        // 房间连接完成后选择特殊房间
        SelectSpecialRooms();
    }

    private void AddRoomFloor(RoomData room, int roomIndex)
    {
        // Convert room center and size into a rectangular grid area.
        // 根据房间中心和宽高，算出房间在 2D 网格里的矩形范围
        int left = room.center.x - room.width / 2;
        int right = room.center.x + room.width / 2;
        int bottom = room.center.y - room.height / 2;
        int top = room.center.y + room.height / 2;

        // Calculate room difficulty.
        // 计算当前房间难度
        float roomDifficulty = GetRoomDifficulty(roomIndex);

        // Add every tile inside the rectangle as a floor position.
        // 把房间范围内的每个格子记录为地板位置
        for (int x = left; x <= right; x++)
        {
            for (int y = bottom; y <= top; y++)
            {
                Vector2Int position = new Vector2Int(x, y);

                AddFloorPosition(position, roomDifficulty);

                // Mark this position as room floor.
                // 把这个位置标记为房间地板
                // Corridors will not be added to this set.
                // 走廊不会被加到这个 set 里
                roomFloorPositions.Add(position);
            }
        }
    }

    private void AddFloorPosition(Vector2Int position, float difficulty)
    {
        // Store floor position.
        // 保存地板坐标
        floorPositions.Add(position);

        // Store the highest difficulty if the position already exists.
        // 如果这个位置已经存在，就保留更高的难度值
        if (!floorDifficultyByPosition.ContainsKey(position))
        {
            floorDifficultyByPosition.Add(position, difficulty);
        }
        else if (difficulty > floorDifficultyByPosition[position])
        {
            floorDifficultyByPosition[position] = difficulty;
        }
    }

    private void AddRoomConnection(int roomAIndex, int roomBIndex)
    {
        // Do not connect a room to itself.
        // 防止房间连接自己
        if (roomAIndex == roomBIndex)
        {
            return;
        }

        // Avoid duplicate connections.
        // 防止重复连接
        for (int i = 0; i < roomConnections.Count; i++)
        {
            bool sameDirection =
                roomConnections[i].roomAIndex == roomAIndex &&
                roomConnections[i].roomBIndex == roomBIndex;

            bool oppositeDirection =
                roomConnections[i].roomAIndex == roomBIndex &&
                roomConnections[i].roomBIndex == roomAIndex;

            if (sameDirection || oppositeDirection)
            {
                return;
            }
        }

        roomConnections.Add(new RoomConnection(roomAIndex, roomBIndex));
    }

    private void CreateExtraRoomConnections()
    {
        // Check every pair of rooms.
        // 检查每一对房间
        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                // Do not add extra connections to the start room.
                // 不给初始房间添加额外连接
                if (i == 0 || j == 0)
                {
                    continue;
                }

                // Random chance to create an extra connection.
                // 按概率随机添加额外连接
                if (Random.value > extraConnectionChance)
                {
                    continue;
                }

                // Only connect rooms that are exactly adjacent in the room grid.
                // 只连接刚好相邻的房间
                int gridDistance =
                    Mathf.Abs(rooms[i].center.x - rooms[j].center.x) +
                    Mathf.Abs(rooms[i].center.y - rooms[j].center.y);

                if (gridDistance == roomSpacing)
                {
                    AddRoomConnection(i, j);
                }
            }
        }
    }

    private void SelectSpecialRooms()
    {
        // Select exit first because the shop should not use the same room.
        // 先选择出口房，因为商店不能和出口重合
        exitRoomIndex = GetBestExitRoomIndex();

        // Select shop after exit.
        // 再选择商店房
        shopRoomIndex = GetBestShopRoomIndex();

        Debug.Log("Exit Room Index: " + exitRoomIndex);
        Debug.Log("Shop Room Index: " + shopRoomIndex);
    }

    private int GetBestExitRoomIndex()
    {
        int bestRoomIndex = -1;
        int bestDistance = -1;

        for (int i = 1; i < rooms.Count; i++)
        {
            int roomStepDistance = GetRoomStepDistanceFromStart(i);

            // Exit room should not be too close to start room.
            // 出口房不要离初始房太近
            if (roomStepDistance < minSpecialRoomStepsFromStart)
            {
                continue;
            }

            // Exit room should be a dead-end room.
            // 出口房应该是只连接一个房间的死路房间
            if (CountRoomConnections(i) != 1)
            {
                continue;
            }

            if (roomStepDistance > bestDistance)
            {
                bestDistance = roomStepDistance;
                bestRoomIndex = i;
            }
        }

        // Fallback if no perfect exit room exists.
        // 如果没有完美出口房，就用离初始房最远的房间
        if (bestRoomIndex == -1)
        {
            bestRoomIndex = GetFarthestRoomIndexFromStart();
        }

        return bestRoomIndex;
    }

    private int GetBestShopRoomIndex()
    {
        List<int> candidates = new List<int>();

        for (int i = 1; i < rooms.Count; i++)
        {
            // Shop cannot be the same room as exit.
            // 商店不能和出口在同一个房间
            if (i == exitRoomIndex)
            {
                continue;
            }

            int roomStepDistance = GetRoomStepDistanceFromStart(i);

            // Shop should not be too close to start room.
            // 商店不要离初始房太近
            if (roomStepDistance < minSpecialRoomStepsFromStart)
            {
                continue;
            }

            candidates.Add(i);
        }

        if (candidates.Count == 0)
        {
            return -1;
        }

        int randomIndex = Random.Range(0, candidates.Count);
        return candidates[randomIndex];
    }

    private int GetFarthestRoomIndexFromStart()
    {
        if (rooms.Count <= 1)
        {
            return -1;
        }

        int bestRoomIndex = 1;
        int bestDistance = -1;

        for (int i = 1; i < rooms.Count; i++)
        {
            int roomStepDistance = GetRoomStepDistanceFromStart(i);

            if (roomStepDistance > bestDistance)
            {
                bestDistance = roomStepDistance;
                bestRoomIndex = i;
            }
        }

        return bestRoomIndex;
    }

    private int GetRoomStepDistanceFromStart(int roomIndex)
    {
        if (roomIndex < 0 || roomIndex >= rooms.Count)
        {
            return 0;
        }

        if (rooms.Count == 0)
        {
            return 0;
        }

        Vector2Int startCenter = rooms[0].center;
        Vector2Int roomCenter = rooms[roomIndex].center;

        int gridDistance =
            Mathf.Abs(roomCenter.x - startCenter.x) +
            Mathf.Abs(roomCenter.y - startCenter.y);

        if (roomSpacing <= 0)
        {
            return 0;
        }

        return Mathf.RoundToInt(gridDistance / (float)roomSpacing);
    }

    private int CountRoomConnections(int roomIndex)
    {
        int connectionCount = 0;

        for (int i = 0; i < roomConnections.Count; i++)
        {
            if (roomConnections[i].roomAIndex == roomIndex ||
                roomConnections[i].roomBIndex == roomIndex)
            {
                connectionCount++;
            }
        }

        return connectionCount;
    }

    private void ConnectRooms()
    {
        // Create corridors for every saved room connection.
        // 遍历所有房间连接，并为每一组连接生成走廊
        for (int i = 0; i < roomConnections.Count; i++)
        {
            // Find the two rooms connected by this connection.
            // 根据连接里保存的房间 index，找到两个房间的数据
            RoomData roomA = rooms[roomConnections[i].roomAIndex];
            RoomData roomB = rooms[roomConnections[i].roomBIndex];

            // Use the room centers as corridor start and end points.
            // 用两个房间中心作为走廊起点和终点
            Vector2Int start = roomA.center;
            Vector2Int end = roomB.center;

            // Corridors do not use the difficulty system.
            // 走廊不参与难度系统
            // This keeps corridor floors normal and corridor walls normal.
            // 这样走廊地板永远普通，走廊旁边的墙也不会变成特殊墙
            float corridorDifficulty = 0f;

            // Randomly choose whether the L-shaped corridor goes horizontal first or vertical first.
            // 随机决定 L 形走廊是先横向再纵向，还是先纵向再横向
            bool horizontalFirst = Random.value > 0.5f;

            if (horizontalFirst)
            {
                CreateHorizontalCorridor(start.x, end.x, start.y, corridorDifficulty);
                CreateVerticalCorridor(start.y, end.y, end.x, corridorDifficulty);

                Vector2Int corner = new Vector2Int(end.x, start.y);
                RegisterCorridorOnMinimap(start, corner, end);
            }
            else
            {
                CreateVerticalCorridor(start.y, end.y, start.x, corridorDifficulty);
                CreateHorizontalCorridor(start.x, end.x, end.y, corridorDifficulty);

                Vector2Int corner = new Vector2Int(start.x, end.y);
                RegisterCorridorOnMinimap(start, corner, end);
            }
        }
    }

    private void CreateHorizontalCorridor(int startX, int endX, int y, float difficulty)
    {
        int direction = startX < endX ? 1 : -1;

        // Add floor positions in a horizontal line.
        // 横向添加走廊地板位置
        for (int x = startX; x != endX + direction; x += direction)
        {
            AddFloorPosition(new Vector2Int(x, y), difficulty);

            // Add a second row so the corridor is wider.
            // 额外添加第二行，让走廊更宽
            AddFloorPosition(new Vector2Int(x, y + 1), difficulty);
        }
    }

    private void CreateVerticalCorridor(int startY, int endY, int x, float difficulty)
    {
        int direction = startY < endY ? 1 : -1;

        // Add floor positions in a vertical line.
        // 纵向添加走廊地板位置
        for (int y = startY; y != endY + direction; y += direction)
        {
            AddFloorPosition(new Vector2Int(x, y), difficulty);

            // Add a second column so the corridor is wider.
            // 额外添加第二列，让走廊更宽
            AddFloorPosition(new Vector2Int(x + 1, y), difficulty);
        }
    }

    private void CreateInnerWalls()
    {
        // Create walls inside rooms.
        // 在房间内部创建墙体障碍
        for (int i = 0; i < rooms.Count; i++)
        {
            // The first room can stay open and simple.
            // 第一个房间可以保持开阔简单
            if (i == 0 && !firstRoomHasInnerWalls)
            {
                continue;
            }

            float difficulty = GetRoomDifficulty(i);

            int obstacleCount = Mathf.RoundToInt(
                Mathf.Lerp(minInnerWallsPerRoom, maxInnerWallsPerRoom, difficulty)
            );

            RoomData room = rooms[i];

            for (int j = 0; j < obstacleCount; j++)
            {
                TryCreateInnerWall(room);
            }
        }
    }

    private void TryCreateInnerWall(RoomData room)
    {
        int left = room.center.x - room.width / 2 + innerWallEdgePadding;
        int right = room.center.x + room.width / 2 - innerWallEdgePadding;
        int bottom = room.center.y - room.height / 2 + innerWallEdgePadding;
        int top = room.center.y + room.height / 2 - innerWallEdgePadding;

        if (left > right || bottom > top)
        {
            return;
        }

        int randomX = Random.Range(left, right + 1);
        int randomY = Random.Range(bottom, top + 1);

        Vector2Int position = new Vector2Int(randomX, randomY);

        // Do not place walls too close to the center of the room.
        // 不要把内部墙生成得太靠近房间中心
        if (Vector2Int.Distance(position, room.center) <= centerClearRadius)
        {
            return;
        }

        // Only place inner walls on floor positions.
        // 内部墙只能生成在地板位置上
        if (!floorPositions.Contains(position))
        {
            return;
        }

        // Avoid duplicate inner walls.
        // 避免重复生成内部墙
        if (innerWallPositions.Contains(position))
        {
            return;
        }

        innerWallPositions.Add(position);
    }

    private void CreateSpecialFloors()
    {
        // Pick limited special floor positions inside each room.
        // 在每个房间内选择有限数量的特殊地板位置
        for (int i = 0; i < rooms.Count; i++)
        {
            RoomData room = rooms[i];
            float difficulty = GetRoomDifficulty(i);

            // If difficulty is 0, this level should have no special floor.
            // 如果难度是 0，这一关不应该有特殊地板
            if (difficulty <= 0f)
            {
                continue;
            }

            // Keep the first room clean.
            // 保持第一个房间干净
            if (i == 0)
            {
                continue;
            }

            int specialFloorCount = Mathf.RoundToInt(
                Mathf.Lerp(minSpecialFloorsPerRoom, maxSpecialFloorsPerRoom, difficulty)
            );

            List<Vector2Int> candidates = GetSpecialFloorCandidates(room);

            for (int j = 0; j < specialFloorCount && candidates.Count > 0; j++)
            {
                int randomIndex = Random.Range(0, candidates.Count);
                Vector2Int selectedPosition = candidates[randomIndex];

                specialFloorPositions.Add(selectedPosition);
                candidates.RemoveAt(randomIndex);
            }
        }
    }

    private List<Vector2Int> GetSpecialFloorCandidates(RoomData room)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        int left = room.center.x - room.width / 2 + specialFloorEdgePadding;
        int right = room.center.x + room.width / 2 - specialFloorEdgePadding;
        int bottom = room.center.y - room.height / 2 + specialFloorEdgePadding;
        int top = room.center.y + room.height / 2 - specialFloorEdgePadding;

        for (int x = left; x <= right; x++)
        {
            for (int y = bottom; y <= top; y++)
            {
                Vector2Int position = new Vector2Int(x, y);

                // Only room floor can become special.
                // 只有房间地板可以变成特殊地板
                // This prevents corridors from getting special floor tiles.
                // 这样可以防止走廊生成特殊地板
                if (!roomFloorPositions.Contains(position))
                {
                    continue;
                }

                // Do not put special floor too close to the room center.
                // 不要把特殊地板放得太靠近房间中心
                if (Vector2Int.Distance(position, room.center) <= specialFloorCenterClearRadius)
                {
                    continue;
                }

                // Do not put special floor on inner wall positions.
                // 不要把特殊地板放在内部墙的位置上
                if (innerWallPositions.Contains(position))
                {
                    continue;
                }

                candidates.Add(position);
            }
        }

        return candidates;
    }

    private void CreateFloorTiles()
    {
        HashSet<Vector2Int> coveredPositions = new HashSet<Vector2Int>();

        // Pass 1: try to place group prefabs and special floor tiles.
        // 第一遍：尝试生成大块地板和特殊地板
        // 走廊和无法放下大块地板的位置会留到第二遍
        List<Vector2Int> shuffledPositions = new List<Vector2Int>(floorPositions);
        for (int i = shuffledPositions.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Vector2Int temp = shuffledPositions[i];
            shuffledPositions[i] = shuffledPositions[j];
            shuffledPositions[j] = temp;
        }

        foreach (Vector2Int position in shuffledPositions)
        {
            if (coveredPositions.Contains(position))
            {
                continue;
            }

            if (!roomFloorPositions.Contains(position))
            {
                continue;
            }

            float difficulty = GetDifficultyForFloorPosition(position);

            WeightedFloorPrefab selectedFloorPrefab;

            if (specialFloorPositions.Contains(position))
            {
                selectedFloorPrefab = GetRandomSpecialFloorPrefab(difficulty);
            }
            else
            {
                selectedFloorPrefab = GetNormalFloorPrefab();
            }

            if (selectedFloorPrefab == null || selectedFloorPrefab.floorPrefab == null)
            {
                continue;
            }

            List<Vector2Int> groupPositions = GetGroupPositions(position, selectedFloorPrefab);

            // Single-tile prefabs are deferred to pass 2.
            // 单格 prefab 留到第二遍处理
            if (groupPositions.Count <= 1)
            {
                continue;
            }

            bool canPlace = true;

            // Check that every cell the group occupies is valid room floor and uncovered.
            // 检查这块大地板占的每个格子都是房间地板且没被覆盖
            for (int i = 0; i < groupPositions.Count; i++)
            {
                if (!floorPositions.Contains(groupPositions[i]) ||
                    !roomFloorPositions.Contains(groupPositions[i]) ||
                    coveredPositions.Contains(groupPositions[i]))
                {
                    canPlace = false;
                    break;
                }
            }

            // Require at least one empty cell of padding around the group's footprint
            // so groups never touch each other.
            // 要求大地板周围至少有一格空隙，避免两块大地板贴在一起
            if (canPlace)
            {
                HashSet<Vector2Int> footprint = new HashSet<Vector2Int>(groupPositions);

                for (int i = 0; i < groupPositions.Count; i++)
                {
                    for (int d = 0; d < directions.Length; d++)
                    {
                        Vector2Int neighbor = groupPositions[i] + directions[d];

                        if (footprint.Contains(neighbor))
                        {
                            continue;
                        }

                        if (coveredPositions.Contains(neighbor))
                        {
                            canPlace = false;
                            break;
                        }
                    }

                    if (!canPlace)
                    {
                        break;
                    }
                }
            }

            if (!canPlace)
            {
                continue;
            }

            for (int i = 0; i < groupPositions.Count; i++)
            {
                coveredPositions.Add(groupPositions[i]);
            }

            Vector3 worldPosition = GridToWorld(position);
            GameObject floor = Instantiate(selectedFloorPrefab.floorPrefab, worldPosition, Quaternion.identity);
            floor.name = selectedFloorPrefab.floorPrefab.name;
            floor.transform.parent = transform;
        }

        // Pass 2: fill every remaining position with a single tile.
        // 第二遍：剩下的位置用单格地板填满
        foreach (Vector2Int position in floorPositions)
        {
            if (coveredPositions.Contains(position))
            {
                continue;
            }

            float difficulty = GetDifficultyForFloorPosition(position);

            WeightedFloorPrefab selectedFloorPrefab;

            if (specialFloorPositions.Contains(position))
            {
                selectedFloorPrefab = GetRandomSpecialFloorPrefab(difficulty);
            }
            else
            {
                selectedFloorPrefab = GetNormalFloorPrefab();
            }

            if (selectedFloorPrefab != null &&
                selectedFloorPrefab.groupOffsets != null &&
                selectedFloorPrefab.groupOffsets.Count > 0)
            {
                selectedFloorPrefab = GetSingleFloorPrefab();
            }

            if (selectedFloorPrefab == null || selectedFloorPrefab.floorPrefab == null)
            {
                Debug.Log("No floor prefab assigned.");
                continue;
            }

            coveredPositions.Add(position);

            Vector3 worldPosition = GridToWorld(position);
            GameObject floor = Instantiate(selectedFloorPrefab.floorPrefab, worldPosition, Quaternion.identity);
            floor.name = selectedFloorPrefab.floorPrefab.name;
            floor.transform.parent = transform;
        }
    }
    private WeightedFloorPrefab GetNormalFloorPrefab()
    {
        if (floorPrefabs == null || floorPrefabs.Count == 0)
        {
            return null;
        }

        float totalWeight = 0f;

        // Prefer the floor that does not scale with difficulty.
        // 优先选择不随难度变化的地板
        // This should be Floor_Normal.
        // 这个通常应该是 Floor_Normal
        for (int i = 0; i < floorPrefabs.Count; i++)
        {
            if (floorPrefabs[i].floorPrefab != null &&
                floorPrefabs[i].weight > 0f &&
                !floorPrefabs[i].scalesWithDifficulty)
            {
                totalWeight += floorPrefabs[i].weight;
            }
        }

        // Fallback if all floors are marked as special.
        // 如果所有地板都被标记为特殊地板，就使用第一个作为备用
        if(totalWeight <= 0f)
        {
            return floorPrefabs[0];
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for(int i = 0; i < floorPrefabs.Count; i++)
        {
            if (floorPrefabs[i].floorPrefab == null ||
                floorPrefabs[i].weight <= 0f ||
                floorPrefabs[i].scalesWithDifficulty)
            {
                continue;
            }

            currentWeight += floorPrefabs[i].weight;

            if (randomValue <= currentWeight)
            {
                return floorPrefabs[i];
            }
        }

        return floorPrefabs[0];
        
    }

    private WeightedFloorPrefab GetRandomSpecialFloorPrefab(float difficulty)
    {
        if (floorPrefabs == null || floorPrefabs.Count == 0)
        {
            return GetNormalFloorPrefab();
        }

        float totalWeight = 0f;

        // Only use floors marked as scaling with difficulty.
        // 只使用被标记为随难度变化的地板
        for (int i = 0; i < floorPrefabs.Count; i++)
        {
            if (floorPrefabs[i].floorPrefab != null &&
                floorPrefabs[i].weight > 0f &&
                floorPrefabs[i].scalesWithDifficulty)
            {
                totalWeight += GetEffectiveFloorWeight(floorPrefabs[i], difficulty);
            }
        }

        // If no special floor exists, use normal floor.
        // 如果没有可用的特殊地板，就使用普通地板
        if (totalWeight <= 0f)
        {
            return GetNormalFloorPrefab();
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < floorPrefabs.Count; i++)
        {
            if (floorPrefabs[i].floorPrefab == null ||
                floorPrefabs[i].weight <= 0f ||
                !floorPrefabs[i].scalesWithDifficulty)
            {
                continue;
            }

            currentWeight += GetEffectiveFloorWeight(floorPrefabs[i], difficulty);

            if (randomValue <= currentWeight)
            {
                return floorPrefabs[i];
            }
        }

        return GetNormalFloorPrefab();
    }

    private float GetEffectiveFloorWeight(WeightedFloorPrefab floorData, float difficulty)
    {
        if (floorData.floorPrefab == null || floorData.weight <= 0f)
        {
            return 0f;
        }

        // Special floors should not appear when difficulty is 0.
        // 难度为 0 时不应该出现特殊地板
        if (floorData.scalesWithDifficulty)
        {
            return floorData.weight * Mathf.Lerp(0f, specialWeightAtMaxDifficulty, difficulty);
        }

        // Normal floors become less dominant at high difficulty.
        // 难度越高，普通地板占比越低
        return floorData.weight * Mathf.Lerp(1f, normalWeightAtMaxDifficulty, difficulty);
    }

    private void CreateWallTiles()
    {
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();

        // Check every floor tile and find empty neighboring positions.
        // 检查每个地板格子，并找出旁边的空格
        foreach (Vector2Int floorPosition in floorPositions)
        {
            float floorDifficulty = GetDifficultyForFloorPosition(floorPosition);

            for (int i = 0; i < directions.Length; i++)
            {
                Vector2Int neighborPosition = floorPosition + directions[i];

                // If a neighbor is not floor, it becomes a wall position.
                // 如果相邻位置不是地板，就把它当作墙的位置
                if (!floorPositions.Contains(neighborPosition))
                {
                    wallPositions.Add(neighborPosition);

                    // Wall difficulty is based on the floor next to it.
                    // 墙体难度根据旁边地板的难度决定
                    StoreWallDifficulty(neighborPosition, floorDifficulty);
                }
            }
        }

        foreach (Vector2Int wallPosition in wallPositions)
        {
            float difficulty = GetDifficultyForWallPosition(wallPosition);

            // Select a wall prefab using weighted random selection and difficulty.
            // 根据权重和难度随机选择墙体 prefab
            GameObject selectedWallPrefab = GetRandomWallPrefab(difficulty);

            if (selectedWallPrefab == null)
            {
                Debug.Log("No wall prefab assigned.");
                return;
            }

            Vector3 worldPosition = GridToWorld(wallPosition);

            // The wall prefab height is 1.5, so center Y is 0.75.
            // 墙体 prefab 高度是 1.5，所以中心 Y 值是 0.75
            worldPosition.y = 0.75f;

            // Create the wall tile in the scene.
            // 在场景中生成墙体格子
            GameObject wall = Instantiate(selectedWallPrefab, worldPosition, Quaternion.identity);
            wall.name = selectedWallPrefab.name;
            wall.transform.parent = transform;
        }
    }

    private void CreateInnerWallTiles()
    {
        // Create walls inside rooms.
        // 在房间内部创建墙体障碍
        foreach (Vector2Int innerWallPosition in innerWallPositions)
        {
            float difficulty = GetDifficultyForFloorPosition(innerWallPosition);

            GameObject selectedWallPrefab = GetRandomWallPrefab(difficulty);

            if (selectedWallPrefab == null)
            {
                Debug.Log("No wall prefab assigned.");
                return;
            }

            Vector3 worldPosition = GridToWorld(innerWallPosition);
            worldPosition.y = 0.75f;

            GameObject wall = Instantiate(selectedWallPrefab, worldPosition, Quaternion.identity);
            wall.name = selectedWallPrefab.name;
            wall.transform.parent = transform;
        }
    }

    private GameObject GetRandomWallPrefab(float difficulty)
    {
        if (wallPrefabs == null || wallPrefabs.Count == 0)
        {
            return null;
        }

        float totalWeight = 0f;

        // Calculate total effective weight.
        // 计算总有效权重
        for (int i = 0; i < wallPrefabs.Count; i++)
        {
            totalWeight += GetEffectiveWallWeight(wallPrefabs[i], difficulty);
        }

        if (totalWeight <= 0f)
        {
            return null;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        // Return the prefab whose weight range contains the random value.
        // 返回随机值落入其权重范围的 prefab
        for (int i = 0; i < wallPrefabs.Count; i++)
        {
            currentWeight += GetEffectiveWallWeight(wallPrefabs[i], difficulty);

            if (randomValue <= currentWeight)
            {
                return wallPrefabs[i].wallPrefab;
            }
        }

        return wallPrefabs[0].wallPrefab;
    }

    private float GetEffectiveWallWeight(WeightedWallPrefab wallData, float difficulty)
    {
        if (wallData.wallPrefab == null || wallData.weight <= 0f)
        {
            return 0f;
        }

        // Bounce walls should not appear when difficulty is 0.
        // 难度为 0 时不应该出现反弹墙
        if (wallData.scalesWithDifficulty)
        {
            return wallData.weight * Mathf.Lerp(0f, specialWeightAtMaxDifficulty, difficulty);
        }

        // Normal walls become less dominant at high difficulty.
        // 难度越高，普通墙占比越低
        return wallData.weight * Mathf.Lerp(1f, normalWeightAtMaxDifficulty, difficulty);
    }

    private void CreateEnemySpawners()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            RoomData room = rooms[i];

            // Create an empty GameObject for the spawner.
            // 创建一个空物体作为 spawner
            GameObject spawnerObject = new GameObject("EnemySpawner_Room_" + i);
            spawnerObject.transform.parent = transform;
            spawnerObject.transform.position = GridToWorld(room.center);

            DungeonEnemySpawner spawner = spawnerObject.AddComponent<DungeonEnemySpawner>();


            float roomWorldWidth = (room.width + 1) * tileSize;
            float roomWorldHeight = (room.height + 1) * tileSize;

            float difficulty = GetRoomDifficulty(i);

            int roomMinEnemies = minEnemiesPerRoom;
            int roomMaxEnemies = maxEnemiesPerRoom + Mathf.RoundToInt(extraEnemiesAtMaxDifficulty * difficulty);

            // The first room can be safe and spawn no enemies.
            // 第一个房间可以是安全房，不生成敌人
            if (firstRoomIsSafe && i == 0)
            {
                roomMinEnemies = 0;
                roomMaxEnemies = 0;
            }

            // Shop room can be safe.
            // 商店房可以不生成敌人
            if (shopRoomIsSafe && i == shopRoomIndex)
            {
                roomMinEnemies = 0;
                roomMaxEnemies = 0;
            }

            // Exit room can be safe.
            // 出口房可以不生成敌人
            if (exitRoomIsSafe && i == exitRoomIndex)
            {
                roomMinEnemies = 0;
                roomMaxEnemies = 0;
            }

            // Create valid enemy spawn positions inside this room.
            // 创建这个房间内可用的敌人生成点
            List<Vector3> spawnPositions = GetRoomEnemySpawnPositions(room);

            // Pass room and enemy settings into the spawner.
            // 把房间和敌人设置传给 spawner
            spawner.SetupSpawner(
                enemyPrefabs,
                roomMinEnemies,
                roomMaxEnemies,
                roomWorldWidth,
                roomWorldHeight,
                spawnPositions
            );

            enemySpawners.Add(spawner);
        }
    }

    private List<Vector3> GetRoomEnemySpawnPositions(RoomData room)
    {
        List<Vector3> spawnPositions = new List<Vector3>();
        List<Vector2Int> selectedGridPositions = new List<Vector2Int>();

        int left = room.center.x - room.width / 2 + 1;
        int right = room.center.x + room.width / 2 - 1;
        int bottom = room.center.y - room.height / 2 + 1;
        int top = room.center.y + room.height / 2 - 1;

        for (int x = left; x <= right; x++)
        {
            for (int y = bottom; y <= top; y++)
            {
                Vector2Int gridPosition = new Vector2Int(x, y);

                // Do not spawn enemies inside inner walls.
                // 不要把敌人生成在内部墙里
                if (innerWallPositions.Contains(gridPosition))
                {
                    continue;
                }

                // Keep the center a little clearer.
                // 让房间中心稍微保持空一点
                if (Vector2Int.Distance(gridPosition, room.center) <= 1)
                {
                    continue;
                }

                // Keep enemy spawn positions separated.
                // 让敌人生成点之间保持距离
                if (!IsEnemySpawnPositionFarEnough(gridPosition, selectedGridPositions))
                {
                    continue;
                }

                selectedGridPositions.Add(gridPosition);

                Vector3 worldPosition = GridToWorld(gridPosition);
                worldPosition.y = 1f;

                spawnPositions.Add(worldPosition);
            }
        }

        return spawnPositions;
    }

    private bool IsEnemySpawnPositionFarEnough(Vector2Int newPosition, List<Vector2Int> selectedPositions)
    {
        for (int i = 0; i < selectedPositions.Count; i++)
        {
            if (Vector2Int.Distance(newPosition, selectedPositions[i]) < enemySpawnSpacing)
            {
                return false;
            }
        }
        return true;
    }

    private void CreateRoomControllers()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            RoomData room = rooms[i];

            // Create an empty GameObject to act as this room's trigger area.
            // 创建一个空物体作为这个房间的触发区域
            GameObject roomObject = new GameObject("RoomController_" + i);
            roomObject.transform.parent = transform;
            roomObject.transform.position = GridToWorld(room.center);

            // Add a trigger collider that covers the room area.
            // 添加一个覆盖房间区域的 trigger collider
            BoxCollider roomTrigger = roomObject.AddComponent<BoxCollider>();
            roomTrigger.isTrigger = true;

            float roomWorldWidth = (room.width + 1) * tileSize;
            float roomWorldHeight = (room.height + 1) * tileSize;

            roomTrigger.size = new Vector3(roomWorldWidth, 4f, roomWorldHeight);
            roomTrigger.center = new Vector3(0f, 2f, 0f);

            // Add the room controller script and connect it to the room's spawner.
            // 添加 room controller 脚本，并把它连接到这个房间的 spawner
            DungeonRoomController roomController = roomObject.AddComponent<DungeonRoomController>();
            roomController.SetEnemySpawner(enemySpawners[i]);

            roomControllers.Add(roomController);

            // Add the reward chest spawner to this generated room.
            // 给这个生成出来的房间添加奖励宝箱刷新器
            SetupRewardChestSpawnerForRoom(
                roomObject,
                i,
                roomWorldWidth,
                roomWorldHeight
            );

            // Register this generated room on the minimap and add a tracker.
            // 把这个生成出来的房间注册到小地图，并添加房间探索检测器
            SetupMinimapForRoom(
                roomObject,
                i,
                roomObject.transform.position,
                roomWorldWidth,
                roomWorldHeight
            );
        }
    }

    private void SetupRewardChestSpawnerForRoom(
        GameObject roomObject,
        int roomIndex,
        float roomWorldWidth,
        float roomWorldHeight
    )
    {
        if (!enableRewardChestSpawning)
        {
            return;
        }

        if (roomObject == null)
        {
            return;
        }

        RoomRewardChestSpawner chestSpawner = roomObject.GetComponent<RoomRewardChestSpawner>();

        if (chestSpawner == null)
        {
            chestSpawner = roomObject.AddComponent<RoomRewardChestSpawner>();
        }

        // Disable reward chests in the start room, shop room, and exit room.
        // 初始房、商店房、出口房都不生成奖励宝箱
        chestSpawner.isStartRoom = roomIndex == 0;
        chestSpawner.isShopRoom = roomIndex == shopRoomIndex;
        chestSpawner.isExitRoom = roomIndex == exitRoomIndex;

        chestSpawner.enemyTag = enemyTagForRewardChest;
        chestSpawner.playerTag = "Player";

        chestSpawner.requirePlayerEnteredRoom = true;
        chestSpawner.requireHadEnemies = true;
        chestSpawner.checkInterval = 0.4f;

        chestSpawner.rewardChestPrefab = rewardChestPrefab;
        chestSpawner.chestSpawnOffset = rewardChestSpawnOffset;
        chestSpawner.roomCenterOffset = Vector3.zero;

        // The spawner uses this area to count enemies inside the current room.
        // 刷新器会用这个范围检测当前房间内是否还有敌人
        chestSpawner.roomHalfExtents = new Vector3(
            roomWorldWidth * 0.5f,
            2f,
            roomWorldHeight * 0.5f
        );
    }

    private void SetupMinimapManager()
    {
        if (!enableMinimap)
        {
            return;
        }

        if (minimapManager == null)
        {
            minimapManager = FindAnyObjectByType<DungeonMinimapManager>();
        }
    }

    private void RegisterCorridorOnMinimap(
        Vector2Int startGridPosition,
        Vector2Int cornerGridPosition,
        Vector2Int endGridPosition
    )
    {
        if (!enableMinimap)
        {
            return;
        }

        SetupMinimapManager();

        if (minimapManager == null)
        {
            return;
        }

        Vector3 startWorldPosition = GridToWorld(startGridPosition);
        Vector3 cornerWorldPosition = GridToWorld(cornerGridPosition);
        Vector3 endWorldPosition = GridToWorld(endGridPosition);

        minimapManager.RegisterCorridorLShape(
            startWorldPosition,
            cornerWorldPosition,
            endWorldPosition
        );
    }

    private void SetupMinimapForRoom(
        GameObject roomObject,
        int roomIndex,
        Vector3 roomWorldCenter,
        float roomWorldWidth,
        float roomWorldHeight
    )
    {
        if (!enableMinimap)
        {
            return;
        }

        if (roomObject == null)
        {
            return;
        }

        SetupMinimapManager();

        if (minimapManager == null)
        {
            return;
        }

        bool isStartRoom = roomIndex == 0;
        bool isShopRoom = roomIndex == shopRoomIndex;
        bool isExitRoom = roomIndex == exitRoomIndex;

        minimapManager.RegisterRoom(
            roomIndex,
            roomWorldCenter,
            roomWorldWidth,
            roomWorldHeight,
            isStartRoom,
            isShopRoom,
            isExitRoom
        );

        DungeonMinimapRoomTracker minimapTracker =
            roomObject.GetComponent<DungeonMinimapRoomTracker>();

        if (minimapTracker == null)
        {
            minimapTracker = roomObject.AddComponent<DungeonMinimapRoomTracker>();
        }

        minimapTracker.Setup(
            roomIndex,
            minimapManager,
            playerTagForMinimap
        );
    }

    private void CreateDoors()
    {
        if (doorBlockerPrefab == null)
        {
            Debug.Log("Door blocker prefab is not assigned.");
            return;
        }

        // Create doors based on all room connections.
        // 根据所有房间连接生成门
        for (int i = 0; i < roomConnections.Count; i++)
        {
            int roomAIndex = roomConnections[i].roomAIndex;
            int roomBIndex = roomConnections[i].roomBIndex;

            RoomData roomA = rooms[roomAIndex];
            RoomData roomB = rooms[roomBIndex];

            // Create one door near each connected room.
            // 每对连接的房间会在两边各生成一个门
            DungeonDoorController roomADoor = CreateDoorBetweenRooms(roomA, roomB);
            DungeonDoorController roomBDoor = CreateDoorBetweenRooms(roomB, roomA);

            // Connect doors to their room controllers.
            // 把门连接到对应的 room controller
            roomControllers[roomAIndex].AddDoor(roomADoor);
            roomControllers[roomBIndex].AddDoor(roomBDoor);
        }
    }

    private Vector2Int GetDoorPositionToward(RoomData room, Vector2Int targetCenter)
    {
        // Find the rough direction from this room to the target room.
        // 找到当前房间到目标房间的大致方向
        Vector2Int direction = targetCenter - room.center;

        // If the target is mostly left or right, place the door on the left/right side.
        // 如果目标主要在左边或右边，就把门放在左右侧
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
            {
                return new Vector2Int(room.center.x + room.width / 2 + 1, room.center.y);
            }

            return new Vector2Int(room.center.x - room.width / 2 - 1, room.center.y);
        }

        // Otherwise, place the door on the top/bottom side.
        // 否则就把门放在上方或下方
        if (direction.y > 0)
        {
            return new Vector2Int(room.center.x, room.center.y + room.height / 2 + 1);
        }

        return new Vector2Int(room.center.x, room.center.y - room.height / 2 - 1);
    }

    private DungeonDoorController CreateDoorBetweenRooms(RoomData room, RoomData targetRoom)
    {
        // Find the direction from this room to the connected room.
        // 找到当前房间到目标房间的大致方向
        Vector2Int direction = targetRoom.center - room.center;

        Vector3 worldPosition = Vector3.zero;

        float doorWidth = tileSize * 2f;
        float doorThickness = 1f;
        float doorHeight = 2f;

        // If the target room is mostly left or right, this is a horizontal corridor.
        // 如果目标房间主要在左边或右边，说明连接的是横向走廊
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            int doorGridX;

            if (direction.x > 0)
            {
                doorGridX = room.center.x + room.width / 2 + 1;
            }
            else
            {
                doorGridX = room.center.x - room.width / 2 - 1;
            }

            // Horizontal corridors use y and y + 1, so the door should be centered at y + 0.5.
            // 横向走廊占 y 和 y + 1 两格，所以门要放在 y + 0.5 的中间位置
            float doorGridY = room.center.y + 0.5f;

            worldPosition = new Vector3(
                doorGridX * tileSize,
                doorHeight / 2f,
                doorGridY * tileSize
            );
        }
        else
        {
            int doorGridY;

            if (direction.y > 0)
            {
                doorGridY = room.center.y + room.height / 2 + 1;
            }
            else
            {
                doorGridY = room.center.y - room.height / 2 - 1;
            }

            // Vertical corridors use x and x + 1, so the door should be centered at x + 0.5.
            // 纵向走廊占 x 和 x + 1 两格，所以门要放在 x + 0.5 的中间位置
            float doorGridX = room.center.x + 0.5f;

            worldPosition = new Vector3(
                doorGridX * tileSize,
                doorHeight / 2f,
                doorGridY * tileSize
            );
        }

        GameObject doorObject = Instantiate(doorBlockerPrefab, worldPosition, Quaternion.identity);
        doorObject.name = doorBlockerPrefab.name;
        doorObject.transform.parent = transform;

        // If this is a horizontal corridor, the door should cover the Z direction.
        // 如果是横向走廊，门需要挡住 Z 方向的两格宽度
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            doorObject.transform.localScale = new Vector3(doorThickness, doorHeight, doorWidth);
        }
        else
        {
            doorObject.transform.localScale = new Vector3(doorWidth, doorHeight, doorThickness);
        }

        DungeonDoorController doorController = doorObject.GetComponent<DungeonDoorController>();

        // Doors start open and hidden.
        // 门一开始是打开且隐藏的
        if (doorController != null)
        {
            doorController.OpenDoor();
        }

        return doorController;
    }

    private void CreateShopRoomObject()
    {
        if (shopRoomPrefab == null)
        {
            return;
        }

        if (shopRoomIndex < 0 || shopRoomIndex >= rooms.Count)
        {
            return;
        }

        RoomData shopRoom = rooms[shopRoomIndex];

        Vector3 shopPosition = GridToWorld(shopRoom.center);
        shopPosition.y = 0.5f;

        GameObject shopObject = Instantiate(shopRoomPrefab, shopPosition, Quaternion.identity);
        shopObject.name = shopRoomPrefab.name;
        shopObject.transform.parent = transform;
    }

    private void CreateExitPortal()
    {
        if (exitPortalPrefab == null || rooms.Count == 0)
        {
            return;
        }

        if (exitRoomIndex < 0 || exitRoomIndex >= rooms.Count)
        {
            exitRoomIndex = rooms.Count - 1;
        }

        // Put the exit portal in the selected exit room.
        // 把出口传送门放在选中的出口房间里
        RoomData exitRoom = rooms[exitRoomIndex];

        Vector3 portalPosition = GridToWorld(exitRoom.center);
        portalPosition.y = 0.3f;

        GameObject portal = Instantiate(exitPortalPrefab, portalPosition, Quaternion.identity);
        portal.name = exitPortalPrefab.name;
        portal.transform.parent = transform;
    }

    private int GetCurrentLevelNumber()
    {
        // If each level is a separate scene, use Build Settings scene index.
        // 如果每一关是单独 scene，就使用 Build Settings 里的 scene index
        // Build index 0 becomes Level 1.
        // Build index 0 会对应 Level 1
        if (useSceneIndexAsLevelNumber)
        {
            return SceneManager.GetActiveScene().buildIndex + 1;
        }

        // If you generate multiple levels inside the same scene,
        // 如果你在同一个 scene 里生成多层关卡，
        // manually update currentLevelNumber instead.
        // 就手动更新 currentLevelNumber
        return currentLevelNumber;
    }

    private float GetLevelDifficulty()
    {
        int levelNumber = GetCurrentLevelNumber();

        // Before firstSpecialEffectLevel, there are no special effects.
        // 在 firstSpecialEffectLevel 之前，不会有特殊效果
        if (levelNumber < firstSpecialEffectLevel)
        {
            return 0f;
        }

        // If the max level is set incorrectly, just treat this as max difficulty.
        // 如果最大难度关卡设置不合理，就直接当作最高难度
        if (maxDifficultyLevel <= firstSpecialEffectLevel)
        {
            return 1f;
        }

        // firstSpecialEffectLevel = 2
        // maxDifficultyLevel = 5
   
        // Level 1 -> 0
        // Level 2 -> 0.25
        // Level 3 -> 0.5
        // Level 4 -> 0.75
        // Level 5 -> 1
        // Level 6+ -> 1
        float difficulty =
            (levelNumber - firstSpecialEffectLevel + 1f) /
            (maxDifficultyLevel - firstSpecialEffectLevel + 1f);

        return Mathf.Clamp01(difficulty);
    }

    private float GetRoomDifficulty(int roomIndex)
    {
        // Overall level difficulty is based on level number.
        // 整体关卡难度根据关卡数决定
        // Level 1 can be completely clean.
        // Level 1 可以完全没有特殊效果
        float levelDifficulty = GetLevelDifficulty();

        // If the level difficulty is 0, the whole level has no special effects.
        // 如果关卡难度是 0，整关都不会有特殊效果
        if (levelDifficulty <= 0f)
        {
            return 0f;
        }

        // Room progress makes later rooms in the same level slightly harder.
        // 同一关里越后面的房间会稍微更难
        float roomProgress = 0f;

        if (rooms.Count > 1)
        {
            roomProgress = roomIndex / (float)(rooms.Count - 1);
        }

        // Early rooms in a level are easier, later rooms are closer to the level difficulty.
        // 同一关前面的房间更简单，后面的房间更接近本关难度
        float roomMultiplier = Mathf.Lerp(0.5f, 1f, roomProgress);

        return Mathf.Clamp01(levelDifficulty * roomMultiplier);
    }

    private float GetDifficultyForFloorPosition(Vector2Int position)
    {
        if (floorDifficultyByPosition.ContainsKey(position))
        {
            return floorDifficultyByPosition[position];
        }

        return 0f;
    }

    private void StoreWallDifficulty(Vector2Int position, float difficulty)
    {
        if (!wallDifficultyByPosition.ContainsKey(position))
        {
            wallDifficultyByPosition.Add(position, difficulty);
        }
        else if (difficulty > wallDifficultyByPosition[position])
        {
            wallDifficultyByPosition[position] = difficulty;
        }
    }

    private float GetDifficultyForWallPosition(Vector2Int position)
    {
        if (wallDifficultyByPosition.ContainsKey(position))
        {
            return wallDifficultyByPosition[position];
        }

        return 0f;
    }

    private Vector3 GridToWorld(Vector2Int gridPosition)
    {
        // Convert 2D grid coordinates into Unity world coordinates.
        // 把 2D 网格坐标转换成 Unity 世界坐标
        // Grid x maps to world X, grid y maps to world Z.
        // 网格 x 对应世界 X，网格 y 对应世界 Z
        return new Vector3(gridPosition.x * tileSize, 0f, gridPosition.y * tileSize);
    }

    //This method identify the postions that the group floor will take place
    //这个method会寻找记录生成大地板所需的位置
    private List<Vector2Int> GetGroupPositions(Vector2Int origin, WeightedFloorPrefab floorData)
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        // Always include the origin so the iterated tile is guaranteed to get a floor.
        // 始终包含原点，确保当前迭代的格子一定会有地板
        positions.Add(origin);

        if (floorData.groupOffsets == null || floorData.groupOffsets.Count == 0)
        {
            return positions;
        }

        for (int i = 0; i < floorData.groupOffsets.Count; i++)
        {
            Vector2Int groupPosition = origin + floorData.groupOffsets[i].offset;

            // Skip the origin if the user also listed it as an offset, to avoid duplicates.
            // 如果用户也在 offset 里把原点列进去，跳过以避免重复
            if (groupPosition == origin)
            {
                continue;
            }

            positions.Add(groupPosition);
        }

        return positions;
    }

    //Force to select single floor prefab to avoid selecting group floor prefab when close to edge of the room
    //强制选择单个地板而不是大块地板防止溢出
    private WeightedFloorPrefab GetSingleFloorPrefab()
    {
        if (floorPrefabs == null || floorPrefabs.Count == 0)
        {
            return null;
        }

        float totalWeight = 0f;

        for (int i = 0; i < floorPrefabs.Count; i++)
        {
            if (floorPrefabs[i].floorPrefab != null &&
                floorPrefabs[i].weight > 0f &&
                !floorPrefabs[i].scalesWithDifficulty &&
                (floorPrefabs[i].groupOffsets == null || floorPrefabs[i].groupOffsets.Count == 0))
            {
                totalWeight += floorPrefabs[i].weight;
            }
        }

        if (totalWeight <= 0f)
        {
            return floorPrefabs[0];
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < floorPrefabs.Count; i++)
        {
            if (floorPrefabs[i].floorPrefab == null ||
                floorPrefabs[i].weight <= 0f ||
                floorPrefabs[i].scalesWithDifficulty ||
                (floorPrefabs[i].groupOffsets != null && floorPrefabs[i].groupOffsets.Count > 0))
            {
                continue;
            }

            currentWeight += floorPrefabs[i].weight;

            if (randomValue <= currentWeight)
            {
                return floorPrefabs[i];
            }
        }

        return floorPrefabs[0];
    }

    //this method is for regenerating the dungeon in game for any purpose
    //重新生成地牢
    public void RegenerateDungeon()
    {
        GenerateDungeon();
    }
}
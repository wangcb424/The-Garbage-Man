using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonRunManager : MonoBehaviour
{
    public static DungeonRunManager Instance;

    // How many small levels each scene contains.
    // 每个大关卡 scene 里面有几个小关卡
    public int smallLevelsPerScene = 5;

    // How many main scene themes are in the run.
    // 一共有几个大关卡主题 scene
    public int totalThemeScenes = 3;

    // The build index of the first dungeon scene.
    // 第一个地牢 scene 在 Build Settings 里的 index
    public int firstDungeonSceneBuildIndex = 2;

    // Current global small level number.
    // 当前全局小关卡数，难度根据这个提升
    public int globalLevelNumber = 1;

    // Current small level number inside this scene.
    // 当前 scene 内的小关卡数
    public int smallLevelInCurrentScene = 1;

    // Player object that should be kept between scenes.
    // 需要在不同 scene 之间保留的玩家
    public Transform playerTransform;

    // Where the player appears after a new small level starts.
    // 每个小关卡开始时玩家出现的位置
    public Vector3 playerStartPosition = new Vector3(0f, 1.2f, 0f);

    private void Awake()
    {
        // If another manager already exists, destroy this one.
        // 如果已经有 manager 了，就删除新的，避免重复
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Store this manager as the main instance.
        // 把这个 manager 存成主实例
        Instance = this;

        // Keep this manager when scenes change.
        // 切换 scene 时不要删除这个 manager
        DontDestroyOnLoad(gameObject);

        // Try to find player if it was not assigned in the Inspector.
        // 如果 Inspector 没有 assign player，就自动找 Player
        FindPlayerIfNeeded();

        // PlayerScenePositioner keeps the Hub player between scenes.
        // DungeonRunManager should not create or own a second persistent player.

        // Listen for scene load events.
        // 监听 scene 加载完成事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        GenerateCurrentSmallLevel();
    }

    private void OnDestroy()
    {
        // Stop listening when this object is destroyed.
        // 物体被删除时取消监听
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void GoToNextSmallLevel()
    {
        // Increase global level.
        // 全局小关卡数增加，难度不会下降
        globalLevelNumber++;

        // Increase small level count inside this scene.
        // 当前 scene 内的小关卡数增加
        smallLevelInCurrentScene++;

        // If this scene already finished all small levels, go to the next scene.
        // 如果当前 scene 已经完成所有小关卡，就进入下一个大关卡 scene
        if (smallLevelInCurrentScene > smallLevelsPerScene)
        {
            smallLevelInCurrentScene = 1;
            LoadNextThemeScene();
            return;
        }

        // Otherwise, regenerate dungeon in the same scene.
        // 否则在同一个 scene 里重新生成地牢
        GenerateCurrentSmallLevel();
    }

    private void LoadNextThemeScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Calculate the next scene index inside the theme scene range.
        // 计算下一个主题 scene 的 build index
        int relativeSceneIndex = currentSceneIndex - firstDungeonSceneBuildIndex;
        int nextRelativeSceneIndex = relativeSceneIndex + 1;

        // Loop back to the first theme scene after the last theme scene.
        // 如果超过最后一个主题 scene，就回到第一个主题 scene
        if (nextRelativeSceneIndex >= totalThemeScenes)
        {
            nextRelativeSceneIndex = 0;
        }

        int nextSceneIndex = firstDungeonSceneBuildIndex + nextRelativeSceneIndex;

        SceneManager.LoadScene(nextSceneIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // After loading a new theme scene, find player again if needed.
        // 新 scene 加载后，如果 player 引用丢了，就重新寻找
        FindPlayerIfNeeded();

        // After loading a new theme scene, generate its first small level.
        // 新主题 scene 加载完成后，生成这个 scene 的第一个小关卡
        GenerateCurrentSmallLevel();
    }

    private void GenerateCurrentSmallLevel()
    {
        DungeonGenerator dungeonGenerator = FindAnyObjectByType<DungeonGenerator>();

        if (dungeonGenerator == null)
        {
            Debug.Log("DungeonGenerator was not found in this scene.");
            return;
        }

        // Find player again before resetting position.
        // 重置位置前再次确认 player 引用
        FindPlayerIfNeeded();

        // Use global level number instead of scene index.
        // 使用全局小关卡数控制难度，不使用 scene index
        dungeonGenerator.useSceneIndexAsLevelNumber = false;
        dungeonGenerator.currentLevelNumber = globalLevelNumber;

        Debug.Log(
            "Generating global level " + globalLevelNumber +
            ", scene small level " + smallLevelInCurrentScene
        );

        // Generate new dungeon.
        // 生成新的地牢
        dungeonGenerator.GenerateDungeon();

        // Move player back to the starting room position.
        // 把玩家放回小关卡起点
        ResetPlayerPosition();
    }

    private void FindPlayerIfNeeded()
    {
        // If player is already assigned, do nothing.
        // 如果 player 已经 assign，就不用再找
        if (playerTransform != null)
        {
            return;
        }

        // Find object with Player tag.
        // 找到 Tag 是 Player 的物体
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }

    private void ResetPlayerPosition()
    {
        FindPlayerIfNeeded();

        if (playerTransform == null)
        {
            Debug.Log("Player Transform is not assigned.");
            return;
        }

        // CharacterController can block direct position reset.
        // CharacterController 可能会影响直接改位置，所以先暂时关闭
        CharacterController characterController = playerTransform.GetComponent<CharacterController>();

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        // Reset player position.
        // 重置玩家位置
        playerTransform.position = playerStartPosition;

        // Reenable CharacterController after moving.
        // 移动完成后重新打开 CharacterController
        if (characterController != null)
        {
            characterController.enabled = true;
        }

        Debug.Log("Reset player to " + playerStartPosition);
    }
}
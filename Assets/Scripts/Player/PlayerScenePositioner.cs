using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerScenePositioner : MonoBehaviour
{
    public static PlayerScenePositioner Instance { get; private set; }

    [Header("Scene Names")]
    public string introSceneName = "IntroScene";
    public string hubSceneName = "HubScene";
    public string buffChoiceSceneName = "BuffChoiceScene";

    [Header("Spawn")]
    public bool moveToSpawnPointOnSceneLoaded = true;

    private void Awake()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        // IntroScene should never keep a player.
        if (currentSceneName == introSceneName)
        {
            Destroy(gameObject);
            return;
        }

        // Only the first Player is kept. HubScene has the original Player.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (moveToSpawnPointOnSceneLoaded)
        {
            MoveToSpawnPoint();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == introSceneName)
        {
            Destroy(gameObject);
            return;
        }

        if (moveToSpawnPointOnSceneLoaded)
        {
            MoveToSpawnPoint();
        }
    }

    private void MoveToSpawnPoint()
    {
        SceneSpawnPoint spawnPoint = FindAnyObjectByType<SceneSpawnPoint>();

        if (spawnPoint == null)
        {
            Debug.Log("SceneSpawnPoint was not found in this scene.");
            return;
        }

        CharacterController characterController = GetComponent<CharacterController>();

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        transform.position = spawnPoint.transform.position;
        transform.rotation = spawnPoint.transform.rotation;

        if (characterController != null)
        {
            characterController.enabled = true;
        }

        Debug.Log("Persistent player moved to spawn point in scene: " + SceneManager.GetActiveScene().name);
    }
}

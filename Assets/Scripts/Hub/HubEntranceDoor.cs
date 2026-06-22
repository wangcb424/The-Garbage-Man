using UnityEngine;
using UnityEngine.SceneManagement;

public class HubEntranceDoor : MonoBehaviour
{
    [Header("Player Detection")]

    // Player tag.
    // 玩家 Tag
    public string playerTag = "Player";

    [Header("Enter Settings")]

    // If true, player enters dungeon automatically.
    // 如果为 true，碰到门自动进入地牢
    public bool enterAutomatically = false;

    // Key used to enter dungeon.
    // 进入地牢按键
    public KeyCode interactKey = KeyCode.E;

    private bool playerInside = false;
    private bool isLoading = false;

    private void Update()
    {
        if (isLoading)
        {
            return;
        }

        if (!playerInside)
        {
            return;
        }

        if (enterAutomatically)
        {
            EnterDungeon();
            return;
        }

        if (Input.GetKeyDown(interactKey))
        {
            EnterDungeon();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        playerInside = true;

        if (enterAutomatically)
        {
            EnterDungeon();
        }
        else
        {
            Debug.Log("Press E to enter dungeon.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        playerInside = false;
    }

    private void EnterDungeon()
    {
        if (isLoading)
        {
            return;
        }

        isLoading = true;

        if (GlobalRunManager.Instance != null)
        {
            GlobalRunManager.Instance.StartOrContinueRunFromHub();
        }
        else
        {
            Debug.Log("GlobalRunManager was not found. Loading first dungeon scene directly.");
            SceneManager.LoadScene(2);
        }
    }
}
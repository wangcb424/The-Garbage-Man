using UnityEngine;

public class ExitPortal : MonoBehaviour
{
    // Player tag.
    // 玩家 Tag
    public string playerTag = "Player";

    // If true, player enters portal automatically.
    // 如果为 true，碰到传送门自动进入 BuffChoiceScene
    public bool enterAutomatically = true;

    // Key used when automatic enter is false.
    // 如果不是自动进入，需要按这个键
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

        if (enterAutomatically || Input.GetKeyDown(interactKey))
        {
            GoToBuffChoiceScene();
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
            GoToBuffChoiceScene();
        }
        else
        {
            Debug.Log("Press E to choose a buff.");
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

    private void GoToBuffChoiceScene()
    {
        if (isLoading)
        {
            return;
        }

        isLoading = true;

        if (GlobalRunManager.Instance != null)
        {
            GlobalRunManager.Instance.GoToBuffChoiceScene();
        }
        else
        {
            Debug.Log("GlobalRunManager was not found.");
        }
    }
}
using UnityEngine;

public class DungeonDoorController : MonoBehaviour
{
    // 门的组件
    // The collider blocks the player when the door is closed.
    private Collider doorCollider;

    // The renderer controls whether the door is visible.
    private Renderer doorRenderer;

    private void Awake()
    {
        // Get the components attached to this door object.
        doorCollider = GetComponent<Collider>();
        doorRenderer = GetComponent<Renderer>();
    }

    //开门系统
    public void OpenDoor()
    {
        // 关掉collision和visable，让player能过去
        // When the door is open, disable collision so the player can pass.
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        // Hide the door visually when it is open.
        if (doorRenderer != null)
        {
            doorRenderer.enabled = false;
        }
    }

    // 关门系统
    public void CloseDoor()
    {
        // 与开门相反
        // When the door is closed, enable collision to block the player.
        if (doorCollider != null)
        {
            doorCollider.enabled = true;
        }

        // Show the door visually when it is closed.
        if (doorRenderer != null)
        {
            doorRenderer.enabled = true;
        }
    }
}
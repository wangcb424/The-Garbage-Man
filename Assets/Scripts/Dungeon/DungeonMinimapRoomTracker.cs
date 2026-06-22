using UnityEngine;

public class DungeonMinimapRoomTracker : MonoBehaviour
{
    public int roomIndex = -1;
    public string playerTag = "Player";
    public DungeonMinimapManager minimapManager;

    public void Setup(int newRoomIndex, DungeonMinimapManager newMinimapManager, string newPlayerTag)
    {
        roomIndex = newRoomIndex;
        minimapManager = newMinimapManager;
        playerTag = newPlayerTag;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        if (minimapManager == null)
        {
            minimapManager = FindAnyObjectByType<DungeonMinimapManager>();
        }

        if (minimapManager != null)
        {
            minimapManager.MarkRoomExplored(roomIndex);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        if (minimapManager != null)
        {
            minimapManager.ClearCurrentRoom(roomIndex);
        }
    }

    private bool IsPlayer(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        if (other.CompareTag(playerTag))
        {
            return true;
        }

        Transform parent = other.transform.parent;

        while (parent != null)
        {
            if (parent.CompareTag(playerTag))
            {
                return true;
            }

            parent = parent.parent;
        }

        return false;
    }
}

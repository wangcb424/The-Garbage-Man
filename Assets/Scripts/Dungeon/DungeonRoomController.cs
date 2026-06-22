using System.Collections.Generic;
using UnityEngine;

public class DungeonRoomController : MonoBehaviour
{
    // The spawner used by this room.
    public DungeonEnemySpawner enemySpawner;

    // Has this room already started its fight?
    private bool hasStartedRoom = false;

    // Has this room already been cleared?
    private bool isCleared = false;

    // Doors connected to this room.
    private List<DungeonDoorController> doors = new List<DungeonDoorController>();

    public void SetEnemySpawner(DungeonEnemySpawner newEnemySpawner)
    {
        // Connect this room to its enemy spawner.
        enemySpawner = newEnemySpawner;
    }

    public void AddDoor(DungeonDoorController door)
    {
        if (door == null)
        {
            return;
        }

        // Add this door if it is not already in the list.
        if (!doors.Contains(door))
        {
            doors.Add(door);

            // Doors should start open before the player enters the room.
            door.OpenDoor();
        }
    }

    private void Update()
    {
        // Only check enemies after the room fight has started.
        if (!hasStartedRoom || isCleared)
        {
            return;
        }

        // If there is no spawner, just clear the room.
        if (enemySpawner == null)
        {
            ClearRoom();
            return;
        }

        // When all spawned enemies are gone, open the doors.
        if (enemySpawner.GetAliveEnemyCount() <= 0)
        {
            ClearRoom();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Do not restart a room that has already started or cleared.
        if (hasStartedRoom || isCleared)
        {
            return;
        }

        // Only the player can trigger the room fight.
        if (!other.CompareTag("Player"))
        {
            return;
        }

        StartRoom();
    }

    private void StartRoom()
    {
        hasStartedRoom = true;

        // Lock the player inside the room.
        CloseDoors();

        int spawnedCount = 0;

        // Spawn enemies for this room.
        if (enemySpawner != null)
        {
            spawnedCount = enemySpawner.SpawnEnemies();
        }

        // If no enemies were spawned, clear the room right away.
        if (spawnedCount <= 0)
        {
            ClearRoom();
        }
    }

    private void CloseDoors()
    {
        // Close every door connected to this room.
        for (int i = 0; i < doors.Count; i++)
        {
            doors[i].CloseDoor();
        }
    }

    private void OpenDoors()
    {
        // Open every door connected to this room.
        for (int i = 0; i < doors.Count; i++)
        {
            doors[i].OpenDoor();
        }
    }

    private void ClearRoom()
    {
        // Mark the room as cleared and open the doors.
        isCleared = true;
        OpenDoors();
    }
}
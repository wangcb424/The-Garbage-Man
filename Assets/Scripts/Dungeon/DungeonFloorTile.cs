using UnityEngine;

public class DungeonFloorTile : MonoBehaviour
{
    // This enum defines the type of this floor tile.
    // 分类floor类型
    public enum FloorType
    {
        Normal,
        Damage,
        Slow,
        Speed
    }

    // This variable lets each floor prefab store its own floor type.
    // 让prefab存自己效果
    public FloorType floorType = FloorType.Normal;
}
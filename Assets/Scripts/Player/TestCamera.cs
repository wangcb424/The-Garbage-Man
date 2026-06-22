using UnityEngine;

public class TestCamera : MonoBehaviour
{
    // The player or object that the camera follows.
    // 摄像头要跟随的玩家或物体
    public Transform target;

    // Offset from the target position.
    // 摄像头和玩家之间的偏移量
    public Vector3 offset = new Vector3(0f, 24f, -18f);

    // How smoothly the camera follows the target.
    // 摄像头跟随的平滑程度
    public float followSmoothness = 8f;

    private void Start()
    {
        FindPlayerIfNeeded();
    }

    private void LateUpdate()
    {
        // If target is missing, try to find player again.
        // 如果 target 没有 assign，就再次尝试寻找 Player
        if (target == null)
        {
            FindPlayerIfNeeded();
        }

        FollowTarget();
    }

    private void FindPlayerIfNeeded()
    {
        // If target is already assigned, do nothing.
        // 如果已经 assign target，就不用再找
        if (target != null)
        {
            return;
        }

        // Find the GameObject with Player tag.
        // 找到 Tag 是 Player 的物体
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            target = playerObject.transform;
        }
    }

    private void FollowTarget()
    {
        if (target == null)
        {
            return;
        }

        // Calculate where the camera should be.
        // 计算摄像头应该在的位置
        Vector3 targetPosition = target.position + offset;

        // Smoothly move the camera to the target position.
        // 平滑移动摄像头到目标位置
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSmoothness * Time.deltaTime
        );
    }
}
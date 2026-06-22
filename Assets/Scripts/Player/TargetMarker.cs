using UnityEngine;

public class TargetMarker : MonoBehaviour
{
    // The target this marker follows.
    // 这个图标跟随的目标
    private Transform target;

    // Offset above the target.
    // 图标在目标上方的偏移
    public Vector3 offset = new Vector3(0f, 2.2f, 0f);

    // If true, marker faces the camera.
    // 如果为 true，图标会面向摄像机
    public bool faceCamera = true;

    public void SetTarget(Transform newTarget)
    {
        // Set the target.
        // 设置跟随目标
        target = newTarget;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Follow target position.
        // 跟随目标位置
        transform.position = target.position + offset;

        if (faceCamera && Camera.main != null)
        {
            // Face the camera.
            // 面向摄像机
            transform.rotation = Quaternion.LookRotation(
                transform.position - Camera.main.transform.position
            );
        }
    }
}
using UnityEngine;

public abstract class EnemyMovementModule : MonoBehaviour
{
    // Enemy core that owns this movement module.
    // 拥有这个移动模块的 EnemyCore
    protected EnemyCore enemyCore;

    public virtual void Initialize(EnemyCore newEnemyCore)
    {
        // Store enemy core.
        // 保存 EnemyCore
        enemyCore = newEnemyCore;
    }

    public abstract void TickMovement();

    protected Vector3 GetFlatDirectionToPlayer()
    {
        if (enemyCore == null || enemyCore.PlayerTarget == null)
        {
            return Vector3.zero;
        }

        Vector3 direction = enemyCore.PlayerTarget.position - enemyCore.transform.position;
        direction.y = 0f;

        return direction;
    }

    protected void MoveEnemy(Vector3 moveDirection, float moveSpeed)
    {
        if (enemyCore == null)
        {
            return;
        }

        moveDirection.y = 0f;

        if (moveDirection.sqrMagnitude <= 0.001f)
        {
            return;
        }

        // Move enemy on XZ plane.
        // 在 XZ 平面移动敌人
        enemyCore.transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
    }

    protected void FaceDirection(Vector3 direction)
    {
        if (enemyCore == null)
        {
            return;
        }

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        // Rotate enemy toward movement direction.
        // 让敌人朝向移动方向
        enemyCore.transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    protected bool IsPathBlocked(
        Vector3 rayStart,
        Vector3 direction,
        float distance,
        LayerMask obstacleLayerMask
    )
    {
        if (enemyCore == null)
        {
            return false;
        }

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return false;
        }

        direction.Normalize();

        // If no layer mask is selected, use default raycast layers as fallback.
        // 如果没有选择 Layer Mask，就用默认 raycast layer 作为备用
        int layerMaskValue = obstacleLayerMask.value;

        if (layerMaskValue == 0)
        {
            layerMaskValue = Physics.DefaultRaycastLayers;
        }

        RaycastHit[] hits = Physics.RaycastAll(
            rayStart,
            direction,
            distance,
            layerMaskValue
        );

        for (int i = 0; i < hits.Length; i++)
        {
            GameObject hitObject = hits[i].collider.gameObject;

            // Ignore this enemy and its child objects.
            // 忽略自己和自己的子物体
            if (hitObject.GetComponentInParent<EnemyCore>() == enemyCore)
            {
                continue;
            }

            // Ignore player.
            // 忽略玩家，避免把玩家当成墙
            if (hitObject.CompareTag("Player"))
            {
                continue;
            }

            // Ignore triggers.
            // 忽略 trigger
            if (hits[i].collider.isTrigger)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    protected Vector3 GetWallAvoidanceDirection(Vector3 wantedDirection, LayerMask obstacleLayerMask)
    {
        if (enemyCore == null)
        {
            return Vector3.zero;
        }

        wantedDirection.y = 0f;

        if (wantedDirection.sqrMagnitude <= 0.001f)
        {
            return Vector3.zero;
        }

        wantedDirection.Normalize();

        Vector3 rayStart = enemyCore.transform.position + Vector3.up * 1f;
        float checkDistance = 2f;

        // If there is no wall in front, keep the original direction.
        // 如果前方没有墙，就继续往原方向走
        if (!IsPathBlocked(rayStart, wantedDirection, checkDistance, obstacleLayerMask))
        {
            return wantedDirection;
        }

        // Try left.
        // 尝试往左绕
        Vector3 leftDirection = Quaternion.Euler(0f, -60f, 0f) * wantedDirection;

        if (!IsPathBlocked(rayStart, leftDirection, checkDistance, obstacleLayerMask))
        {
            return leftDirection.normalized;
        }

        // Try right.
        // 尝试往右绕
        Vector3 rightDirection = Quaternion.Euler(0f, 60f, 0f) * wantedDirection;

        if (!IsPathBlocked(rayStart, rightDirection, checkDistance, obstacleLayerMask))
        {
            return rightDirection.normalized;
        }

        // If both sides are blocked, move backward.
        // 如果左右都被挡住，就先往后退
        return -wantedDirection;
    }
}
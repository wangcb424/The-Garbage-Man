using UnityEngine;

public class EnemyWanderingMovement : EnemyMovementModule
{
    // Enemy movement speed.
    // 敌人移动速度
    public float moveSpeed = 3f;

    // Enemy stops chasing when close enough.
    // 敌人离玩家足够近时停止靠近
    public float stopDistance = 1.2f;

    // How far the enemy can see.
    // 敌人的视野距离
    public float viewDistance = 18f;

    // Enemy view angle.
    // 敌人的视野角度
    public float viewAngle = 120f;

    // Random movement direction change interval.
    // 随机移动方向改变间隔
    public float wanderChangeTime = 2f;

    // Layers that block enemy vision and movement.
    // 会阻挡敌人视野和移动的 Layer
    public LayerMask obstacleLayerMask;

    // Current random movement direction.
    // 当前随机移动方向
    private Vector3 wanderDirection;

    // Timer for changing random direction.
    // 改变随机方向的计时器
    private float wanderTimer;

    public override void Initialize(EnemyCore newEnemyCore)
    {
        base.Initialize(newEnemyCore);

        // Pick a random direction when the module starts.
        // 模块开始时先随机一个移动方向
        PickNewWanderDirection();
    }

    public override void TickMovement()
    {
        if (enemyCore == null)
        {
            return;
        }

        if (enemyCore.PlayerTarget != null && CanSeePlayer())
        {
            ChasePlayer();
        }
        else
        {
            WanderMove();
        }
    }

    private bool CanSeePlayer()
    {
        if (enemyCore.PlayerTarget == null)
        {
            return false;
        }

        Vector3 enemyEyePosition = enemyCore.transform.position + Vector3.up * 1f;
        Vector3 playerEyePosition = enemyCore.PlayerTarget.position + Vector3.up * 1f;

        Vector3 directionToPlayer = playerEyePosition - enemyEyePosition;
        float distanceToPlayer = directionToPlayer.magnitude;

        // Player is too far away.
        // 玩家太远，看不到
        if (distanceToPlayer > viewDistance)
        {
            return false;
        }

        directionToPlayer.Normalize();

        // Check if player is inside view angle.
        // 检查玩家是否在视野角度内
        float angleToPlayer = Vector3.Angle(enemyCore.transform.forward, directionToPlayer);

        if (angleToPlayer > viewAngle * 0.5f)
        {
            return false;
        }

        // If something solid blocks the path, enemy cannot see player.
        // 如果中间有实体挡住，敌人就看不到玩家
        if (IsPathBlocked(enemyEyePosition, directionToPlayer, distanceToPlayer, obstacleLayerMask))
        {
            return false;
        }

        return true;
    }

    private void ChasePlayer()
    {
        Vector3 directionToPlayer = GetFlatDirectionToPlayer();
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer <= stopDistance)
        {
            FaceDirection(directionToPlayer);
            return;
        }

        Vector3 moveDirection = directionToPlayer.normalized;

        // Try to avoid walls.
        // 尝试避开墙
        moveDirection = GetWallAvoidanceDirection(moveDirection, obstacleLayerMask);

        MoveEnemy(moveDirection, moveSpeed);
        FaceDirection(moveDirection);
    }

    private void WanderMove()
    {
        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f)
        {
            PickNewWanderDirection();
        }

        Vector3 moveDirection = GetWallAvoidanceDirection(wanderDirection, obstacleLayerMask);

        MoveEnemy(moveDirection, moveSpeed);
        FaceDirection(moveDirection);
    }

    private void PickNewWanderDirection()
    {
        wanderTimer = wanderChangeTime;

        float randomX = Random.Range(-1f, 1f);
        float randomZ = Random.Range(-1f, 1f);

        wanderDirection = new Vector3(randomX, 0f, randomZ).normalized;

        if (wanderDirection.sqrMagnitude < 0.01f)
        {
            wanderDirection = Vector3.forward;
        }
    }
}
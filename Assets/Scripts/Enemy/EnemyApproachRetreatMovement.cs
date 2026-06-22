using UnityEngine;

public class EnemyApproachRetreatMovement : EnemyMovementModule
{
    // Enemy movement speed.
    // 敌人移动速度
    public float moveSpeed = 3.5f;

    // Enemy first approaches until it reaches this distance.
    // 敌人会先靠近到这个距离
    public float approachDistance = 5f;

    // Enemy tries not to get closer than this distance.
    // 敌人尽量不要比这个距离更近
    public float minDistance = 2f;

    // Enemy tries not to go farther than this distance after approaching.
    // 敌人靠近后尽量不要退得超过这个距离
    public float maxDistance = 7f;

    // How often enemy changes forward/backward behavior.
    // 敌人前进后退状态改变间隔
    public float behaviorChangeTime = 1.2f;

    // How much random side movement the enemy gets.
    // 敌人左右随机移动强度
    public float sideMoveAmount = 0.7f;

    // Layers that block movement.
    // 会阻挡移动的 Layer
    public LayerMask obstacleLayerMask;

    // Whether enemy has reached the player area once.
    // 敌人是否已经接近过玩家
    private bool hasReachedPlayerArea = false;

    // 1 means move toward player, -1 means move away from player.
    // 1 表示靠近玩家，-1 表示远离玩家
    private int forwardBackwardDirection = 1;

    // Random side direction.
    // 随机左右方向
    private float sideDirection = 0f;

    // Timer for changing movement behavior.
    // 改变移动行为的计时器
    private float behaviorTimer;

    public override void Initialize(EnemyCore newEnemyCore)
    {
        base.Initialize(newEnemyCore);

        // Pick a movement behavior when the module starts.
        // 模块开始时先随机一个移动行为
        PickNewMovementBehavior();
    }

    public override void TickMovement()
    {
        if (enemyCore == null || enemyCore.PlayerTarget == null)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(
            enemyCore.transform.position,
            enemyCore.PlayerTarget.position
        );

        if (!hasReachedPlayerArea)
        {
            ApproachPlayer(distanceToPlayer);
        }
        else
        {
            MoveForwardBackwardAroundPlayer(distanceToPlayer);
        }
    }

    private void ApproachPlayer(float distanceToPlayer)
    {
        Vector3 directionToPlayer = GetFlatDirectionToPlayer();

        if (distanceToPlayer <= approachDistance)
        {
            hasReachedPlayerArea = true;
            PickNewMovementBehavior();
            return;
        }

        Vector3 moveDirection = directionToPlayer.normalized;
        moveDirection = GetWallAvoidanceDirection(moveDirection, obstacleLayerMask);

        MoveEnemy(moveDirection, moveSpeed);
        FaceDirection(moveDirection);
    }

    private void MoveForwardBackwardAroundPlayer(float distanceToPlayer)
    {
        behaviorTimer -= Time.deltaTime;

        if (behaviorTimer <= 0f)
        {
            PickNewMovementBehavior();
        }

        Vector3 directionToPlayer = GetFlatDirectionToPlayer();

        if (directionToPlayer.sqrMagnitude <= 0.001f)
        {
            return;
        }

        directionToPlayer.Normalize();

        // If too close, force moving away.
        // 如果太近，强制后退
        if (distanceToPlayer < minDistance)
        {
            forwardBackwardDirection = -1;
        }

        // If too far, force moving closer.
        // 如果太远，强制靠近
        if (distanceToPlayer > maxDistance)
        {
            forwardBackwardDirection = 1;
        }

        // Side direction is perpendicular to player direction.
        // 左右方向是到玩家方向的垂直方向
        Vector3 sideDirectionVector = Vector3.Cross(Vector3.up, directionToPlayer).normalized;

        Vector3 moveDirection =
            directionToPlayer * forwardBackwardDirection +
            sideDirectionVector * sideDirection * sideMoveAmount;

        moveDirection.y = 0f;
        moveDirection.Normalize();

        moveDirection = GetWallAvoidanceDirection(moveDirection, obstacleLayerMask);

        MoveEnemy(moveDirection, moveSpeed);

        // This enemy faces the player while moving.
        // 这个敌人移动时一直面向玩家
        FaceDirection(directionToPlayer);
    }

    private void PickNewMovementBehavior()
    {
        behaviorTimer = behaviorChangeTime;

        // Randomly choose forward or backward.
        // 随机选择靠近或后退
        if (Random.value > 0.5f)
        {
            forwardBackwardDirection = 1;
        }
        else
        {
            forwardBackwardDirection = -1;
        }

        // Random side movement amount.
        // 随机左右移动幅度
        sideDirection = Random.Range(-1f, 1f);
    }
}
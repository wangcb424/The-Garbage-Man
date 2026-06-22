using UnityEngine;

public abstract class EnemyWeaponModule : MonoBehaviour
{
    // Enemy core that owns this weapon module.
    // 拥有这个武器模块的 EnemyCore
    protected EnemyCore enemyCore;

    public virtual void Initialize(EnemyCore newEnemyCore)
    {
        // Store enemy core.
        // 保存 EnemyCore
        enemyCore = newEnemyCore;
    }

    public virtual void TickWeapon()
    {
        // Some weapons do not need update logic.
        // 有些武器不需要每帧逻辑
    }

    public virtual void OnEnemyTriggerEnter(Collider other)
    {
        // Some weapons do not need trigger logic.
        // 有些武器不需要 trigger 逻辑
    }

    public virtual void OnEnemyCollisionEnter(Collision collision)
    {
        // Some weapons do not need collision logic.
        // 有些武器不需要 collision 逻辑
    }
}
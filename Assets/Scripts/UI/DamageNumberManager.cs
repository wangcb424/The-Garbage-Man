using UnityEngine;

public class DamageNumberManager : MonoBehaviour
{
    public GameObject damageNumberPrefab;
    public Canvas damageCanvas;

    private void OnEnable()
    {
        EnemyCore.OnEnemyHit += HandleEnemyHit;
    }

    private void OnDisable()
    {
        EnemyCore.OnEnemyHit -= HandleEnemyHit;
    }

    private void HandleEnemyHit(Vector3 position, int damage)
    {
        DamageNumber.Spawn(position, damage, damageNumberPrefab, damageCanvas);
    }
}
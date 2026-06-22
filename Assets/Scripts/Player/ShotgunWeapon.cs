using UnityEngine;

public class ShotgunWeapon : PlayerWeapon
{
    public int pelletCount = 5;
    public float spreadAngle = 45f;

    protected override void FireWeapon()
    {
        if (bulletPrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = transform.position + transform.forward * 0.5f + Vector3.up * 0.2f;

        if (firePoint != null)
        {
            spawnPosition = firePoint.position;
        }

        Vector3 baseDirection = weaponHolder.GetPlayerTransform().forward;
        baseDirection.y = 0f;

        if (baseDirection.sqrMagnitude <= 0.001f)
        {
            return;
        }

        baseDirection.Normalize();

        float halfSpread = spreadAngle * 0.5f;
        float angleStep = pelletCount > 1 ? spreadAngle / (pelletCount - 1) : 0f;

        for (int i = 0; i < pelletCount; i++)
        {
            float angle = -halfSpread + angleStep * i;
            Vector3 pelletDirection = Quaternion.Euler(0f, angle, 0f) * baseDirection;

            GameObject bulletObject = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);

            PlayerBullet bullet = bulletObject.GetComponent<PlayerBullet>();

            if (bullet != null)
            {
                bullet.Launch(pelletDirection.normalized, bulletSpeed);
            }
        }

        InvokeOnShoot();
    }
}
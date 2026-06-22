using UnityEngine;

public class DungeonWallTile : MonoBehaviour
{
    // This enum defines what kind of wall this is.
    // Normal walls just block movement.
    // Bounce walls reflect bullets when bullets collide with them.
    public enum WallType
    {
        Normal,
        Bounce
    }

    // This variable stores the wall type for this prefab.
    // 跟floor的一样
    public WallType wallType = WallType.Normal;

    // This controls how fast the bullet moves after bouncing.
    // 反弹速度
    public float bounceForce = 12f;

    private void OnCollisionEnter(Collision collision)
    {
        // Only bounce bullets if this wall is a bounce wall.
        // check墙的种类
        if (wallType != WallType.Bounce)
        {
            return;
        }

        TryBounceBullet(collision);
    }

    private void TryBounceBullet(Collision collision)
    {
        // Only objects tagged as Bullet should bounce.
        // 定义反弹tag
        if (!collision.gameObject.CompareTag("Bullet"))
        {
            return;
        }

        // 下面都是防止出错
        Rigidbody bulletRb = collision.gameObject.GetComponent<Rigidbody>();

        // If the bullet does not have a Rigidbody, we cannot change its velocity.
        if (bulletRb == null)
        {
            return;
        }

        // Make sure there is a collision contact point.
        if (collision.contacts.Length == 0)
        {
            return;
        }

        // 存子弹的variable
        // Get the current bullet movement direction.
        Vector3 incomingDirection = bulletRb.linearVelocity.normalized;

        // Get the wall surface direction from the collision.
        Vector3 wallNormal = collision.contacts[0].normal;

        // Reflect the bullet direction based on the wall normal.
        Vector3 bounceDirection = Vector3.Reflect(incomingDirection, wallNormal);

        // Apply the new bounced velocity.
        bulletRb.linearVelocity = bounceDirection * bounceForce;
    }
}
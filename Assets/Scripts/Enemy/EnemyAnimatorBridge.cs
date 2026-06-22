using UnityEngine;

public class EnemyAnimatorBridge : MonoBehaviour
{
    public Animator animator;
    public string movingParameterName = "IsMoving";
    public string retreatingParameterName = "IsRetreating";
    public float moveThreshold = 0.01f;
    private Vector3 lastPosition;

    private void Start()
    {
        lastPosition = transform.position;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        if (animator == null)
        {
            return;
        }
        Vector3 currentPosition = transform.position;
        Vector3 movement = currentPosition - lastPosition;
        movement.y = 0f;

        bool isMoving = movement.sqrMagnitude > moveThreshold * moveThreshold;

        bool isRetreating = false;

        if (isMoving)
        {
            Vector3 moveDirection = movement.normalized;
            Vector3 forwardDirection = transform.forward;
            forwardDirection.y = 0f;
            forwardDirection.Normalize();

            float dot = Vector3.Dot(forwardDirection, moveDirection);

            // dot 小于 0 表示敌人面朝玩家，但身体在后退
            isRetreating = dot < -0.2f;
        }

        animator.SetBool(movingParameterName, isMoving);
        animator.SetBool(retreatingParameterName, isRetreating);

        lastPosition = currentPosition;
    }
}

using UnityEngine;

public class ShopItemVisualRotator : MonoBehaviour
{
    // Rotation speed.
    // 旋转速度
    public float rotateSpeed = 60f;

    // Floating height.
    // 上下浮动高度
    public float floatHeight = 0.1f;

    // Floating speed.
    // 上下浮动速度
    public float floatSpeed = 2f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.localPosition;
    }

    private void Update()
    {
        RotateItem();
        FloatItem();
    }

    private void RotateItem()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    private void FloatItem()
    {
        float offsetY = Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        transform.localPosition = new Vector3(
            startPosition.x,
            startPosition.y + offsetY,
            startPosition.z
        );
    }
}
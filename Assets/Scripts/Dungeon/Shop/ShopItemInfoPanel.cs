using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemInfoPanel : MonoBehaviour
{
    [Header("UI References")]

    // Item name on the right side.
    // 右边商品名字
    public TMP_Text itemNameText;

    // Price number on the left side.
    // 左边价格数字
    public TMP_Text priceText;

    // Press E text.
    // 购买提示文字
    public TMP_Text promptText;

    // Trash icon image.
    // 垃圾图标
    public Image trashIconImage;

    [Header("Follow Settings")]

    // Offset from target item.
    // 相对商品的位置偏移
    public Vector3 offset = new Vector3(0f, 1.8f, 0f);

    // If true, panel faces camera.
    // 如果为 true，面板始终朝向摄像机
    public bool faceCamera = true;

    // Target item.
    // 跟随的商品
    private Transform target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }

    public void UpdatePanel(string itemName, int price)
    {
        if (itemNameText != null)
        {
            itemNameText.text = itemName;
        }

        if (priceText != null)
        {
            priceText.text = price.ToString();
        }

        if (promptText != null)
        {
            promptText.text = "Press E";
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = target.position + offset;

        if (faceCamera && Camera.main != null)
        {
            // Use camera rotation directly.
            // 直接使用摄像机角度，让 World Space Canvas 面向玩家视角
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}
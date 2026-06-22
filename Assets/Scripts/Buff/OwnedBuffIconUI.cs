using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OwnedBuffIconUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text countText;

    public void Setup(BuffData buffData, int count)
    {
        if (buffData == null)
        {
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = buffData.icon;
            iconImage.enabled = buffData.icon != null;
        }

        if (countText != null)
        {
            countText.text = "x" + count;
        }
    }
}
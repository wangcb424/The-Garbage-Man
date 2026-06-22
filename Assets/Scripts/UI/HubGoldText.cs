using TMPro;
using UnityEngine;

public class HubGoldText : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text goldText;

    [Header("Text Settings")]
    public string prefix = "";
    public bool useCommaFormat = true;

    private void Reset()
    {
        goldText = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        UpdateGoldText();
    }

    private void UpdateGoldText()
    {
        if (goldText == null)
        {
            return;
        }

        if (GlobalRunManager.Instance == null)
        {
            goldText.text = prefix + "0";
            return;
        }

        int hubGold = GlobalRunManager.Instance.totalGold;

        if (useCommaFormat)
        {
            goldText.text = prefix + hubGold.ToString("N0");
        }
        else
        {
            goldText.text = prefix + hubGold.ToString();
        }
    }
}
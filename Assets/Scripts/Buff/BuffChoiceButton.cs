using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffChoiceButton : MonoBehaviour
{
    [Header("Buff Data")]
    public BuffData buffData;

    [Header("Card UI")]
    public Image cardBackgroundImage;
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text effectText;
    public TMP_Text costText;
    public TMP_Text statusText;
    public Button buyButton;

    [Header("Card Colors")]
    public Color normalColor = Color.white;
    public Color notEnoughTrashColor = new Color(0.55f, 0.55f, 0.55f, 1f);
    public Color purchasedColor = new Color(0.55f, 1f, 0.65f, 1f);

    [Header("Purchase")]
    public bool canBuyOnlyOnce = true;

    private BuffChoiceManager manager;
    private bool purchased = false;

    private void Awake()
    {
        if (buyButton == null)
        {
            buyButton = GetComponent<Button>();
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(BuyBuff);
            buyButton.onClick.AddListener(BuyBuff);
        }
    }

    public void Setup(BuffData newBuffData, BuffChoiceManager newManager)
    {
        buffData = newBuffData;
        manager = newManager;
        purchased = false;

        UpdateUI();
    }

    public void BuyBuff()
    {
        if (buffData == null)
        {
            Debug.Log("BuffData is missing.");
            return;
        }

        if (GlobalRunManager.Instance == null)
        {
            Debug.Log("GlobalRunManager was not found.");
            return;
        }

        if (canBuyOnlyOnce && purchased)
        {
            SetStatus("Owned");
            return;
        }

        if (GlobalRunManager.Instance.runTrash < buffData.cost)
        {
            SetStatus("Need Trash");
            return;
        }

        bool paid = GlobalRunManager.Instance.SpendRunTrash(buffData.cost);

        if (!paid)
        {
            SetStatus("Need Trash");
            return;
        }

        buffData.ApplyBuff();

        if (canBuyOnlyOnce)
        {
            purchased = true;
        }

        SetStatus("Owned");

        if (manager != null)
        {
            manager.NotifyBuffPurchased(buffData);
            manager.RefreshAllBuffButtons();
            manager.RefreshAllUI();
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (buffData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (iconImage != null)
        {
            iconImage.sprite = buffData.icon;
            iconImage.enabled = buffData.icon != null;
        }

        if (nameText != null)
        {
            nameText.text = buffData.buffName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = buffData.description;
        }

        if (effectText != null)
        {
            effectText.text = buffData.GetEffectText();
        }

        if (costText != null)
        {
            costText.text = buffData.cost.ToString();
        }

        bool canBuy = CanBuy();

        if (buyButton != null)
        {
            buyButton.interactable = canBuy;
        }

        if (cardBackgroundImage != null)
        {
            if (purchased)
            {
                cardBackgroundImage.color = purchasedColor;
            }
            else if (!canBuy)
            {
                cardBackgroundImage.color = notEnoughTrashColor;
            }
            else
            {
                cardBackgroundImage.color = normalColor;
            }
        }

        if (statusText != null)
        {
            if (purchased)
            {
                statusText.text = "Owned";
            }
            else if (!canBuy)
            {
                statusText.text = "Need Trash";
            }
            else
            {
                statusText.text = "";
            }
        }
    }

    private bool CanBuy()
    {
        if (buffData == null)
        {
            return false;
        }

        if (GlobalRunManager.Instance == null)
        {
            return false;
        }

        if (canBuyOnlyOnce && purchased)
        {
            return false;
        }

        return GlobalRunManager.Instance.runTrash >= buffData.cost;
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }

        Debug.Log(message);
    }
}
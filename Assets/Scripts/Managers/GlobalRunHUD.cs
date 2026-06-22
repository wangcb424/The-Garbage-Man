using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalRunHUD : MonoBehaviour
{
    [Header("Health UI")]

    // Health bar fill image.
    // 血条填充图片
    public Image healthFillImage;

    // Health text.
    // 血量文字
    public TMP_Text healthText;

    [Header("Resource UI")]

    // Gold text.
    // 金币文字
    public TMP_Text goldText;

    // Trash text.
    // 垃圾文字
    public TMP_Text trashText;

    [Header("Item UI")]

    // Heal potion count text.
    // 血药数量文字
    public TMP_Text healPotionText;

    [Header("Level UI")]

    // Level text.
    // 关卡文字
    public TMP_Text levelText;

    private void Update()
    {
        UpdateHUD();
    }

    private void UpdateHUD()
    {
        if (GlobalRunManager.Instance == null)
        {
            return;
        }

        UpdateHealthUI();
        UpdateResourceUI();
        UpdateItemUI();
        UpdateLevelUI();
    }

    private void UpdateHealthUI()
    {
        int currentHealth = GlobalRunManager.Instance.currentHealth;
        int maxHealth = GlobalRunManager.Instance.maxHealth;

        if (maxHealth <= 0)
        {
            maxHealth = 1;
        }

        float healthPercent = currentHealth / (float)maxHealth;

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = healthPercent;
        }

        if (healthText != null)
        {
            healthText.text = currentHealth + " / " + maxHealth;
        }
    }

    private void UpdateResourceUI()
    {
        if (goldText != null)
        {
            goldText.text = GlobalRunManager.Instance.runGold.ToString();
        }

        if (trashText != null)
        {
            trashText.text = GlobalRunManager.Instance.runTrash.ToString();
        }
    }

    private void UpdateItemUI()
    {
        if (healPotionText != null)
        {
            healPotionText.text = "x " + GlobalRunManager.Instance.healPotionCount;
        }
    }

    private void UpdateLevelUI()
    {
        if (levelText == null)
        {
            return;
        }

        int dungeonSceneNumber =
            GlobalRunManager.Instance.currentDungeonSceneBuildIndex -
            GlobalRunManager.Instance.firstDungeonSceneBuildIndex +
            1;

        int smallLevelNumber = GlobalRunManager.Instance.currentSmallLevel;

        levelText.text = "Level " + dungeonSceneNumber + "-" + smallLevelNumber;
    }
}
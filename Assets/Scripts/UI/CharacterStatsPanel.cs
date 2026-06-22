using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterStatsPanel : MonoBehaviour
{
    [Header("Stats Text")]

    public TMP_Text healthText;
    public TMP_Text moveSpeedText;
    public TMP_Text attackSpeedText;
    public TMP_Text critChanceText;
    public TMP_Text blockReductionText;
    public TMP_Text healPotionText;
    public TMP_Text goldText;
    public TMP_Text trashText;
    public TMP_Text levelText;
    public TMP_Text weaponText;

    private void OnEnable()
    {
        UpdateStatsPanel();
    }

    private void Update()
    {
        UpdateStatsPanel();
    }

    private void UpdateStatsPanel()
    {
        if (GlobalRunManager.Instance == null)
        {
            return;
        }

        GlobalRunManager manager = GlobalRunManager.Instance;

        if (healthText != null)
        {
            healthText.text = "Health: " + manager.currentHealth + " / " + manager.maxHealth;
        }

        if (moveSpeedText != null)
        {
            moveSpeedText.text = "Move Speed: " + manager.moveSpeed.ToString("F1");
        }

        if (attackSpeedText != null)
        {
            attackSpeedText.text = "Attack Speed: x" + manager.attackSpeedMultiplier.ToString("F2");
        }

        if (critChanceText != null)
        {
            critChanceText.text = "Crit Chance: " + (manager.critChance * 100f).ToString("F0") + "%";
        }

        if (blockReductionText != null)
        {
            blockReductionText.text = "Block Reduction: " + (manager.blockDamageReduction * 100f).ToString("F0") + "%";
        }

        if (healPotionText != null)
        {
            healPotionText.text = "Heal Potions: x " + manager.healPotionCount;
        }

        if (goldText != null)
        {
            goldText.text = "Run Gold: " + manager.runGold;
        }

        if (trashText != null)
        {
            trashText.text = "Trash: " + manager.runTrash;
        }

        if (levelText != null)
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

            if (currentSceneIndex == manager.hubSceneBuildIndex)
            {
                levelText.text = "Location: Hub";
            }
            else
            {
                int dungeonSceneNumber =
                    manager.currentDungeonSceneBuildIndex -
                    manager.firstDungeonSceneBuildIndex +
                    1;

                levelText.text = "Level: " + dungeonSceneNumber + "-" + manager.currentSmallLevel;
            }
        }

        if (weaponText != null)
        {
            if (manager.currentWeaponPrefab != null)
            {
                weaponText.text = "Weapon: " + manager.currentWeaponPrefab.name;
            }
            else
            {
                weaponText.text = "Weapon: None";
            }
        }
    }
}
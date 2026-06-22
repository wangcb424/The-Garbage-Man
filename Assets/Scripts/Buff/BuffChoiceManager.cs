using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class BuffChoiceManager : MonoBehaviour
{
    [Header("Buff Shop")]
    public List<BuffData> allBuffs = new List<BuffData>();
    public List<BuffChoiceButton> buffButtons = new List<BuffChoiceButton>();

    [Header("Top UI")]
    [FormerlySerializedAs("runGoldText")]
    public TMP_Text runTrashText;

    public TMP_Text levelText;

    [Header("Stats UI")]
    public TMP_Text healthText;
    public TMP_Text maxHealthText;
    public TMP_Text moveSpeedText;
    public TMP_Text attackSpeedText;
    public TMP_Text critChanceText;
    public TMP_Text blockReductionText;

    [Header("Owned Buff UI")]
    public Transform ownedBuffIconParent;
    public OwnedBuffIconUI ownedBuffIconPrefab;

    private Dictionary<BuffData, int> ownedBuffCounts = new Dictionary<BuffData, int>();

    private float refreshTimer = 0f;

    private void Start()
    {
        SetupBuffShop();
        RefreshAllUI();
    }

    private void Update()
    {
        refreshTimer -= Time.deltaTime;

        if (refreshTimer <= 0f)
        {
            refreshTimer = 0.2f;
            RefreshAllUI();
        }
    }

    private void SetupBuffShop()
    {
        if (buffButtons == null || buffButtons.Count == 0)
        {
            Debug.Log("BuffChoiceManager has no buff buttons.");
            return;
        }

        for (int i = 0; i < buffButtons.Count; i++)
        {
            if (buffButtons[i] == null)
            {
                continue;
            }

            if (allBuffs == null || i >= allBuffs.Count || allBuffs[i] == null)
            {
                buffButtons[i].gameObject.SetActive(false);
                continue;
            }

            buffButtons[i].gameObject.SetActive(true);
            buffButtons[i].Setup(allBuffs[i], this);
        }
    }

    public void RefreshAllUI()
    {
        UpdateTopUI();
        UpdateStatsUI();
        RefreshAllBuffButtons();
    }

    public void RefreshAllBuffButtons()
    {
        if (buffButtons == null)
        {
            return;
        }

        for (int i = 0; i < buffButtons.Count; i++)
        {
            if (buffButtons[i] != null)
            {
                buffButtons[i].UpdateUI();
            }
        }
    }

    public void NotifyBuffPurchased(BuffData buffData)
    {
        if (buffData == null)
        {
            return;
        }

        if (!ownedBuffCounts.ContainsKey(buffData))
        {
            ownedBuffCounts.Add(buffData, 0);
        }

        ownedBuffCounts[buffData]++;

        RebuildOwnedBuffIcons();
    }

    private void UpdateTopUI()
    {
        if (GlobalRunManager.Instance == null)
        {
            if (runTrashText != null)
            {
                runTrashText.text = "0";
            }

            if (levelText != null)
            {
                levelText.text = "Level";
            }

            return;
        }

        GlobalRunManager manager = GlobalRunManager.Instance;

        if (runTrashText != null)
        {
            runTrashText.text = manager.runTrash.ToString();
        }

        if (levelText != null)
        {
            levelText.text = "Level " + manager.GetCurrentLevelText();
        }
    }

    private void UpdateStatsUI()
    {
        if (GlobalRunManager.Instance == null)
        {
            return;
        }

        GlobalRunManager manager = GlobalRunManager.Instance;

        if (healthText != null)
        {
            healthText.text = manager.currentHealth + " / " + manager.maxHealth;
        }

        if (maxHealthText != null)
        {
            maxHealthText.text = manager.maxHealth.ToString();
        }

        if (moveSpeedText != null)
        {
            moveSpeedText.text = manager.moveSpeed.ToString("F1");
        }

        if (attackSpeedText != null)
        {
            attackSpeedText.text = "x" + manager.attackSpeedMultiplier.ToString("F2");
        }

        if (critChanceText != null)
        {
            critChanceText.text = (manager.critChance * 100f).ToString("F0") + "%";
        }

        if (blockReductionText != null)
        {
            blockReductionText.text = (manager.blockDamageReduction * 100f).ToString("F0") + "%";
        }
    }

    private void RebuildOwnedBuffIcons()
    {
        if (ownedBuffIconParent == null)
        {
            return;
        }

        if (ownedBuffIconPrefab == null)
        {
            return;
        }

        for (int i = ownedBuffIconParent.childCount - 1; i >= 0; i--)
        {
            Destroy(ownedBuffIconParent.GetChild(i).gameObject);
        }

        foreach (KeyValuePair<BuffData, int> pair in ownedBuffCounts)
        {
            BuffData buffData = pair.Key;
            int count = pair.Value;

            OwnedBuffIconUI iconUI = Instantiate(
                ownedBuffIconPrefab,
                ownedBuffIconParent
            );

            iconUI.Setup(buffData, count);
        }
    }

    public void ContinueToNextDungeon()
    {
        if (GlobalRunManager.Instance == null)
        {
            Debug.Log("GlobalRunManager was not found.");
            return;
        }

        GlobalRunManager.Instance.ContinueAfterBuffChoice();
    }

    public void RetreatAndCashOut()
    {
        if (GlobalRunManager.Instance == null)
        {
            Debug.Log("GlobalRunManager was not found.");
            return;
        }

        GlobalRunManager.Instance.RetreatAndCashOutRun();
    }
}
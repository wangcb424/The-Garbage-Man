using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HubHUD : MonoBehaviour
{
    public Slider atmProgressBar;
    public TMP_Text atmProgressText;
    public GameObject winScreenPanel;
    public MilestonePopupUI milestonePopup;

    private void OnEnable()
    {
        GlobalRunManager.OnMilestoneReached += HandleMilestoneReached;
        GlobalRunManager.OnGameWin += HandleGameWin;
    }

    private void OnDisable()
    {
        GlobalRunManager.OnMilestoneReached -= HandleMilestoneReached;
        GlobalRunManager.OnGameWin -= HandleGameWin;
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (GlobalRunManager.Instance == null)
        {
            return;
        }

        float progress = (float)GlobalRunManager.Instance.atmTotalDeposited /
                         (float)GlobalRunManager.Instance.atmGoal;

        progress = Mathf.Clamp01(progress);

        if (atmProgressBar != null)
        {
            atmProgressBar.value = progress;
        }

        if (atmProgressText != null)
        {
            int deposited = GlobalRunManager.Instance.atmTotalDeposited;
            int goal = GlobalRunManager.Instance.atmGoal;
            atmProgressText.text = "ATM: " + deposited + " / " + goal;
        }
    }

    private void HandleMilestoneReached(int milestoneIndex)
    {
        if (milestonePopup == null)
        {
            return;
        }

        string reward = GetMilestoneRewardDescription(milestoneIndex);
        milestonePopup.ShowMilestone(milestoneIndex, reward);
    }

    private string GetMilestoneRewardDescription(int milestoneIndex)
    {
        switch (milestoneIndex)
        {
            case 1: return "+1 Starting Potion";
            case 2: return "+5 Potion Heal Amount";
            case 3: return "New Weapon Unlocked!";
            case 4: return "+1 Starting Potion";
            case 5: return "+5 Potion Heal Amount";
            case 6: return "New Weapon Unlocked!";
            case 7: return "+0.2 Move Speed";
            case 8: return "+0.2 Move Speed";
            case 9: return "+0.2 Move Speed";
            case 10: return "You Win!";
            default: return "Reward";
        }
    }

    private void HandleGameWin()
    {
        if (milestonePopup != null)
        {
            milestonePopup.gameObject.SetActive(false);
        }

        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }
}
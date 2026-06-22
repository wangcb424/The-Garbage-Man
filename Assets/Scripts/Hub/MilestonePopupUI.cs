using System.Collections;
using TMPro;
using UnityEngine;

public class MilestonePopupUI : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text rewardText;
    public float displayDuration = 3f;

    private Coroutine hideCoroutine;

    public void ShowMilestone(int milestoneIndex, string rewardDescription)
    {
        if (titleText != null)
        {
            titleText.text = "Milestone " + milestoneIndex + " Reached!";
        }

        if (rewardText != null)
        {
            rewardText.text = rewardDescription;
        }

        gameObject.SetActive(true);

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(displayDuration);
        gameObject.SetActive(false);
    }
}
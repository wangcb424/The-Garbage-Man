using UnityEngine;

public class WinScreenUI : MonoBehaviour
{
    public void OnResetButtonClicked()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);

        if (GlobalRunManager.Instance != null)
        {
            GlobalRunManager.Instance.ResetAllSaveData();
        }
    }

    public void OnContinueButtonClicked()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

}
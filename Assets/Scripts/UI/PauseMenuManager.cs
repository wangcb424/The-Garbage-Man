using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public enum PausePage
    {
        Main,
        Stats,
        Settings
    }

    [Header("Menu Root")]

    // Whole pause menu root.
    // 整个暂停菜单根物体
    public GameObject pauseMenuRoot;

    [Header("Pages")]

    // Main menu page.
    // 主菜单页面
    public GameObject mainPage;

    // Character stats page.
    // 角色属性页面
    public GameObject statsPage;

    // Settings page. This can be empty for now.
    // 设置页面，现在可以先不拖
    public GameObject settingsPage;

    [Header("Page Number Text")]

    // MainPage page number text.
    // MainPage 的页数文字
    public TMP_Text mainPageNumberText;

    // StatsPage page number text.
    // StatsPage 的页数文字
    public TMP_Text statsPageNumberText;

    [Header("Input Settings")]

    // Pause key.
    // 暂停按键
    public KeyCode pauseKey = KeyCode.Escape;

    // Left page switch key.
    // 左切换按键
    public KeyCode previousPageKey = KeyCode.Q;

    // Right page switch key.
    // 右切换按键
    public KeyCode nextPageKey = KeyCode.E;

    [Header("Pause State")]

    // Whether the game is paused.
    // 当前是否暂停
    public bool isPaused = false;

    // Current page.
    // 当前页面
    public PausePage currentPage = PausePage.Main;

    [Header("Cursor Settings")]

    // Show cursor when pause menu is open.
    // 暂停菜单打开时显示鼠标
    public bool showCursorWhenPaused = true;

    private void Start()
    {
        ForceCloseMenuAtStart();
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePauseMenu();
        }

        if (!isPaused)
        {
            return;
        }

        HandlePageSwitchInput();
    }

    private void ForceCloseMenuAtStart()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.SetActive(false);
        }

        currentPage = PausePage.Main;

        if (mainPage != null)
        {
            mainPage.SetActive(true);
        }

        if (statsPage != null)
        {
            statsPage.SetActive(false);
        }

        if (settingsPage != null)
        {
            settingsPage.SetActive(false);
        }

        UpdatePageNumberText();
    }

    private void HandlePageSwitchInput()
    {
        if (Input.GetKeyDown(previousPageKey))
        {
            SwitchMainAndStatsPage();
        }

        if (Input.GetKeyDown(nextPageKey))
        {
            SwitchMainAndStatsPage();
        }
    }

    private void SwitchMainAndStatsPage()
    {
        // Q / E only switches between MainPage and StatsPage.
        // Q / E 只在 MainPage 和 StatsPage 之间切换
        if (currentPage == PausePage.Main)
        {
            ShowStatsPage();
            return;
        }

        if (currentPage == PausePage.Stats)
        {
            ShowMainPage();
            return;
        }

        // SettingsPage does not use Q / E.
        // SettingsPage 不用 Q / E 切换
        if (currentPage == PausePage.Settings)
        {
            return;
        }
    }

    public void TogglePauseMenu()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            OpenPauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.SetActive(true);
        }

        ShowMainPage();

        if (showCursorWhenPaused)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void ClosePauseMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.SetActive(false);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        ClosePauseMenu();
    }

    public void ShowMainPage()
    {
        currentPage = PausePage.Main;

        if (mainPage != null)
        {
            mainPage.SetActive(true);
        }

        if (statsPage != null)
        {
            statsPage.SetActive(false);
        }

        if (settingsPage != null)
        {
            settingsPage.SetActive(false);
        }

        UpdatePageNumberText();
    }

    public void ShowStatsPage()
    {
        currentPage = PausePage.Stats;

        if (mainPage != null)
        {
            mainPage.SetActive(false);
        }

        if (statsPage != null)
        {
            statsPage.SetActive(true);
        }

        if (settingsPage != null)
        {
            settingsPage.SetActive(false);
        }

        UpdatePageNumberText();
    }

    public void ShowSettingsPage()
    {
        currentPage = PausePage.Settings;

        if (mainPage != null)
        {
            mainPage.SetActive(false);
        }

        if (statsPage != null)
        {
            statsPage.SetActive(false);
        }

        if (settingsPage != null)
        {
            settingsPage.SetActive(true);
        }

        UpdatePageNumberText();
    }

    private void UpdatePageNumberText()
    {
        if (mainPageNumberText != null)
        {
            mainPageNumberText.text = "1 / 2";
        }

        if (statsPageNumberText != null)
        {
            statsPageNumberText.text = "2 / 2";
        }
    }

    public void QuitToHub()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (GlobalRunManager.Instance != null)
        {
            GlobalRunManager.Instance.QuitCurrentRunToHub();
        }
        else
        {
            SceneManager.LoadScene(1);
        }
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (GlobalRunManager.Instance != null)
        {
            GlobalRunManager.Instance.SaveAndQuitGame();
            return;
        }

        Debug.Log("Quit Game");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }
}
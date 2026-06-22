using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroSequenceManager : MonoBehaviour
{
    private const int HubSceneBuildIndex = 1;

    [Header("Page Data")]
    public List<IntroPageData> introPages = new List<IntroPageData>();

    [Header("UI Images")]
    public Image backgroundImage;
    public Image storyImage;

    [Header("Canvas Groups")]
    public CanvasGroup pageCanvasGroup;
    public CanvasGroup fadeCanvasGroup;

    [Header("UI Text")]
    public TMP_Text titleText;
    public TMP_Text bodyText;
    public TMP_Text continueHintText;
    public TMP_Text skipHintText;

    [Header("Timing")]
    public float fadeDuration = 0.6f;
    public float typewriterSpeed = 0.025f;
    public float pageStartDelay = 0.15f;

    [Header("Input")]
    public KeyCode nextKey = KeyCode.Space;
    public KeyCode skipKey = KeyCode.Escape;

    [Header("Hint Text")]
    public string continueHintMessage = "Click / Space to continue";

    [Header("Save")]
    public bool saveIntroPlayed = true;
    public string introPlayedKey = "IntroPlayed";

    private int currentPageIndex = 0;

    private bool isTyping = false;
    private bool isChangingPage = false;
    private bool isLoadingHub = false;

    private Coroutine typingCoroutine;
    private Coroutine pageRoutine;

    private string currentFullTitle = "";
    private string currentFullBody = "";

    private void Awake()
    {
        Time.timeScale = 1f;

        AutoFindMissingReferences();
    }

    private void Start()
    {
        SetupInitialUI();
        StartCoroutine(StartIntroRoutine());
    }

    private void Update()
    {
        if (isLoadingHub)
        {
            return;
        }

        UpdateHintText();

        if (Input.GetKeyDown(skipKey))
        {
            LoadHubImmediately();
            return;
        }

        if (Input.GetMouseButtonDown(0) ||
            Input.GetKeyDown(nextKey) ||
            Input.GetKeyDown(KeyCode.Return))
        {
            HandleNextInput();
        }
    }

    private void AutoFindMissingReferences()
    {
        if (skipHintText == null)
        {
            GameObject skipHintObject = GameObject.Find("SkipHintText");

            if (skipHintObject != null)
            {
                skipHintText = skipHintObject.GetComponent<TMP_Text>();
            }
        }

        if (continueHintText == null)
        {
            GameObject continueHintObject = GameObject.Find("ContinueHintText");

            if (continueHintObject != null)
            {
                continueHintText = continueHintObject.GetComponent<TMP_Text>();
            }
        }

        if (titleText == null)
        {
            GameObject titleObject = GameObject.Find("TitleText");

            if (titleObject != null)
            {
                titleText = titleObject.GetComponent<TMP_Text>();
            }
        }

        if (bodyText == null)
        {
            GameObject bodyObject = GameObject.Find("BodyText");

            if (bodyObject != null)
            {
                bodyText = bodyObject.GetComponent<TMP_Text>();
            }
        }
    }

    private void SetupInitialUI()
    {
        Time.timeScale = 1f;

        if (pageCanvasGroup != null)
        {
            pageCanvasGroup.alpha = 0f;
            pageCanvasGroup.blocksRaycasts = false;
            pageCanvasGroup.interactable = false;
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = true;
            fadeCanvasGroup.interactable = false;
        }

        if (continueHintText != null)
        {
            continueHintText.text = continueHintMessage;
        }

        UpdateHintText();

        if (titleText != null)
        {
            titleText.text = "";
        }

        if (bodyText != null)
        {
            bodyText.text = "";
        }
    }

    private void UpdateHintText()
    {
        if (skipHintText == null)
        {
            return;
        }

        skipHintText.text = GetKeyDisplayName(skipKey) + " to skip";
    }

    private string GetKeyDisplayName(KeyCode key)
    {
        if (key == KeyCode.Escape)
        {
            return "Esc";
        }

        if (key == KeyCode.Space)
        {
            return "Space";
        }

        if (key == KeyCode.Return)
        {
            return "Enter";
        }

        if (key == KeyCode.Mouse0)
        {
            return "Click";
        }

        return key.ToString();
    }

    private IEnumerator StartIntroRoutine()
    {
        yield return FadeScreenFromBlack();

        ShowCurrentPage();
    }

    private void HandleNextInput()
    {
        if (isChangingPage)
        {
            return;
        }

        if (isTyping)
        {
            FinishTypingImmediately();
            return;
        }

        GoToNextPage();
    }

    private void ShowCurrentPage()
    {
        if (isLoadingHub)
        {
            return;
        }

        if (introPages == null || introPages.Count == 0)
        {
            FinishIntroWithFade();
            return;
        }

        if (currentPageIndex < 0 || currentPageIndex >= introPages.Count)
        {
            FinishIntroWithFade();
            return;
        }

        IntroPageData page = introPages[currentPageIndex];

        if (page == null)
        {
            currentPageIndex++;
            ShowCurrentPage();
            return;
        }

        currentFullTitle = page.pageTitle;
        currentFullBody = page.pageBody;

        if (backgroundImage != null)
        {
            backgroundImage.sprite = page.backgroundImage;
            backgroundImage.enabled = page.backgroundImage != null;
        }

        if (storyImage != null)
        {
            storyImage.sprite = page.storyImage;
            storyImage.enabled = page.storyImage != null;
        }

        if (titleText != null)
        {
            titleText.text = "";
        }

        if (bodyText != null)
        {
            bodyText.text = "";
        }

        StopCurrentPageRoutines();

        pageRoutine = StartCoroutine(ShowPageRoutine());
    }

    private IEnumerator ShowPageRoutine()
    {
        isChangingPage = true;

        if (pageCanvasGroup != null)
        {
            pageCanvasGroup.alpha = 0f;
        }

        yield return WaitUnscaled(pageStartDelay);

        yield return FadePageIn();

        isChangingPage = false;

        typingCoroutine = StartCoroutine(TypePageText());
    }

    private IEnumerator TypePageText()
    {
        isTyping = true;

        if (titleText != null)
        {
            titleText.text = "";
        }

        if (bodyText != null)
        {
            bodyText.text = "";
        }

        for (int i = 0; i <= currentFullTitle.Length; i++)
        {
            if (isLoadingHub)
            {
                yield break;
            }

            if (titleText != null)
            {
                titleText.text = currentFullTitle.Substring(0, i);
            }

            yield return WaitUnscaled(typewriterSpeed);
        }

        yield return WaitUnscaled(0.15f);

        for (int i = 0; i <= currentFullBody.Length; i++)
        {
            if (isLoadingHub)
            {
                yield break;
            }

            if (bodyText != null)
            {
                bodyText.text = currentFullBody.Substring(0, i);
            }

            yield return WaitUnscaled(typewriterSpeed);
        }

        isTyping = false;
    }

    private void FinishTypingImmediately()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (titleText != null)
        {
            titleText.text = currentFullTitle;
        }

        if (bodyText != null)
        {
            bodyText.text = currentFullBody;
        }

        isTyping = false;
    }

    public void GoToNextPage()
    {
        if (isLoadingHub)
        {
            return;
        }

        if (isChangingPage)
        {
            return;
        }

        if (pageRoutine != null)
        {
            StopCoroutine(pageRoutine);
            pageRoutine = null;
        }

        pageRoutine = StartCoroutine(GoToNextPageRoutine());
    }

    private IEnumerator GoToNextPageRoutine()
    {
        isChangingPage = true;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;

        yield return FadePageOut();

        currentPageIndex++;

        if (currentPageIndex >= introPages.Count)
        {
            FinishIntroWithFade();
            yield break;
        }

        isChangingPage = false;

        ShowCurrentPage();
    }

    private void FinishIntroWithFade()
    {
        if (isLoadingHub)
        {
            return;
        }

        StartCoroutine(FinishIntroWithFadeRoutine());
    }

    private IEnumerator FinishIntroWithFadeRoutine()
    {
        isLoadingHub = true;

        SaveIntroPlayed();

        yield return FadeScreenToBlack();

        LoadHubSceneNow();
    }

    private void LoadHubImmediately()
    {
        if (isLoadingHub)
        {
            return;
        }

        isLoadingHub = true;

        Time.timeScale = 1f;

        StopAllCoroutines();

        SaveIntroPlayed();

        LoadHubSceneNow();
    }

    private void LoadHubSceneNow()
    {
        Time.timeScale = 1f;

        Debug.Log("Intro finished. Loading HubScene by build index: " + HubSceneBuildIndex);

        SceneManager.LoadScene(HubSceneBuildIndex, LoadSceneMode.Single);
    }

    private void SaveIntroPlayed()
    {
        if (!saveIntroPlayed)
        {
            return;
        }

        PlayerPrefs.SetInt(introPlayedKey, 1);
        PlayerPrefs.Save();
    }

    private IEnumerator FadePageIn()
    {
        if (pageCanvasGroup == null)
        {
            yield break;
        }

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            pageCanvasGroup.alpha = Mathf.Lerp(
                0f,
                1f,
                timer / fadeDuration
            );

            yield return null;
        }

        pageCanvasGroup.alpha = 1f;
    }

    private IEnumerator FadePageOut()
    {
        if (pageCanvasGroup == null)
        {
            yield break;
        }

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            pageCanvasGroup.alpha = Mathf.Lerp(
                1f,
                0f,
                timer / fadeDuration
            );

            yield return null;
        }

        pageCanvasGroup.alpha = 0f;
    }

    private IEnumerator FadeScreenFromBlack()
    {
        if (fadeCanvasGroup == null)
        {
            yield break;
        }

        fadeCanvasGroup.blocksRaycasts = true;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            fadeCanvasGroup.alpha = Mathf.Lerp(
                1f,
                0f,
                timer / fadeDuration
            );

            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }

    private IEnumerator FadeScreenToBlack()
    {
        if (fadeCanvasGroup == null)
        {
            yield break;
        }

        fadeCanvasGroup.blocksRaycasts = true;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            fadeCanvasGroup.alpha = Mathf.Lerp(
                0f,
                1f,
                timer / fadeDuration
            );

            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
    }

    private IEnumerator WaitUnscaled(float duration)
    {
        if (duration <= 0f)
        {
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private void StopCurrentPageRoutines()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (pageRoutine != null)
        {
            StopCoroutine(pageRoutine);
            pageRoutine = null;
        }

        isTyping = false;
        isChangingPage = false;
    }
}
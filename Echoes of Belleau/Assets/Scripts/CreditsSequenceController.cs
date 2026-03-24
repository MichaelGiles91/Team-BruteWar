using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsSequenceController : MonoBehaviour
{
    [Header("--- Credits Scroll ---")]
    [SerializeField] RectTransform creditsText;
    [SerializeField] float scrollSpeed = 40f;
    [SerializeField] float endYPosition = 1600f;

    [Header("--- UI Text ---")]
    [SerializeField] TextMeshProUGUI thankYouText;
    [SerializeField] TextMeshProUGUI gameTitleText;
    [SerializeField] TextMeshProUGUI skipText;

    [Header("--- Thank You Timing ---")]
    [SerializeField] float thankYouFadeInTime = 2f;
    [SerializeField] float thankYouHoldTime = 3f;
    [SerializeField] float thankYouFadeOutTime = 2f;

    [Header("--- Game Title Timing ---")]
    [SerializeField] float typewriterSpeed = 0.08f;
    [SerializeField] float titleHoldTime = 2f;
    [SerializeField] float titleFadeOutTime = 2f;

    [Header("--- Skip UI ---")]
    [SerializeField] float skipPromptDelay = 3f;
    [SerializeField] bool allowEscapeSkip = true;

    [Header("--- Scene Loading ---")]
    [SerializeField] string menuSceneName = "MainMenu";

    [Header("--- Fade ---")]
    [SerializeField] ScreenFader screenFader;
    [SerializeField] float endBlackHoldTime = 0.5f;

    bool creditsFinished;
    bool sequenceStarted;
    bool inputEnabled;
    bool isSkipping;
    string fullGameTitle;

    void Start()
    {
        MusicManager.instance.PlayMusic(MusicType.Cutscene, 3, .75f);
        fullGameTitle = gameTitleText.text;
        gameTitleText.text = "";

        SetTextAlpha(thankYouText, 0f);
        SetTextAlpha(gameTitleText, 0f);
        SetTextAlpha(skipText, 0f);

        StartCoroutine(BeginScene());
    }
    void Update()
    {
        if (allowEscapeSkip && inputEnabled && !isSkipping && Input.GetButtonDown("Cancel"))
        {
            StartCoroutine(SkipToMenu());
            return;
        }

        if (inputEnabled && !creditsFinished && !isSkipping)
        {
            ScrollCredits();
        }
    }

    IEnumerator BeginScene()
    {
        inputEnabled = false;

        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeIn());

        inputEnabled = true;

        StartCoroutine(ShowSkipPromptLoop());
    }

    void ScrollCredits()
    {
        creditsText.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        if (creditsText.anchoredPosition.y >= endYPosition)
        {
            creditsFinished = true;

            if (!sequenceStarted)
            {
                sequenceStarted = true;
                StartCoroutine(EndSequence());
            }
        }
    }

    IEnumerator ShowSkipPromptLoop()
    {
        yield return new WaitForSeconds(skipPromptDelay);

        while (!sequenceStarted && !creditsFinished)
        {
            // Fade in
            yield return StartCoroutine(FadeText(skipText, 0f, 1f, 1f));

            // Stay visible
            yield return new WaitForSeconds(1.5f);

            // Fade out
            yield return StartCoroutine(FadeText(skipText, 1f, 0f, 1f));

            // Wait before showing again
            yield return new WaitForSeconds(4f);
        }

        SetTextAlpha(skipText, 0f);
    }

    IEnumerator EndSequence()
    {
        inputEnabled = false;

        // Hide skip prompt
        yield return StartCoroutine(FadeText(skipText, skipText.color.a, 0f, 0.5f));

        // Optional: hide scrolling credits once finished
        creditsText.gameObject.SetActive(false);

        // Thank you text
        yield return StartCoroutine(FadeText(thankYouText, 0f, 1f, thankYouFadeInTime));
        yield return new WaitForSeconds(thankYouHoldTime);
        yield return StartCoroutine(FadeText(thankYouText, 1f, 0f, thankYouFadeOutTime));

        yield return new WaitForSeconds(0.5f);

        // Game title typewriter
        gameTitleText.text = "";
        SetTextAlpha(gameTitleText, 1f);

        yield return StartCoroutine(TypeText(gameTitleText, fullGameTitle, typewriterSpeed));
        yield return new WaitForSeconds(titleHoldTime);
        yield return StartCoroutine(FadeText(gameTitleText, 1f, 0f, titleFadeOutTime));

        // Final fade to black
        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeOut());

        yield return new WaitForSeconds(endBlackHoldTime);

        SceneManager.LoadScene(menuSceneName);
    }

    IEnumerator SkipToMenu()
    {
        if (isSkipping) yield break;
        isSkipping = true;
        inputEnabled = false;

        SetTextAlpha(skipText, 0f);
        SetTextAlpha(thankYouText, 0f);
        SetTextAlpha(gameTitleText, 0f);

        if (creditsText != null)
            creditsText.gameObject.SetActive(false);

        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeOut());

        yield return new WaitForSeconds(endBlackHoldTime);

        SceneManager.LoadScene(menuSceneName);
    }

    IEnumerator FadeText(TextMeshProUGUI textObject, float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        Color color = textObject.color;
        color.a = startAlpha;
        textObject.color = color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            textObject.color = color;
            yield return null;
        }

        color.a = endAlpha;
        textObject.color = color;
    }

    IEnumerator TypeText(TextMeshProUGUI textObject, string fullText, float delayPerCharacter)
    {
        textObject.text = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            textObject.text += fullText[i];
            yield return new WaitForSeconds(delayPerCharacter);
        }
    }

    void SetTextAlpha(TextMeshProUGUI textObject, float alpha)
    {
        Color color = textObject.color;
        color.a = alpha;
        textObject.color = color;
    }
}
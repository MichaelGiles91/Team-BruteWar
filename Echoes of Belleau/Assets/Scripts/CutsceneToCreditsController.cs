using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CutsceneToCreditsController : MonoBehaviour
{
    [Header("--- References ---")]
    [SerializeField] PlayableDirector cutscene;
    [SerializeField] ScreenFader fader;

    [Header("--- Scene Flow ---")]
    [SerializeField] string creditsSceneName;
    [SerializeField] float fadeBeforeEnd;

    bool isSkipping = false;
    bool isLoadingScene = false;

    void Start()
    {
        StartCoroutine(PlaySequence());
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel") && !isSkipping && !isLoadingScene)
        {
            StartCoroutine(SkipCutscene());
        }
    }

    IEnumerator PlaySequence()
    {
        // Let the scene render black first
        yield return null;

        if (isSkipping || isLoadingScene)
            yield break;

        // Fade in from black
        if (fader != null)
            yield return StartCoroutine(fader.FadeIn());

        if (isSkipping || isLoadingScene)
            yield break;

        // Play cutscene
        if (cutscene != null)
            cutscene.Play();

        // Wait until it's time to fade before the cutscene ends
        if (cutscene != null)
        {
            float waitTime = Mathf.Max(0f, (float)cutscene.duration - fadeBeforeEnd);
            yield return new WaitForSeconds(waitTime);
        }

        if (isSkipping || isLoadingScene)
            yield break;

        // Fade to black while the cutscene is still finishing
        if (fader != null)
            yield return StartCoroutine(fader.FadeOut());

        if (isSkipping || isLoadingScene)
            yield break;

        isLoadingScene = true;

        // Load credits scene
        if (!string.IsNullOrEmpty(creditsSceneName))
            SceneManager.LoadScene(creditsSceneName);
    }

    IEnumerator SkipCutscene()
    {
        isSkipping = true;
        isLoadingScene = true;

        // Instantly force black so camera reset is never visible
        if (fader != null && fader.fadeImage != null)
        {
            Color c = fader.fadeImage.color;
            c.a = 1f;
            fader.fadeImage.color = c;
        }

        // Stop timeline after screen is already black
        if (cutscene != null)
            cutscene.Stop();

        // Wait one frame so black is shown before scene load
        yield return null;

        if (!string.IsNullOrEmpty(creditsSceneName))
            SceneManager.LoadScene(creditsSceneName);
    }
}
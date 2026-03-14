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
    void Start()
    {
        StartCoroutine(PlaySequence());
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel") && !isSkipping)
        {
            StartCoroutine(SkipCutscene());
        }
    }


    IEnumerator PlaySequence()
    {
        // Let the scene render black first
        yield return null;

        // Fade in from black
        if (fader != null)
            yield return StartCoroutine(fader.FadeIn());

        // Play cutscene
        if (cutscene != null)
            cutscene.Play();

        // Wait until it's time to fade before the cutscene ends
        float waitTime = Mathf.Max(0f, (float)cutscene.duration - fadeBeforeEnd);
        yield return new WaitForSeconds(waitTime);

        // Fade to black while the cutscene is still finishing
        if (fader != null)
            yield return StartCoroutine(fader.FadeOut());

        // Load credits scene
        if (!string.IsNullOrEmpty(creditsSceneName))
            SceneManager.LoadScene(creditsSceneName);
    }

    IEnumerator SkipCutscene()
    {
        isSkipping = true;

        // Stop timeline immediately
        if (cutscene != null)
            cutscene.Stop();

        // Fade out quickly
        if (fader != null)
            yield return StartCoroutine(fader.FadeOut());

        SceneManager.LoadScene(creditsSceneName);
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class BetweenLevelsCutsceneController : MonoBehaviour
{
    [Header("--- References ---")]
    [SerializeField] private PlayableDirector director;
    [SerializeField] private ScreenFader screenFader;

    [Header("--- Scene Flow ---")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool allowSkip = true;

    [Header("--- Skip Settings ---")]
    [SerializeField] private float skipLoadDelay = 0.1f;

    [Header("--- Blur Fade ---")]
    [SerializeField] private Volume blurVolume;
    [SerializeField] private float blurFadeDuration = 4f;

    private bool isSkipping;
    private bool isLoadingNextScene;

    private void Awake()
    {
        if (director == null)
            director = GetComponent<PlayableDirector>();
    }

    private void Start()
    {
        if (screenFader != null && screenFader.fadeImage != null)
        {
            Color c = screenFader.fadeImage.color;
            c.a = 1f;
            screenFader.fadeImage.color = c;
        }

        if (blurVolume != null)
            blurVolume.weight = 1f;

        if (playOnStart && director != null)
        {
            director.Play();
            StartCoroutine(WaitForCutsceneToEnd());
        }

        if (blurVolume != null)
            StartCoroutine(FadeBlurToNormal());
    }

    private void Update()
    {
        if (!allowSkip || isSkipping || isLoadingNextScene)
            return;

        if (Input.GetButtonDown("Cancel"))
            StartCoroutine(SkipCutscene());
    }

    public void FadeIn()
    {
        if (screenFader != null)
            screenFader.PlayFadeIn();
    }

    public void FadeOut()
    {
        if (screenFader != null)
            screenFader.PlayFadeOut();
    }

    private IEnumerator FadeBlurToNormal()
    {
        float timer = 0f;

        while (timer < blurFadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / blurFadeDuration;

            if (blurVolume != null)
                blurVolume.weight = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        if (blurVolume != null)
            blurVolume.weight = 0f;
    }

    private IEnumerator WaitForCutsceneToEnd()
    {
        while (director != null && director.state == PlayState.Playing)
            yield return null;

        if (isSkipping || isLoadingNextScene)
            yield break;

        isLoadingNextScene = true;
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator SkipCutscene()
    {
        isSkipping = true;
        isLoadingNextScene = true;

        if (screenFader != null && screenFader.fadeImage != null)
        {
            Color c = screenFader.fadeImage.color;
            c.a = 1f;
            screenFader.fadeImage.color = c;
        }

        if (director != null)
            director.Stop();

        yield return new WaitForSecondsRealtime(skipLoadDelay);

        SceneManager.LoadScene(nextSceneName);
    }
}
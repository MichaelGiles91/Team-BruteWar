using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroController : MonoBehaviour
{
    public PlayableDirector cutscene;
    public VideoPlayer introVideo;
    public GameObject introVideoImage;
    public ScreenFader fader;
    public string gameplayScene;

    bool isSkipping = false;
    bool isLoadingScene = false;

    IEnumerator Start()
    {
        yield return null;

        MusicManager.instance.PlayMusic(MusicType.Cutscene, 0, .75f);

        // Start by fading into the timeline cutscene
        yield return StartCoroutine(fader.FadeIn());

        if (!isSkipping && cutscene != null)
            cutscene.Play();

        if (cutscene != null)
            yield return new WaitForSeconds((float)cutscene.duration);

        if (isSkipping || isLoadingScene)
            yield break;

        // Fade to black before video
        yield return StartCoroutine(fader.FadeOut());

        if (isSkipping || isLoadingScene)
            yield break;

        // Show video layer
        if (introVideoImage != null)
            introVideoImage.SetActive(true);

        // Prepare video first
        if (introVideo != null)
        {
            introVideo.Prepare();
            yield return new WaitUntil(() => introVideo.isPrepared);
        }

        if (isSkipping || isLoadingScene)
            yield break;

        // Play video
        if (introVideo != null)
            introVideo.Play();

        // Wait one frame so first frame lands
        yield return null;

        if (isSkipping || isLoadingScene)
            yield break;

        // Reveal the video
        yield return StartCoroutine(fader.FadeIn());

        if (isSkipping || isLoadingScene)
            yield break;

        // Wait until video finishes
        while (introVideo != null && introVideo.isPlaying)
        {
            if (isSkipping || isLoadingScene)
                yield break;

            yield return null;
        }

        if (isSkipping || isLoadingScene)
            yield break;

        // Fade out before gameplay
        yield return StartCoroutine(fader.FadeOut());

        if (isLoadingScene)
            yield break;

        isLoadingScene = true;
        SceneManager.LoadScene(gameplayScene);
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel") && !isSkipping && !isLoadingScene)
        {
            StartCoroutine(SkipCutscene());
        }
    }

    IEnumerator SkipCutscene()
    {
        isSkipping = true;
        isLoadingScene = true;

        // Instantly force black so no camera snap or video/frame flash is visible
        if (fader != null && fader.fadeImage != null)
        {
            Color c = fader.fadeImage.color;
            c.a = 1f;
            fader.fadeImage.color = c;
        }

        // Stop timeline
        if (cutscene != null)
            cutscene.Stop();

        // Stop video if it is already active
        if (introVideo != null)
            introVideo.Stop();

        // Optional: hide video layer
        if (introVideoImage != null)
            introVideoImage.SetActive(false);

        // Wait one real frame so black is definitely shown before scene load
        yield return null;

        SceneManager.LoadScene(gameplayScene);
    }
}
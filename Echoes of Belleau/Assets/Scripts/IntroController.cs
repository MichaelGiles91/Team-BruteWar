using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    public PlayableDirector cutscene;
    public ScreenFader fader;
    public string gameplayScene;

    bool isSkipping = false;
    bool isLoadingScene = false;

    IEnumerator Start()
    {
        yield return null;

        MusicManager.instance.PlayMusic(MusicType.Cutscene, 0, .75f);

        // Fade in to cutscene
        yield return StartCoroutine(fader.FadeIn());

        if (!isSkipping && cutscene != null)
            cutscene.Play();

        if (cutscene != null)
            yield return new WaitForSeconds((float)cutscene.duration);

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

        // Force instant black to avoid visual pop
        if (fader != null && fader.fadeImage != null)
        {
            Color c = fader.fadeImage.color;
            c.a = 1f;
            fader.fadeImage.color = c;
        }

        // Stop timeline
        if (cutscene != null)
            cutscene.Stop();

        yield return null;

        SceneManager.LoadScene(gameplayScene);
    }
}
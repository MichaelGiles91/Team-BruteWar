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

    IEnumerator Start()
    {
        yield return null;

        // Fade into cutscene
        yield return StartCoroutine(fader.FadeIn());

        cutscene.Play();
        yield return new WaitForSeconds((float)cutscene.duration);

        // Fade to black
        yield return StartCoroutine(fader.FadeOut());

        // Show video layer
        introVideoImage.SetActive(true);

        // Prepare video first
        introVideo.Prepare();
        yield return new WaitUntil(() => introVideo.isPrepared);

        // Play video
        introVideo.Play();

        // Wait one frame so the first video frame can land on the texture
        yield return null;

        // Reveal the video
        yield return StartCoroutine(fader.FadeIn());

        // Wait until it finishes
        while (introVideo.isPlaying)
            yield return null;

        // Fade out before gameplay
        yield return StartCoroutine(fader.FadeOut());

        SceneManager.LoadScene(gameplayScene);
    }
}
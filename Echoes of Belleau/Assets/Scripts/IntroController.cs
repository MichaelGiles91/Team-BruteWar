using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroController : MonoBehaviour
{
    public PlayableDirector cutscene;
    public VideoPlayer introVideo;
    public ScreenFader fader;

    public string gameplayScene;

    IEnumerator Start()
    {
        // Fade from black
        yield return StartCoroutine(fader.FadeIn());

        // Play hospital cutscene
        cutscene.Play();
        yield return new WaitForSeconds((float)cutscene.duration);

        // Fade to black
        yield return StartCoroutine(fader.FadeOut());

        yield return new WaitForSeconds(0.5f);

        // Play intro title video
        introVideo.Play();

        // Wait for video to finish
        yield return new WaitUntil(() => !introVideo.isPlaying);

        // Load gameplay scene
        SceneManager.LoadScene(gameplayScene);
    }
}
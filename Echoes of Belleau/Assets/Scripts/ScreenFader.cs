using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public Image fadeImage;
    public float fadeSpeed = 1f;

    public IEnumerator FadeOut()
    {
        float t = 0;
        Color c = fadeImage.color;

        while (t < 1)
        {
            t += Time.deltaTime * fadeSpeed;
            c.a = Mathf.Lerp(0, 1, t);
            fadeImage.color = c;
            yield return null;
        }
    }

    public IEnumerator FadeIn()
    {
        float t = 0;
        Color c = fadeImage.color;

        while (t < 1)
        {
            t += Time.deltaTime * fadeSpeed;
            c.a = Mathf.Lerp(1, 0, t);
            fadeImage.color = c;
            yield return null;
        }
    }

    public void PlayFadeOut()
    {
        StartCoroutine(FadeOut());
    }

    public void PlayFadeIn()
    {
        StartCoroutine(FadeIn());
    }
}

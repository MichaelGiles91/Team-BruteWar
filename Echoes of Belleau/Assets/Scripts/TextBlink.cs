using UnityEngine;
using TMPro;

public class TextBlink : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] float blinkSpeed;

    float alpha = 1f;
    bool fadingOut = true;

    void Update()
    {
        Color c = text.color;

        if (fadingOut)
        {
            alpha -= Time.deltaTime / blinkSpeed;

            if (alpha <= 0f)
            {
                alpha = 0f;
                fadingOut = false;
            }
        }
        else
        {
            alpha += Time.deltaTime / blinkSpeed;

            if (alpha >= 1f)
            {
                alpha = 1f;
                fadingOut = true;
            }
        }

        c.a = alpha;
        text.color = c;
    }
}
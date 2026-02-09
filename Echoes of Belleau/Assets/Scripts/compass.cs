using UnityEngine;
using UnityEngine.UI;

public class compass : MonoBehaviour
{

    public RawImage compassImage;
    public Transform player;


  

    // Update is called once per frame
    void Update()
    {
        compassImage.uvRect = new Rect(player.localEulerAngles.y / 360f, 0f, 1f, 1f);
    }
}

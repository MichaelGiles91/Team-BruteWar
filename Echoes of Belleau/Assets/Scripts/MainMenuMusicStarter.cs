using UnityEngine;

public class MainMenuMusicStarter : MonoBehaviour
{
    void Start()
    {
        MusicManager.instance.PlayMusic(MusicType.Menu, 0);
    }
}
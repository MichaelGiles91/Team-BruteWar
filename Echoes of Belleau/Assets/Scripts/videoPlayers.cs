using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class videoPlayers : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] string nextScene;
    
    void Start()
    {
        videoPlayer.loopPointReached += EndReached;      
    }

   void EndReached(VideoPlayer VP)
    {
        SceneManager.LoadScene(nextScene);
    }
}

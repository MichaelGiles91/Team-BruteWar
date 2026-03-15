using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunctions : MonoBehaviour
{
   
    public void playGame()
    {
        SceneManager.LoadScene("Intro Scene");
    }
    public void resume()
    {
        MusicManager.instance.PlayMusic(MusicType.Calm, 0, 1f);
        gameManager.instance.stateUnpause();
    }

    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gameManager.instance.stateUnpause();
    }

    public void quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

#else 
    Application.Quit();
#endif
    }
    public void respawn()
    {
        gameManager.instance.Respawn();
    }
}

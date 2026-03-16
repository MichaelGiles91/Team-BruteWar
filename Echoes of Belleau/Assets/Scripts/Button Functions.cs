using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunctions : MonoBehaviour
{
   
    public void playGame()
    {
        MainMenuController.instance.StartGame();
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

    public void respawn()
    {
        MusicManager.instance.PlayMusic(MusicType.Calm, 0, .5f);
        gameManager.instance.Respawn();
    }

    public void settings()
    {
        gameManager.instance.settingsMenu();
    }

    public void back()
    {
        gameManager.instance.Back();
    }

    public void settingsMainMenu()
    {
        MainMenuSettings.instance.settingsMenu();
    }

    public void backMainMenu()
    {
        MainMenuSettings.instance.Back();
    }

    public void credits()
    {
        SceneManager.LoadScene("Credits");
    }

    public void quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

#else 
    Application.Quit();
#endif
    }
}

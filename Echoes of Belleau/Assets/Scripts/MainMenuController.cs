using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public static MainMenuController instance;
    [SerializeField] ScreenFader screenFader;


    private void Awake()
    {
        if (Cursor.visible == false)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        instance = this;
    }
    void Start()
    {
        MusicManager.instance.PlayMusic(MusicType.Menu, 0);
        StartCoroutine(StartMenu());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            StartCoroutine(StartGameRoutine());
            SceneManager.LoadScene("Showcase");
        }
    }


    IEnumerator StartMenu()
    {

        yield return StartCoroutine(screenFader.FadeIn());
    }

    public void StartGame()
    {
        StartCoroutine(StartGameRoutine());
        SceneManager.LoadScene("Intro Scene");
    }

    IEnumerator StartGameRoutine()
    {
        yield return StartCoroutine(screenFader.FadeOut());
        
    }


}
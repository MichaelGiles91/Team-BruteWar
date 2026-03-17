using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class menuManager : MonoBehaviour
{
    [Header("Menu Objects")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject settingsMenu;

    [Header("First Selected Options")]
    [SerializeField] private GameObject mainMenuFirst;
    [SerializeField] private GameObject settingsMenuFirst;

    private bool isPaused;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        if (InputManager.instance.MenuOpenCloseInput)
        {
            if (!isPaused)
            {
                Pause();
            }
            else
            {
                Unpause();
            }
        }
    }

    #region Pause/Unpause Functions

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        OpenMainMenu();
    }

    public void Unpause()
    {
        isPaused = false;
        Time.timeScale = 1f;

        CloseAllMenus();
    }

    #endregion

    #region Canvas Activations

    public void OpenMainMenu()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);

        EventSystem.current.SetSelectedGameObject(mainMenuFirst);
    }
    public void OpenSettingsMenu()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);

        EventSystem.current.SetSelectedGameObject(settingsMenuFirst);
    }

    public void CloseAllMenus()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);
    }

    #endregion
}

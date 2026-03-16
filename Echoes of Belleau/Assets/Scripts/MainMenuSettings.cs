using UnityEngine;

public class MainMenuSettings : MonoBehaviour
{
    public static MainMenuSettings instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuMain;
    [SerializeField] GameObject menuSettings;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        menuActive = menuMain;
        menuSettings.SetActive(false);
    }

    public void settingsMenu()
    {
        menuSettings.SetActive(true);
        menuMain.SetActive(false);
        menuActive = menuSettings;
        if (Input.GetButtonDown("Cancel"))
        {
            menuSettings.SetActive(false);
            menuMain.SetActive(true);
            menuActive = menuMain;
        }
    }

    internal void Back()
    {
        menuMain.SetActive(true);
        menuSettings.SetActive(false);
        menuActive = menuMain;
    }
}

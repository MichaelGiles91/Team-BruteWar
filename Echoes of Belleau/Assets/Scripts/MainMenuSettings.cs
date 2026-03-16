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
        
    }

    public void settingsMenu()
    {
        menuSettings.SetActive(true);
        menuActive = menuSettings;
        if (Input.GetButtonDown("Cancel"))
        {
            menuSettings.SetActive(false);
            menuActive = menuMain;
        }
    }

    internal void Back()
    {
        menuSettings.SetActive(false);
        menuActive = menuMain;
    }
}

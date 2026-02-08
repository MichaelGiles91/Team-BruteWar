using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [SerializeField] GameObject menuActive; // if a settings menu GameObject[] is an array for multiple
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] TMP_Text gameGoalCountText;
    public GameObject playerDamageFlash;
    public Image playerHPBar;

    public GameObject player;
    public playerController playerScript;
    public bool isPaused;

    float timeScaleOrig;

    int gameGoalCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake() //Begins before Start
    {
        instance = this;
        timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player"); // Use to find player by tag
        playerScript = player.GetComponent<playerController>(); // Use to get the playerController script from the player GameObject
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
            else if (menuActive == menuPause)
            {
                stateUnpause();
            }
        }
    } 

    public void statePause()
    {
        isPaused = true;
        Time.timeScale = 0; // Set the time scale to 0 to pause the game
        Cursor.visible = true; // Make the cursor visible when the game is paused
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor when the game is paused
    }

    public void stateUnpause()
    {
        isPaused = false;
        Time.timeScale = timeScaleOrig; // Reset the time scale to its original value to unpause the game
        Cursor.visible = false; // Hide the cursor when the game is unpaused
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor when the game is unpaused
        menuActive.SetActive(false); // Deactivate the active menu
        menuActive = null; // Set the active menu to null
    }

    public void UpdateGameGoal(int amount)
    {
        gameGoalCount += amount; // Update the game goal count by adding the specified amount
        gameGoalCountText.text = gameGoalCount.ToString("F0"); // Update the game goal count text to display the current count as an integer

        if (gameGoalCount <= 0)
        {
            // you win
            statePause();
            menuActive = menuWin;
            menuActive.SetActive(true);
        }
    }

    public void youLose()
    {
        statePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }
}


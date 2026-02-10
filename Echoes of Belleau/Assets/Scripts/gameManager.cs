
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;


    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject checkpointNotification;
    [SerializeField] TMP_Text gameGoalCountText;
    
    public Image playerHPBar;
    public GameObject playerDamageFlash;
    public Image playerStaminaBar;
    public GameObject map;

    public GameObject player;
    public PlayerController playerScript;
    
    public bool isPaused;

    float timeScaleOrig;

    int gameGoalCount;


    Vector3 checkpointPos;
    Quaternion checkpointRot;
    bool hasCheckpoint;

    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {

            if(menuActive == null)
    
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);

            }else if(menuActive == menuPause)
            {
                stateUnpause();
            }
        }

        if (Input.GetButtonDown("Map"))
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = map;
                menuActive.SetActive(true);
            }
            else if (menuActive == map)
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

    public void updateGameGoal(int amount)
    {
        gameGoalCount += amount;
        gameGoalCountText.text = gameGoalCount.ToString("F0");
        if(gameGoalCount <= 0)
        {
            statePause();
            menuActive = menuWin;
            menuActive.SetActive(true);

       
    }

    public void UpdateGameGoal(int amount)
    {
        gameGoalCount += amount; // Update the game goal count by adding the specified amount
        gameGoalCountText.text = gameGoalCount.ToString("F0"); // Update the game goal count text to display the current count as an integer

        if (gameGoalCount <= 0)
        {
           
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
    public void SetCheckpoint(Transform t)
    {
        checkpointPos = t.position;
        checkpointRot = t.rotation;
        hasCheckpoint = true;
        StartCoroutine(showCheckpointNotification());
    }
    public void Respawn()
    {
       
        if (menuLose != null) menuLose.SetActive(false);

        
        stateUnpause();

        
        if (!hasCheckpoint)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return; 
           
        }

        
        CharacterController cc = player.GetComponent<CharacterController>();
        cc.enabled = false;
        player.transform.SetPositionAndRotation(checkpointPos, checkpointRot);
        cc.enabled = true;

        
        playerScript.RespawnReset();
    }
    IEnumerator showCheckpointNotification()
    {
       checkpointNotification.SetActive(true);
        yield return new WaitForSeconds(10f);
        checkpointNotification.SetActive(false);

    }
}


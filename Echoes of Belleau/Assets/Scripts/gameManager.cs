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
    [SerializeField] TMP_Text gameGoalCountText;
    [SerializeField] TMP_Text checkpointNotificationText;
    public Image playerHPBar;
    public GameObject PlayerDamageFlash;

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
    }
    public void statePause()
    {
        isPaused = true;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void stateUnpause()
    {
        isPaused = false;
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;

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
        // hide lose UI
        if (menuLose != null) menuLose.SetActive(false);

        // unpause the game
        stateUnpause();

        // if no checkpoint yet, fall back to restart for now
        if (!hasCheckpoint)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return; // or SceneManager.LoadScene(...) if that's what you do
           
        }

        // move player
        player.transform.SetPositionAndRotation(checkpointPos, checkpointRot);

        // reset player state
        playerScript.RespawnReset();
    }
    IEnumerator showCheckpointNotification()
    {
        checkpointNotificationText.gameObject.SetActive(true);
        checkpointNotificationText.text = "CheckPoint Reached!";
        yield return new WaitForSeconds(2f);
        checkpointNotificationText.gameObject.SetActive(false);

    }
}

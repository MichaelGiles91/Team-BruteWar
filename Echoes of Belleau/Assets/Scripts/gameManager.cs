using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [Header ("---Game Screens---")]
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    public bool isPaused;
    [Header("---UI Elements---")]
    [SerializeField] GameObject checkpointNotification;
    [SerializeField] TMP_Text gameGoalCountText;
    public PlayerController ammoAmount;
    [SerializeField] TMP_Text ammoAmountText;
    public Image playerHPBar;
    public GameObject playerDamageFlash;
    public Image playerStaminaBar;
    [SerializeField] GameObject map;
    [SerializeField] FullscreenMapUI mapUI;
    [Header("---Compass Items---")]
    [SerializeField] RawImage compassImage;
    [SerializeField] GameObject iconPrefab;
    [SerializeField] bool autoActivateFirstMarker;
    [Header("---Objective Items---")]
    [SerializeField] GameObject objEnemyCounter;
    [SerializeField] TMP_Text objEnemyText;
    [SerializeField] GameObject objective;
    [SerializeField] TMP_Text objectiveHeaderText;
    [SerializeField] TMP_Text objectiveText;
    [SerializeField] float objectiveHideDelay = 3f;

    Coroutine hideObjectiveRoutine;

    public GameObject player;
    public PlayerController playerScript;
    
    float timeScaleOrig;

    int objEnemy;
    int gameGoalCount;


    List<ObjMarker> objMarkers = new List<ObjMarker>();

    float compassUnit;

    int currentObjectiveIndex = 0;
    bool hasActivatedFirstMarker = false;


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
        ammoAmount = player.GetComponent<PlayerController>();
        compassUnit = compassImage.rectTransform.rect.width / 360f;
        //mapUI.SetObjective(currentObjectiveTransform);

        ObjMarker[] markers = GameObject.FindObjectsByType<ObjMarker>(FindObjectsInactive.Include,FindObjectsSortMode.None);

        foreach (var m in markers)
            RegisterObjectiveMarker(m);

        RefreshMapObjective();
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
                objective.SetActive(true);

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
        objective.SetActive(false);
        menuActive = null; // Set the active menu to null

    }

    public void updateGameGoal(int amount)
    {
        gameGoalCount += amount;
        gameGoalCountText.text = gameGoalCount.ToString("F0");

        if (gameGoalCount <= 0)
        {
            //you won!!
            statePause();
            menuActive = menuWin;
            menuActive.SetActive(true);
        }
    }

    public void updateObjEnemyCounter(int amount)
    {
        objEnemyCounter.SetActive(true);
        objEnemy = amount;
        objEnemyText.text = objEnemy.ToString();
        if(amount == 0)
        {
            objEnemyCounter.SetActive(false);
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

    public void updateAmmoAmount(int currentAmmo)
    {
        ammoAmountText.text = currentAmmo.ToString();
    }

    public void updateCompass(float yRotation)
    {
        compassImage.uvRect = new Rect(yRotation / 360f, 0f, 1f, 1f);

        float halfWidth = compassImage.rectTransform.rect.width * 0.5f;

        foreach (ObjMarker marker in objMarkers)
        {
            if (marker == null || !marker.isActive) continue;
            if (marker.image == null) continue;

            Vector2 pos = GetPosOnCompass(marker);

            float iconHalf = marker.image.rectTransform.rect.width * 0.5f;
            pos.x = Mathf.Clamp(pos.x, -halfWidth + iconHalf, halfWidth - iconHalf);

            marker.image.rectTransform.anchoredPosition = pos;
        }
    }

    public void addObjMarker (ObjMarker marker)
    {
        GameObject newMarker = Instantiate(iconPrefab, compassImage.transform);
        marker.image = newMarker.GetComponent<Image>();
        marker.image.sprite = marker.icon;

        marker.image.gameObject.SetActive(false);

    }

    Vector2 GetPosOnCompass (ObjMarker marker)
    {
        Vector2 playerPos = new Vector2(player.transform.position.x, player.transform.position.z);
        Vector2 playerFwd = new Vector2(player.transform.forward.x, player.transform.forward.z);

        float angle = Vector2.SignedAngle(marker.position - playerPos, playerFwd);

        return new Vector2(compassUnit * angle , 0f);
    }

    public void RegisterObjectiveMarker(ObjMarker marker)
    {
        if (marker == null) return;

        if (!objMarkers.Contains(marker))
        {
            objMarkers.Add(marker);
            addObjMarker(marker);
        }

        objMarkers.Sort((a, b) => a.objectiveOrder.CompareTo(b.objectiveOrder));

        marker.SetActive(false);

        if (autoActivateFirstMarker && !hasActivatedFirstMarker && objMarkers.Count > 0)
        {
            currentObjectiveIndex = 0;
            objMarkers[0].SetActive(true);
            RefreshMapObjective();
            hasActivatedFirstMarker = true;
        }
    }

    public void UnregisterObjectiveMarker(ObjMarker marker)
    {
        if (marker == null) return;

        marker.SetActive(false);

        objMarkers.Remove(marker);

    }

    public void CompleteCurrentObjectiveAndAdvance()
    {

        if (objMarkers.Count > 0 &&
            currentObjectiveIndex >= 0 &&
            currentObjectiveIndex < objMarkers.Count)
        {
            objMarkers[currentObjectiveIndex].SetActive(false);
        }

        currentObjectiveIndex++;

        if (objMarkers.Count > 0 && currentObjectiveIndex < objMarkers.Count)
        {
            objMarkers[currentObjectiveIndex].SetActive(true);
        }
        else
        {
            RefreshMapObjective();
        }
    }

    public void updateObjectiveText(string text, string headerText)
    {
        objective.SetActive(true);

        if(objectiveHeaderText != null)
        {
            objectiveHeaderText.text = headerText;
        }

        if (objectiveText != null)
        {
            objectiveText.text = text;
        }

        if (hideObjectiveRoutine != null)
            StopCoroutine(hideObjectiveRoutine);

        hideObjectiveRoutine = StartCoroutine(HideObjectiveAfterDelay());

    }

    IEnumerator HideObjectiveAfterDelay()
    {
        yield return new WaitForSeconds(objectiveHideDelay);
        objective.SetActive(false);
        hideObjectiveRoutine = null;
    }

    public void RefreshMapObjective()
    {
        if (mapUI == null) return;

        if (objMarkers.Count > 0 && currentObjectiveIndex >= 0 && currentObjectiveIndex < objMarkers.Count)
        {
            mapUI.SetObjectivePin(objMarkers[currentObjectiveIndex].transform);
        }
        else
        {
            mapUI.SetObjectivePin(null);
        }
    }

    public void SetActiveObjectiveZone(Collider zone)
    {
        if (mapUI != null)
            mapUI.SetActiveZone(zone);
    }
}


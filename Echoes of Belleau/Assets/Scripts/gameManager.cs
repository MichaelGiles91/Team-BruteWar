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

    [Header("---Game Screens---")]
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    public bool isPaused;
    [Header("---UI Elements---")]
    [SerializeField] GameObject checkpointNotification;
    
    public Image playerHPBar;
    public GameObject playerDamageFlash;
    public Image playerStaminaBar;
    [SerializeField] GameObject map;
    [SerializeField] FullscreenMapUI mapUI;
    [SerializeField] GameObject mapStuff;
    
    [Header("---Weapon/Ammo---")]
    public PlayerController ammoAmount;
    [SerializeField] TMP_Text ammoAmountText;
    [SerializeField] TMP_Text ammoMaxText;
    [SerializeField] Image currentWeaponIcon;
    [Header("---Compass Items---")]
    [SerializeField] RawImage compassImage;
    [SerializeField] GameObject iconPrefab;
    [SerializeField] bool autoActivateFirstMarker;
    [Header("---Objective Items---")]
    [SerializeField] GameObject objEnemyCounter;
    [SerializeField] TMP_Text objEnemyText;
    [SerializeField] TMP_Text medkitAmountText;
    [SerializeField] GameObject objective;
    [SerializeField] TMP_Text objectiveHeaderText;
    [SerializeField] TMP_Text objectiveText;
    [SerializeField] float objectiveHideDelay = 3f;

    Coroutine hideObjectiveRoutine;

    public GameObject player;

    public PlayerController playerScript;

    float timeScaleOrig;

    int objEnemy;

    List<ObjMarker> objMarkers = new List<ObjMarker>();

    float compassUnit;

    int currentObjectiveIndex = 0;
    bool hasActivatedFirstMarker = false;

    public PlayerController medkitAmount;

    bool fogOrig;

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
        medkitAmount = player.GetComponent<PlayerController>();
        mapStuff = GameObject.FindWithTag("Map Stuff");
        mapStuff.SetActive(false);
        compassUnit = compassImage.rectTransform.rect.width / 360f;

        ObjMarker[] markers = GameObject.FindObjectsByType<ObjMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var m in markers)
            RegisterObjectiveMarker(m);

        if (objMarkers.Count > 0)
        {
            currentObjectiveIndex = Mathf.Clamp(currentObjectiveIndex, 0, objMarkers.Count - 1);
            objMarkers[currentObjectiveIndex].SetActive(true);
        }

        RefreshMapObjective();
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
                objective.SetActive(true);

            }
            else if (menuActive == menuPause)
            {
                stateUnpause();
            }
        }

        if (Input.GetButtonDown("Map"))
        {
            if (menuActive == null)
            {
                fogOrig = RenderSettings.fog;
                RenderSettings.fog = false;

                statePause();
                mapStuff.SetActive(true);
                menuActive = map;
                menuActive.SetActive(true);
            }
            else if (menuActive == map)
            {
                RenderSettings.fog = fogOrig;

                mapStuff.SetActive(false);
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

    public void youWin()
    {
            //you won!!
            statePause();
            menuActive = menuWin;
            menuActive.SetActive(true);
    }

    public void updateObjEnemyCounter(int amount)
    {
        objEnemyCounter.SetActive(true);
        objEnemy = amount;
        objEnemyText.text = objEnemy.ToString();
        if (amount == 0)
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

    public void updateAmmoAmount(int currentAmmo, int maxAmmo)
    {
        ammoAmountText.text = currentAmmo.ToString();
        ammoMaxText.text = maxAmmo.ToString();
    }

    public void updateMedkitAmount(int currentMedkit)
    {
        medkitAmountText.text = currentMedkit.ToString();
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

    public void addObjMarker(ObjMarker marker)
    {
        GameObject newMarker = Instantiate(iconPrefab, compassImage.transform);
        marker.image = newMarker.GetComponent<Image>();
        marker.image.sprite = marker.icon;

        marker.image.gameObject.SetActive(false);

    }

    Vector2 GetPosOnCompass(ObjMarker marker)
    {
        Vector2 playerPos = new Vector2(player.transform.position.x, player.transform.position.z);
        Vector2 playerFwd = new Vector2(player.transform.forward.x, player.transform.forward.z);

        float angle = Vector2.SignedAngle(marker.position - playerPos, playerFwd);

        return new Vector2(compassUnit * angle, 0f);
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

        for (int i = 0; i < objMarkers.Count; i++)
            objMarkers[i].SetActive(i == currentObjectiveIndex);

        RefreshMapObjective();

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

        RefreshMapObjective();
    }

    public void updateObjectiveText(string text, string headerText)
    {
        objective.SetActive(true);

        if (objectiveHeaderText != null)
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

    public void UpdateWeaponIcon(Sprite icon)
    {
        if (currentWeaponIcon == null) return;

        currentWeaponIcon.enabled = (icon != null);
        currentWeaponIcon.sprite = icon;
    }
}
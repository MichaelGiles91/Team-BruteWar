using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

[System.Serializable]
public class PlayerCheckpointData
{
    public Vector3 position;
    public Quaternion rotation;
    public int hp;
    public int medkits;
    public int grenades;
    public float devilDogPoints;
}

[System.Serializable]
public class WeaponCheckpointData
{
    public gunStats gunRef;
    public int ammoCur;
    public int ammoMax;
}

[System.Serializable]
public class PickupCheckpointData
{
    public CheckpointPickup pickup;
    public bool isActive;
}

[System.Serializable]
public class EnemyCheckpointData
{
    public SingleSoldierSpawner spawnPoint;
    public Vector3 position;
    public Quaternion rotation;
    public float health;
    public bool isAlive;
}

[System.Serializable]
public class AreaObjectiveCheckpointData
{
    public AreaObjective objective;
    public bool wasActive;
    public bool wasComplete;
}

[System.Serializable]
public class ObjectiveMarkerCheckpointData
{
    public ObjMarker marker;
    public bool wasActive;
}

[System.Serializable]
public class StreetCutsceneCheckpointData
{
    public StreetCutsceneManager cutsceneManager;
    public StreetCutsceneTrigger trigger;
    public bool cutscenePlayed;
    public bool triggerUsed;
    public Vector3 tankPosition;
    public Quaternion tankRotation;
    public bool houseStreetActive;
    public bool houseCornerActive;
    public bool rubbleActive;
    public bool destroyedHouseStreetActive;
    public bool destroyedHouseCornerActive;
}

[System.Serializable]
public class BossCheckpointData
{
    public BossCutsceneManager cutsceneManager;
    public BossCutsceneTrigger trigger;
    public Transform tank;
    public Vector3 tankPosition;
    public Quaternion tankRotation;
    public bool cutscenePlayed;
    public bool triggerUsed;
}

[System.Serializable]
public class SearchObjectiveCheckpointData
{
    public SearchObjective objective;
    public bool wasActive;
    public bool wasComplete;
}

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [Header("---Game Screens---")]
    public GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuSettings;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    public bool isPaused;
    [Header("---UI Elements---")]
    [SerializeField] GameObject checkpointNotification;
    
    public Image playerHPBar;
    public GameObject playerDamageFlash;
    public Image lowHealthIndicator;
    public GameObject emptyAmmoClipIndicator;
    public Image playerStaminaBar;
    public Image devilDogBarFill;
    [SerializeField] GameObject map;
    [SerializeField] FullscreenMapUI mapUI;
    [SerializeField] GameObject mapStuff;
    public ScreenFader fader;

    [Header("---Crosshair---")]
    [SerializeField] RectTransform crosshairTop;
    [SerializeField] RectTransform crosshairBottom;
    [SerializeField] RectTransform crosshairLeft;
    [SerializeField] RectTransform crosshairRight;
    [SerializeField] float baseCrosshairGap = 12f;

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
    [SerializeField] TMP_Text grenadeAmountText;
    [SerializeField] TMP_Text medkitAmountText;
    [SerializeField] GameObject objective;
    [SerializeField] TMP_Text objectiveHeaderText;
    [SerializeField] TMP_Text objectiveText;
    [SerializeField] float objectiveHideDelay = 3f;
    [Header("--- Tutorial Popup ---")]
    [SerializeField] GameObject tutorialPopup;
    [SerializeField] TMP_Text tutorialHeaderText;
    [SerializeField] TMP_Text tutorialBodyText;
    [Header("--- Street Cutscene References ---")]
    [SerializeField] StreetCutsceneManager streetCutsceneManager;
    [SerializeField] StreetCutsceneTrigger streetCutsceneTrigger;
    [SerializeField] Transform streetTank;

    [Header("--- Boss Checkpoint References ---")]
    [SerializeField] BossCutsceneManager bossCutsceneManager;
    [SerializeField] BossCutsceneTrigger bossCutsceneTrigger;
    [SerializeField] Transform bossTank;

    [SerializeField] GameObject lockedText;

    Coroutine hideObjectiveRoutine;

    public GameObject player;

    public PlayerController playerScript;

    float timeScaleOrig;

    int objEnemy;

    List<ObjMarker> objMarkers = new List<ObjMarker>();

    float compassUnit;

    int currentObjectiveIndex = 0;
    bool hasActivatedFirstMarker = false;

    public PlayerController grenadeAmount;

    public PlayerController medkitAmount;

    bool fogOrig;

    bool hasCheckpoint;

    string sceneName;

    bool playerInCombat;
    float combatTimer;
    [SerializeField] float combatExitDelay = 6f;

    PlayerCheckpointData playerCheckpointData = new PlayerCheckpointData();
    List<EnemyCheckpointData> enemyCheckpointData = new List<EnemyCheckpointData>();
    List<WeaponCheckpointData> checkpointWeapons = new List<WeaponCheckpointData>();
    List<PickupCheckpointData> pickupCheckpointData = new List<PickupCheckpointData>();
    List<AreaObjectiveCheckpointData> objectiveCheckpointData = new List<AreaObjectiveCheckpointData>();
    List<ObjectiveMarkerCheckpointData> markerCheckpointData = new List<ObjectiveMarkerCheckpointData>();
    List<SearchObjectiveCheckpointData> searchObjectiveCheckpointData = new List<SearchObjectiveCheckpointData>();
    StreetCutsceneCheckpointData streetCutsceneCheckpointData = new StreetCutsceneCheckpointData();
    BossCheckpointData bossCheckpointData = new BossCheckpointData();
    int checkpointSelectedGunIndex = 0;

    private void Awake()
    {
        instance = this;

        if (tutorialPopup != null)
            tutorialPopup.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        timeScaleOrig = Time.timeScale;

        if (sceneName == "Scene1")
        {
            MusicManager.instance.SetLevelMusic(MusicType.Calm, 0, MusicType.Combat, 0);
            MusicManager.instance.PlayBaseMusic();
        }

        if (sceneName == "Level 2")
        {
            MusicManager.instance.SetLevelMusic(MusicType.Calm, 1, MusicType.Combat, 1);
            MusicManager.instance.PlayBaseMusic();
        }

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();
        ammoAmount = player.GetComponent<PlayerController>();
        grenadeAmount = player.GetComponent<PlayerController>();
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

        if (fader != null)
            StartCoroutine(FadeInScene());
    }

    // Update is called once per frame
    void Update()
    {

        if (!isPaused)
            UpdateCombatState();
        if(Input.GetButtonDown("Debug Tut"))
        {
            ResetTutorial("MedKitPickup");
            ResetTutorial("AmmoPickup");
            ResetTutorial("StressSystem");
            ResetTutorial("DevilDog");
            ResetTutorial("Keybinds");
        }

        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                MusicManager.instance.PlayMusic(MusicType.Menu, 1, .5f);

                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);
                objective.SetActive(true);
            }
            else if (menuActive == menuSettings)
            {
                menuActive = menuPause;
                menuSettings.SetActive(false);
                menuPause.SetActive(true);
            }
            else
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
        playerScript.canShoot = false;
        playerScript.canJump = false;
        playerScript.canMelee = false;

        Time.timeScale = 0; // Set the time scale to 0 to pause the game
        Cursor.visible = true; // Make the cursor visible when the game is paused
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor when the game is paused
    }

    public void stateUnpause()
    {
        isPaused = false;
        playerScript.canShoot = true;
        playerScript.canJump = true;
        playerScript.canMelee = true;
        Time.timeScale = timeScaleOrig; // Reset the time scale to its original value to unpause the game
        Cursor.visible = false; // Hide the cursor when the game is unpaused
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor when the game is unpaused
        menuActive.SetActive(false); // Deactivate the active menu
        objective.SetActive(false);
        menuActive = null; // Set the active menu to null
        SyncMusicToCombatState();

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
    public void SetCheckpoint()
    {
        hasCheckpoint = true;

        SavePlayerCheckpointData();
        SaveEnemyCheckpointData();
        SavePickupCheckpointData();
        SaveObjectiveCheckpointData();
        SaveObjectiveMarkerCheckpointData();

        SaveSearchObjectiveCheckpointData();
        SaveStreetCutsceneCheckpointData();
        SaveBossCheckpointData();

        StartCoroutine(showCheckpointNotification());
    }
    public void Respawn()
    {
        if (menuLose != null)
            menuLose.SetActive(false);

        stateUnpause();

        if (!hasCheckpoint)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        RestorePlayerCheckpointData();
        RestoreEnemyCheckpointData();
        RestorePickupCheckpointData();
        RestoreObjectiveCheckpointData();
        RestoreObjectiveMarkerCheckpointData();

        RestoreSearchObjectiveCheckpointData();
        RestoreStreetCutsceneCheckpointData();
        RestoreBossCheckpointData();
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

    public void updateGrenadeAmount(int currentGrenade)
    {
        grenadeAmountText.text = currentGrenade.ToString();
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

    IEnumerator FadeInScene()
    {
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(fader.FadeIn());
    }
    
    public void updateDevilDogBar(float currentPoints, float maxPoints)
    {
        if(devilDogBarFill != null)
        {
            devilDogBarFill.fillAmount = (float)currentPoints / maxPoints;
        }
    }

    void SavePlayerCheckpointData()
    {
        playerCheckpointData.position = player.transform.position;
        playerCheckpointData.rotation = player.transform.rotation;
        playerCheckpointData.hp = playerScript.GetHP();
        playerCheckpointData.medkits = playerScript.MedkitCount;
        playerCheckpointData.grenades = playerScript.GetGrenades();
        playerCheckpointData.devilDogPoints = playerScript.GetDevilDogPoints();
        checkpointWeapons = playerScript.GetWeaponCheckpointData();
        checkpointSelectedGunIndex = playerScript.GetSelectedGunIndex();
    }

    void SaveEnemyCheckpointData()
    {
        enemyCheckpointData.Clear();

        SingleSoldierSpawner[] spawnPoints = FindObjectsByType<SingleSoldierSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (SingleSoldierSpawner spawnPoint in spawnPoints)
        {
            if (spawnPoint == null) continue;

            EnemyCheckpointData data = new EnemyCheckpointData();
            data.spawnPoint = spawnPoint;

            SoldierEnemyController soldier = spawnPoint.GetCurrentSoldier();

            if (soldier != null)
            {
                data.position = soldier.transform.position;
                data.rotation = soldier.transform.rotation;
                data.health = soldier.GetHealth();
                data.isAlive = soldier.IsAlive();
            }
            else
            {
                data.position = spawnPoint.transform.position;
                data.rotation = spawnPoint.transform.rotation;
                data.health = 0f;
                data.isAlive = false;
            }

            enemyCheckpointData.Add(data);
        }
    }

    void SavePickupCheckpointData()
    {
        pickupCheckpointData.Clear();

        CheckpointPickup[] pickups = FindObjectsByType<CheckpointPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (CheckpointPickup pickup in pickups)
        {
            if (pickup == null) continue;

            PickupCheckpointData data = new PickupCheckpointData();
            data.pickup = pickup;
            data.isActive = pickup.IsActive();

            pickupCheckpointData.Add(data);
        }
    }

    void RestorePlayerCheckpointData()
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;

        player.transform.SetPositionAndRotation(playerCheckpointData.position, playerCheckpointData.rotation);

        if (cc != null)
            cc.enabled = true;

        playerScript.RespawnReset();

        playerScript.RestoreWeaponsWithoutReinstantiating(checkpointWeapons, checkpointSelectedGunIndex);

        playerScript.SetHP(playerCheckpointData.hp);
        playerScript.SetMedkits(playerCheckpointData.medkits);
        playerScript.SetGrenades(playerCheckpointData.grenades);
        playerScript.SetDevilDogPoints(playerCheckpointData.devilDogPoints);
    }

    void RestoreEnemyCheckpointData()
    {
        foreach (EnemyCheckpointData data in enemyCheckpointData)
        {
            if (data.spawnPoint == null)
                continue;

            SoldierEnemyController soldier = data.spawnPoint.GetCurrentSoldier();

            if (data.isAlive)
            {
                if (soldier == null)
                    soldier = data.spawnPoint.SpawnSoldier();

                if (soldier != null)
                {
                    soldier.RestoreCheckpointState(
                        data.position,
                        data.rotation,
                        data.health,
                        true
                    );
                }
            }
            else
            {
                if (soldier != null)
                {
                    Destroy(soldier.gameObject);
                    data.spawnPoint.ClearSoldierReference();
                }
            }
        }
    }

    void RestorePickupCheckpointData()
    {
        foreach (PickupCheckpointData data in pickupCheckpointData)
        {
            if (data.pickup == null) continue;

            data.pickup.RestoreState(data.isActive);
        }
    }

    void SaveObjectiveCheckpointData()
    {
        objectiveCheckpointData.Clear();

        AreaObjective[] objectives = FindObjectsByType<AreaObjective>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (AreaObjective objective in objectives)
        {
            if (objective == null) continue;

            AreaObjectiveCheckpointData data = new AreaObjectiveCheckpointData();
            data.objective = objective;
            data.wasActive = objective.IsActiveObjective();
            data.wasComplete = objective.IsCompleteObjective();

            objectiveCheckpointData.Add(data);
        }
    }

    void RestoreObjectiveCheckpointData()
    {
        foreach (AreaObjectiveCheckpointData data in objectiveCheckpointData)
        {
            if (data.objective == null) continue;

            data.objective.RestoreCheckpointState(data.wasActive, data.wasComplete);
        }
    }

    void SaveObjectiveMarkerCheckpointData()
    {
        markerCheckpointData.Clear();

        for (int i = 0; i < objMarkers.Count; i++)
        {
            if (objMarkers[i] == null) continue;

            ObjectiveMarkerCheckpointData data = new ObjectiveMarkerCheckpointData();
            data.marker = objMarkers[i];
            data.wasActive = objMarkers[i].isActive;

            markerCheckpointData.Add(data);
        }
    }

    void RestoreObjectiveMarkerCheckpointData()
    {
        int restoredActiveIndex = -1;

        foreach (ObjectiveMarkerCheckpointData data in markerCheckpointData)
        {
            if (data.marker == null) continue;

            data.marker.SetActive(data.wasActive);

            if (data.wasActive)
                restoredActiveIndex = objMarkers.IndexOf(data.marker);
        }

        if (restoredActiveIndex >= 0)
            currentObjectiveIndex = restoredActiveIndex;
        else
            currentObjectiveIndex = 0;

        RefreshMapObjective();
    }

    void SaveSearchObjectiveCheckpointData()
    {
        searchObjectiveCheckpointData.Clear();

        SearchObjective[] objectives = FindObjectsByType<SearchObjective>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (SearchObjective objective in objectives)
        {
            if (objective == null) continue;

            SearchObjectiveCheckpointData data = new SearchObjectiveCheckpointData();
            data.objective = objective;
            data.wasActive = objective.IsActiveObjective();
            data.wasComplete = objective.IsCompleteObjective();

            searchObjectiveCheckpointData.Add(data);
        }
    }

    void RestoreSearchObjectiveCheckpointData()
    {
        foreach (SearchObjectiveCheckpointData data in searchObjectiveCheckpointData)
        {
            if (data.objective == null) continue;

            data.objective.RestoreCheckpointState(data.wasActive, data.wasComplete);
        }
    }

    void SaveStreetCutsceneCheckpointData()
    {
        if (streetCutsceneManager == null)
            return;

        streetCutsceneCheckpointData.cutsceneManager = streetCutsceneManager;
        streetCutsceneCheckpointData.cutscenePlayed = streetCutsceneManager.HasPlayed();

        if (streetCutsceneTrigger != null)
        {
            streetCutsceneCheckpointData.trigger = streetCutsceneTrigger;
            streetCutsceneCheckpointData.triggerUsed = streetCutsceneTrigger.HasTriggered();
        }

        if (streetTank != null)
        {
            streetCutsceneCheckpointData.tankPosition = streetTank.position;
            streetCutsceneCheckpointData.tankRotation = streetTank.rotation;
        }

        streetCutsceneCheckpointData.houseStreetActive = streetCutsceneManager.HouseStreetActive();
        streetCutsceneCheckpointData.houseCornerActive = streetCutsceneManager.HouseCornerActive();
        streetCutsceneCheckpointData.rubbleActive = streetCutsceneManager.RubbleActive();
        streetCutsceneCheckpointData.destroyedHouseStreetActive = streetCutsceneManager.DestroyedHouseStreetActive();
        streetCutsceneCheckpointData.destroyedHouseCornerActive = streetCutsceneManager.DestroyedHouseCornerActive();
    }
    void RestoreStreetCutsceneCheckpointData()
    {
        if (streetCutsceneCheckpointData.cutsceneManager != null)
            streetCutsceneCheckpointData.cutsceneManager.SetPlayed(streetCutsceneCheckpointData.cutscenePlayed);

        if (streetCutsceneCheckpointData.trigger != null)
            streetCutsceneCheckpointData.trigger.SetTriggered(streetCutsceneCheckpointData.triggerUsed);

        if (streetTank != null)
        {
            streetTank.position = streetCutsceneCheckpointData.tankPosition;
            streetTank.rotation = streetCutsceneCheckpointData.tankRotation;
        }

        if (streetCutsceneCheckpointData.cutsceneManager != null)
        {
            streetCutsceneCheckpointData.cutsceneManager.RestoreWorldState(
                streetCutsceneCheckpointData.houseStreetActive,
                streetCutsceneCheckpointData.houseCornerActive,
                streetCutsceneCheckpointData.rubbleActive,
                streetCutsceneCheckpointData.destroyedHouseStreetActive,
                streetCutsceneCheckpointData.destroyedHouseCornerActive
            );
        }
    }

    void SaveBossCheckpointData()
    {
        if (bossCutsceneManager != null)
        {
            bossCheckpointData.cutsceneManager = bossCutsceneManager;
            bossCheckpointData.cutscenePlayed = bossCutsceneManager.HasPlayed();
        }

        if (bossCutsceneTrigger != null)
        {
            bossCheckpointData.trigger = bossCutsceneTrigger;
            bossCheckpointData.triggerUsed = bossCutsceneTrigger.HasTriggered();
        }

        if (bossTank != null)
        {
            bossCheckpointData.tank = bossTank;
            bossCheckpointData.tankPosition = bossTank.position;
            bossCheckpointData.tankRotation = bossTank.rotation;
        }
    }

    void RestoreBossCheckpointData()
    {
        if (bossCheckpointData.cutsceneManager != null)
            bossCheckpointData.cutsceneManager.SetHasPlayed(bossCheckpointData.cutscenePlayed);

        if (bossCheckpointData.trigger != null)
            bossCheckpointData.trigger.SetTriggered(bossCheckpointData.triggerUsed);

        if (bossCheckpointData.tank != null)
        {
            bossCheckpointData.tank.position = bossCheckpointData.tankPosition;
            bossCheckpointData.tank.rotation = bossCheckpointData.tankRotation;
        }
    }


    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        yield return StartCoroutine(fader.FadeOut());

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void settingsMenu()
    {
        menuSettings.SetActive(true);
        menuActive = menuSettings;
        if (Input.GetButtonDown("Cancel"))
        {
            menuSettings.SetActive(false);
            menuActive = menuPause;
        }
    }

    internal void Back()
    {
        menuSettings.SetActive(false);
        menuActive = menuPause;
    }

    public void ShowTutorial(string tutorialID, string header, string body)
    {
        if (PlayerPrefs.GetInt("Tutorial_" + tutorialID, 0) == 1)
            return;

        if (menuActive != null)
            return;

        PlayerPrefs.SetInt("Tutorial_" + tutorialID, 1);
        PlayerPrefs.Save();

        tutorialHeaderText.text = header;
        tutorialBodyText.text = body;

        statePause();

        menuActive = tutorialPopup;
        menuActive.SetActive(true);
    }

    public bool HasShownTutorial(string tutorialID)
    {
        return PlayerPrefs.GetInt("Tutorial_" + tutorialID, 0) == 1;
    }

    public IEnumerator HideLockedAfterDelay()
    {
        lockedText.SetActive(true);
        yield return new WaitForSeconds(1f);
        lockedText.SetActive(false);
    }

    public void ShowLockedText()
    {
        StartCoroutine(HideLockedAfterDelay());
    }

    public void SetCrosshairSpread(float extraSpread)
    {
        float gap = baseCrosshairGap + extraSpread;

        if (crosshairTop != null)
            crosshairTop.anchoredPosition = new Vector2(0f, gap);

        if (crosshairBottom != null)
            crosshairBottom.anchoredPosition = new Vector2(0f, -gap);

        if (crosshairLeft != null)
            crosshairLeft.anchoredPosition = new Vector2(-gap, 0f);

        if (crosshairRight != null)
            crosshairRight.anchoredPosition = new Vector2(gap, 0f);
    }


    public void ResetTutorial(string tutorialID)
    {
            PlayerPrefs.DeleteKey("Tutorial_" + tutorialID);
            PlayerPrefs.Save();
    }

    void UpdateCombatState()
    {
        if (combatTimer > 0)
        {
            combatTimer -= Time.deltaTime;

            if (!playerInCombat)
            {
                playerInCombat = true;
                MusicManager.instance.PlayBaseCombatMusic();
            }
        }
        else
        {
            if (playerInCombat)
            {
                playerInCombat = false;
                MusicManager.instance.PlayBaseMusic();
            }
        }
    }

    void SyncMusicToCombatState()
    {
        if (playerInCombat)
            MusicManager.instance.PlayBaseCombatMusic();
        else
            MusicManager.instance.PlayBaseMusic();
    }

    public void TriggerCombat(float duration = -1f)
    {
        if (duration <= 0)
            duration = combatExitDelay;

        combatTimer = Mathf.Max(combatTimer, duration);
    }
}
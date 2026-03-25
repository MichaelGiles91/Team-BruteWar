using UnityEngine;
using UnityEngine.Playables;

public class StreetCutsceneManager : MonoBehaviour
{
    [Header("--- Timeline ---")]
    [SerializeField] PlayableDirector director;

    [Header("--- Player Scripts To Disable ---")]
    [SerializeField] MonoBehaviour[] playerScriptsToDisable;

    [Header("--- UI Objects To Disable ---")]
    [SerializeField] GameObject[] uiObjectsToDisable;

    [Header("--- Cameras ---")]
    [SerializeField] Camera gameplayCamera;
    [SerializeField] Camera cutsceneCamera;

    [Header("--- Scripts To Disable At Start ---")]
    [SerializeField] MonoBehaviour[] scriptsToDisableAtStart;

    [Header("--- Scripts To Enable At End ---")]
    [SerializeField] MonoBehaviour[] scriptsToEnableAtEnd;

    [Header("--- Stuff to Disable At End ---")]
    [SerializeField] GameObject houseStreet;
    [SerializeField] GameObject houseCorner;

    [Header("--- Stuff to Enable At End ---")]
    [SerializeField] GameObject rubble;
    [SerializeField] GameObject destroyedHouseStreet;
    [SerializeField] GameObject destroyedHouseCorner;


    bool hasPlayed;

    void Awake()
    {
        if (director == null)
            director = GetComponent<PlayableDirector>();

        if (director != null)
            director.playOnAwake = false;

        if (cutsceneCamera != null)
            cutsceneCamera.gameObject.SetActive(false);

        if (gameplayCamera != null)
            gameplayCamera.gameObject.SetActive(true);
    }

    void OnEnable()
    {
        if (director != null)
            director.stopped += OnTimelineStopped;
    }

    void OnDisable()
    {
        if (director != null)
            director.stopped -= OnTimelineStopped;
    }

    public void PlayCutscene()
    {
        if (hasPlayed || director == null)
            return;

        hasPlayed = true;
        BeginCutsceneState();
        director.Play();
    }

    void BeginCutsceneState()
    {
        SetScriptsEnabled(playerScriptsToDisable, false);
        SetScriptsEnabled(scriptsToDisableAtStart, false);
        SetUIEnabled(false);

        if (gameplayCamera != null)
            gameplayCamera.gameObject.SetActive(false);

        if (cutsceneCamera != null)
            cutsceneCamera.gameObject.SetActive(true);
    }

    void EndCutsceneState()
    {
        if (cutsceneCamera != null)
            cutsceneCamera.gameObject.SetActive(false);

        if (gameplayCamera != null)
            gameplayCamera.gameObject.SetActive(true);

        if (houseCorner != null) houseCorner.SetActive(false);
        if (houseStreet != null) houseStreet.SetActive(false);
        if (destroyedHouseCorner != null) destroyedHouseCorner.SetActive(true);
        if (destroyedHouseStreet != null) destroyedHouseStreet.SetActive(true);
        if (rubble != null) rubble.SetActive(true);

        SetScriptsEnabled(playerScriptsToDisable, true);
        SetScriptsEnabled(scriptsToEnableAtEnd, true);
        SetUIEnabled(true);
    }

    void OnTimelineStopped(PlayableDirector stoppedDirector)
    {
        if (stoppedDirector != director)
            return;

        EndCutsceneState();
    }

    void SetScriptsEnabled(MonoBehaviour[] scripts, bool value)
    {
        if (scripts == null) return;

        foreach (MonoBehaviour script in scripts)
        {
            if (script != null)
                script.enabled = value;
        }
    }

    void SetUIEnabled(bool value)
    {
        if (uiObjectsToDisable == null) return;

        foreach (GameObject obj in uiObjectsToDisable)
        {
            if (obj != null)
                obj.SetActive(value);
        }
    }

    public bool HasPlayed()
    {
        return hasPlayed;
    }

    public void SetPlayed(bool value)
    {
        hasPlayed = value;
    }

    public void RestoreWorldState(bool houseStreetOn, bool houseCornerOn, bool rubbleOn, bool destroyedStreetOn, bool destroyedCornerOn)
    {
        if (houseStreet != null) houseStreet.SetActive(houseStreetOn);
        if (houseCorner != null) houseCorner.SetActive(houseCornerOn);
        if (rubble != null) rubble.SetActive(rubbleOn);
        if (destroyedHouseStreet != null) destroyedHouseStreet.SetActive(destroyedStreetOn);
        if (destroyedHouseCorner != null) destroyedHouseCorner.SetActive(destroyedCornerOn);
    }

    public bool HouseStreetActive() => houseStreet != null && houseStreet.activeSelf;
    public bool HouseCornerActive() => houseCorner != null && houseCorner.activeSelf;
    public bool RubbleActive() => rubble != null && rubble.activeSelf;
    public bool DestroyedHouseStreetActive() => destroyedHouseStreet != null && destroyedHouseStreet.activeSelf;
    public bool DestroyedHouseCornerActive() => destroyedHouseCorner != null && destroyedHouseCorner.activeSelf;
}
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

    [Header("--- Rubble to Enable At End---")]
    [SerializeField] GameObject rubble;

    bool isPlaying;

    void Awake()
    {
        if (director == null)
            director = GetComponent<PlayableDirector>();

        Debug.Log("Awake called on StreetCutsceneManager");

        if (gameplayCamera != null)
            Debug.Log("Gameplay camera assigned: " + gameplayCamera.name);
        else
            Debug.LogWarning("Gameplay camera is NOT assigned");

        if (cutsceneCamera != null)
            Debug.Log("Cutscene camera assigned: " + cutsceneCamera.name);
        else
            Debug.LogWarning("Cutscene camera is NOT assigned");

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
        Debug.Log("PlayCutscene called");

        if (isPlaying)
        {
            Debug.LogWarning("Cutscene already playing");
            return;
        }

        if (director == null)
        {
            Debug.LogWarning("PlayableDirector is null");
            return;
        }

        isPlaying = true;
        BeginCutsceneState();
        director.Play();
    }

    void BeginCutsceneState()
    {
        Debug.Log("BeginCutsceneState called");

        SetScriptsEnabled(playerScriptsToDisable, false);
        SetScriptsEnabled(scriptsToDisableAtStart, false);
        SetUIEnabled(false);

        if (gameplayCamera != null)
        {
            Debug.Log("Turning OFF gameplay camera object: " + gameplayCamera.name);
            gameplayCamera.gameObject.SetActive(false);
        }

        if (cutsceneCamera != null)
        {
            Debug.Log("Turning ON cutscene camera object: " + cutsceneCamera.name);
            cutsceneCamera.gameObject.SetActive(true);
        }
    }

    void EndCutsceneState()
    {
        Debug.Log("EndCutsceneState called");

        if (cutsceneCamera != null)
        {
            Debug.Log("Turning OFF cutscene camera object: " + cutsceneCamera.name);
            cutsceneCamera.gameObject.SetActive(false);
        }

        if (gameplayCamera != null)
        {
            Debug.Log("Turning ON gameplay camera object: " + gameplayCamera.name);
            gameplayCamera.gameObject.SetActive(true);
        }

        rubble.SetActive(true);
        SetScriptsEnabled(playerScriptsToDisable, true);
        SetScriptsEnabled(scriptsToEnableAtEnd, true);
        SetUIEnabled(true);

        isPlaying = false;
    }

    void OnTimelineStopped(PlayableDirector stoppedDirector)
    {
        Debug.Log("Timeline stopped");

        if (stoppedDirector != director) return;
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
}
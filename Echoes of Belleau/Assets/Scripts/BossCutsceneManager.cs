using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class BossCutsceneManager : MonoBehaviour
{
    [Header("--- Timeline ---")]
    [SerializeField] PlayableDirector director;

    [Header("--- Player ---")]
    [SerializeField] GameObject player;
    [SerializeField] MonoBehaviour[] playerScriptsToDisable;

    [Header("--- Cameras ---")]
    [SerializeField] Camera gameplayCamera;
    [SerializeField] Camera cutsceneCamera;

    [Header("--- UI To Disable During Cutscene ---")]
    [SerializeField] GameObject[] uiObjectsToDisable;

    [Header("--- Teleport ---")]
    [SerializeField] Transform bossArenaSpawnPoint;

    //[Header("--- Tank End Position ---")]
    //[SerializeField] Transform tankEndPoint;
    //[SerializeField] Transform tank;

    [Header("--- Fade ---")]
    [SerializeField] ScreenFader fader;
    [SerializeField] float fadeOutDelayBeforeTeleport = 0f;

    [Header("--- Scripts To Disable At Start ---")]
    [SerializeField] MonoBehaviour[] scriptsToDisableAtStart;

    [Header("--- Scripts To Enable At End ---")]
    [SerializeField] MonoBehaviour[] scriptsToEnableAtEnd;

    [Header("--- Stuff to Enable At End ---")]
    [SerializeField] GameObject gas;

    bool hasPlayed;
    bool isPlaying;

    void Awake()
    {
        if (director == null)
            director = GetComponent<PlayableDirector>();

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
        if (hasPlayed || isPlaying || director == null)
            return;

        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        isPlaying = true;
        hasPlayed = true;

        DisablePlayer();

        foreach (MonoBehaviour script in scriptsToDisableAtStart)
        {
            if (script != null)
                script.enabled = false;
        }

        if (gas != null) gas.SetActive(true);

        if (gameplayCamera != null)
            gameplayCamera.gameObject.SetActive(false);

        if (cutsceneCamera != null)
            cutsceneCamera.gameObject.SetActive(true);

        director.Play();

        yield return null;
    }

    void OnTimelineStopped(PlayableDirector pd)
    {
        if (!isPlaying || pd != director)
            return;

        StartCoroutine(FadeAndTeleport());
    }

    IEnumerator FadeAndTeleport()
    {
        if (fadeOutDelayBeforeTeleport > 0f)
            yield return new WaitForSeconds(fadeOutDelayBeforeTeleport);

        if (fader != null)
            yield return StartCoroutine(fader.FadeOut());

        //if (tank != null && tankEndPoint != null)
        //{
        //    tank.position = tankEndPoint.position;
        //    tank.rotation = tankEndPoint.rotation;
        //}

        TeleportPlayer();

        if (cutsceneCamera != null)
            cutsceneCamera.gameObject.SetActive(false);

        if (gameplayCamera != null)
            gameplayCamera.gameObject.SetActive(true);

        foreach (MonoBehaviour script in scriptsToEnableAtEnd)
        {
            if (script != null)
                script.enabled = true;
        }

        if (fader != null)
            yield return StartCoroutine(fader.FadeIn());

        EnablePlayer();

        isPlaying = false;
    }

    void TeleportPlayer()
    {
        if (player == null || bossArenaSpawnPoint == null)
            return;

        CharacterController cc = player.GetComponent<CharacterController>();

        if (cc != null)
            cc.enabled = false;

        player.transform.position = bossArenaSpawnPoint.position;
        player.transform.rotation = bossArenaSpawnPoint.rotation;

        if (cc != null)
            cc.enabled = true;
    }

    void DisablePlayer()
    {
        foreach (MonoBehaviour script in playerScriptsToDisable)
        {
            if (script != null)
                script.enabled = false;
        }

        foreach (GameObject obj in uiObjectsToDisable)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    void EnablePlayer()
    {
        foreach (MonoBehaviour script in playerScriptsToDisable)
        {
            if (script != null)
                script.enabled = true;
        }

        foreach (GameObject obj in uiObjectsToDisable)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }

    public bool HasPlayed()
    {
        return hasPlayed;
    }

    public void SetHasPlayed(bool value)
    {
        hasPlayed = value;
    }
}
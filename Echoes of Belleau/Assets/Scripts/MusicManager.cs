using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MusicType
{
    Menu,
    Cutscene,
    Calm,
    Combat,
    Boss
}

[System.Serializable]
public class MusicTrackGroup
{
    public MusicType type;
    public List<AudioClip> clips = new List<AudioClip>();
    public bool loop = true;
}

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [Header("--- Audio Sources ---")]
    [SerializeField] AudioSource sourceA;
    [SerializeField] AudioSource sourceB;

    [Header("--- Music Library ---")]
    [SerializeField] List<MusicTrackGroup> musicGroups = new List<MusicTrackGroup>();

    [Header("--- Fade Settings ---")]
    [SerializeField] float defaultFadeDuration = 2f;
    [SerializeField] float fadeBeforeEnd = 2f;

    AudioSource activeSource;
    AudioSource inactiveSource;

    Coroutine transitionRoutine;
    MusicType? currentMusicType = null;
    int currentClipIndex = -1;
    bool isTransitioning;
    bool endFadeTriggered;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        activeSource = sourceA;
        inactiveSource = sourceB;

        ConfigureSource(sourceA);
        ConfigureSource(sourceB);
    }

    void Update()
    {
        CheckForTrackEnding();
    }

    void ConfigureSource(AudioSource source)
    {
        if (source == null) return;

        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
    }

    void CheckForTrackEnding()
    {
        if (isTransitioning) return;
        if (activeSource == null) return;
        if (!activeSource.isPlaying) return;
        if (activeSource.clip == null) return;
        if (activeSource.loop) return;

        float timeRemaining = activeSource.clip.length - activeSource.time;

        if (!endFadeTriggered && timeRemaining <= fadeBeforeEnd)
        {
            endFadeTriggered = true;
            FadeOutCurrentTrack(defaultFadeDuration);
        }
    }

    MusicTrackGroup GetGroup(MusicType type)
    {
        foreach (MusicTrackGroup group in musicGroups)
        {
            if (group.type == type)
                return group;
        }

        return null;
    }

    public void PlayMusic(MusicType type)
    {
        PlayMusic(type, 0, defaultFadeDuration);
    }

    public void PlayMusic(MusicType type, int clipIndex)
    {
        PlayMusic(type, clipIndex, defaultFadeDuration);
    }

    public void PlayMusic(MusicType type, int clipIndex, float fadeDuration)
    {
        MusicTrackGroup group = GetGroup(type);

        if (group == null)
        {
            Debug.LogWarning("No music group found for type: " + type);
            return;
        }

        if (group.clips == null || group.clips.Count == 0)
        {
            Debug.LogWarning("Music group has no clips assigned for type: " + type);
            return;
        }

        if (clipIndex < 0 || clipIndex >= group.clips.Count)
        {
            Debug.LogWarning("Clip index out of range for type: " + type + " | Index: " + clipIndex);
            return;
        }

        AudioClip selectedClip = group.clips[clipIndex];

        if (selectedClip == null)
        {
            Debug.LogWarning("Selected clip is null for type: " + type + " | Index: " + clipIndex);
            return;
        }

        // If this exact track is already playing, do nothing
        if (currentMusicType.HasValue &&
            currentMusicType.Value == type &&
            currentClipIndex == clipIndex &&
            activeSource.isPlaying &&
            activeSource.clip == selectedClip)
        {
            return;
        }

        if (!activeSource.isPlaying && !inactiveSource.isPlaying)
        {
            activeSource.clip = selectedClip;
            activeSource.volume = 1f;
            activeSource.loop = group.loop;
            activeSource.Play();

            currentMusicType = type;
            currentClipIndex = clipIndex;
            endFadeTriggered = false;
            return;
        }

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(CrossfadeToTrack(selectedClip, group.loop, type, clipIndex, fadeDuration));
    }

    public void StopMusic(float fadeDuration = -1f)
    {
        if (fadeDuration < 0f)
            fadeDuration = defaultFadeDuration;

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(FadeOutOnly(fadeDuration));
    }

    public void FadeOutCurrentTrack(float fadeDuration = -1f)
    {
        if (fadeDuration < 0f)
            fadeDuration = defaultFadeDuration;

        if (!activeSource.isPlaying)
            return;

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(FadeOutOnly(fadeDuration));
    }

    IEnumerator CrossfadeToTrack(AudioClip newClip, bool shouldLoop, MusicType newType, int newClipIndex, float fadeDuration)
    {
        isTransitioning = true;
        endFadeTriggered = false;

        // Figure out which source is currently dominant
        if (sourceA.isPlaying || sourceB.isPlaying)
        {
            if (sourceA.volume >= sourceB.volume)
            {
                activeSource = sourceA;
                inactiveSource = sourceB;
            }
            else
            {
                activeSource = sourceB;
                inactiveSource = sourceA;
            }
        }

        // If inactive source already has the requested clip, keep it.
        // Otherwise replace it.
        if (inactiveSource.clip != newClip)
        {
            inactiveSource.Stop();
            inactiveSource.clip = newClip;
            inactiveSource.loop = shouldLoop;
            inactiveSource.volume = 0f;
            inactiveSource.Play();
        }
        else if (!inactiveSource.isPlaying)
        {
            inactiveSource.loop = shouldLoop;
            inactiveSource.Play();
        }

        float startActiveVolume = activeSource.isPlaying ? activeSource.volume : 0f;
        float startInactiveVolume = inactiveSource.volume;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = fadeDuration <= 0f ? 1f : timer / fadeDuration;

            if (activeSource.isPlaying)
                activeSource.volume = Mathf.Lerp(startActiveVolume, 0f, t);

            inactiveSource.volume = Mathf.Lerp(startInactiveVolume, 1f, t);

            yield return null;
        }

        if (activeSource.isPlaying)
        {
            activeSource.volume = 0f;
            activeSource.Stop();
        }

        inactiveSource.volume = 1f;

        AudioSource temp = activeSource;
        activeSource = inactiveSource;
        inactiveSource = temp;

        currentMusicType = newType;
        currentClipIndex = newClipIndex;
        isTransitioning = false;
        transitionRoutine = null;
    }

    IEnumerator FadeOutOnly(float fadeDuration)
    {
        isTransitioning = true;

        float startVolume = activeSource.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = fadeDuration <= 0f ? 1f : timer / fadeDuration;

            activeSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        activeSource.volume = 0f;
        activeSource.Stop();
        activeSource.clip = null;

        currentMusicType = null;
        currentClipIndex = -1;
        endFadeTriggered = false;
        isTransitioning = false;
        transitionRoutine = null;
    }

    public bool IsPlayingTwoSongs()
    {
        return sourceA != null && sourceB != null && sourceA.isPlaying && sourceB.isPlaying;
    }

    public MusicType? GetCurrentMusicType()
    {
        return currentMusicType;
    }

    public int GetCurrentClipIndex()
    {
        return currentClipIndex;
    }
}
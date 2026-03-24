using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SFXType
{
    Jump,
    Land,
    Hurt,
    Death,  
    Medkit,
    
    FootstepWalk,
    FootstepRun,

    BreathingIdleLoop,
    BreathingRunLoop,
    BreathingOutOfBreathLoop,

    Knife,
    GrenadeThrow,
    GrenadeExplode,
    Reload,
}

[System.Serializable]
public class SFXGroup
{
    public SFXType type;
    public List<AudioClip> clips = new List<AudioClip>();
    [Range(0f, 1f)] public float volume = 1f;
}

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;

    [Header("--- Audio Sources ---")]
    [SerializeField] AudioSource oneShotSource;
    [SerializeField] AudioSource footstepSource;
    [SerializeField] AudioSource breathingSource;

    [Header("--- SFX Library ---")]
    [SerializeField] List<SFXGroup> sfxGroups = new List<SFXGroup>();

    Coroutine breathingRoutine;
    SFXType? currentBreathingType;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        ConfigureSource(oneShotSource, false);
        ConfigureSource(footstepSource, false);
        ConfigureSource(breathingSource, false);
    }

    void ConfigureSource(AudioSource source, bool looping)
    {
        if (source == null) return;

        source.playOnAwake = false;
        source.loop = looping;
        source.spatialBlend = 0f;
    }

    SFXGroup GetGroup(SFXType type)
    {
        foreach (SFXGroup group in sfxGroups)
        {
            if (group.type == type)
                return group;
        }

        return null;
    }

    AudioClip GetRandomClip(SFXGroup group, AudioClip lastClip = null)
    {
        if (group == null || group.clips == null || group.clips.Count == 0)
            return null;

        if (group.clips.Count == 1)
            return group.clips[0];

        AudioClip chosenClip = null;
        int safety = 0;

        while (chosenClip == null || chosenClip == lastClip)
        {
            chosenClip = group.clips[Random.Range(0, group.clips.Count)];
            safety++;

            if (safety > 10)
                break;
        }

        return chosenClip;
    }

    public void PlayOneShot(SFXType type)
    {
        if (oneShotSource == null) return;

        SFXGroup group = GetGroup(type);
        if (group == null) return;

        AudioClip clip = GetRandomClip(group);
        if (clip == null) return;

        oneShotSource.PlayOneShot(clip, group.volume);
    }

    public void PlayFootstep(SFXType type)
    {
        if (footstepSource == null)
        {
            
            return;
        }

        SFXGroup group = GetGroup(type);
        if (group == null)
        {
            
            return;
        }

        AudioClip clip = GetRandomClip(group);
        if (clip == null)
        {
            
            return;
        }

        
        footstepSource.PlayOneShot(clip, group.volume);
    }

    public void StopFootstepLoop()
    {
        if (footstepSource != null)
        {
            footstepSource.Stop();
            footstepSource.clip = null;
        }
    }

    public void PlayBreathingLoop(SFXType type)
    {
        if (currentBreathingType == type)
            return;

        StopBreathingLoop();

        currentBreathingType = type;
        breathingRoutine = StartCoroutine(PlayRandomBreathingLoop(type, breathingSource));
    }

    public void StopBreathingLoop()
    {
        if (breathingRoutine != null)
        {
            StopCoroutine(breathingRoutine);
            breathingRoutine = null;
        }

        currentBreathingType = null;

        if (breathingSource != null)
        {
            breathingSource.Stop();
            breathingSource.clip = null;
        }
    }

    IEnumerator PlayRandomBreathingLoop(SFXType type, AudioSource source)
    {
        if (source == null)
            yield break;

        SFXGroup group = GetGroup(type);
        if (group == null)
            yield break;

        AudioClip lastClip = null;

        while (true)
        {
            AudioClip clip = GetRandomClip(group, lastClip);
            if (clip == null)
                yield break;

            source.clip = clip;
            source.volume = group.volume;
            source.loop = false;
            source.Play();

            lastClip = clip;

            yield return new WaitForSeconds(clip.length);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        }
    }

    public void PlayJump()
    {
        PlayOneShot(SFXType.Jump);
    }

    public void PlayLand()
    {
        PlayOneShot(SFXType.Land);
    }

    public void PlayHurt()
    {
        PlayOneShot(SFXType.Hurt);
    }

    public void PlayDeath()
    {
        PlayOneShot(SFXType.Death);
    }

    public void PlayKnifeSwing()
    {
        PlayOneShot(SFXType.Knife);
    }

    public void PlayGrenadeThrow()
    {
        PlayOneShot(SFXType.GrenadeThrow);
    }

    public void PlayGrenadeExplosion()
    {
        PlayOneShot(SFXType.GrenadeExplode);
    }

    public void PlayReload()
    {
        PlayOneShot(SFXType.Reload);
    }

    public void PlayMedkit()
    {
        PlayOneShot(SFXType.Medkit);
    }

    public void PlayWalkFootstep()
    {
        PlayFootstep(SFXType.FootstepWalk);
    }

    public void PlayRunFootstep()
    {
        PlayFootstep(SFXType.FootstepRun);
    }
}
using UnityEngine;

public class StreetCutsceneTrigger : MonoBehaviour
{
    [SerializeField] StreetCutsceneManager cutsceneManager;
    [SerializeField] bool disableTriggerAfterUse = true;

    bool hasTriggered;
    Collider triggerCol;

    void Awake()
    {
        triggerCol = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        MusicManager.instance.PlayMusic(MusicType.Boss, 0, .5f);
        hasTriggered = true;

        if (cutsceneManager != null)
            cutsceneManager.PlayCutscene();

        if (disableTriggerAfterUse && triggerCol != null)
            triggerCol.enabled = false;
    }

    public bool HasTriggered()
    {
        return hasTriggered;
    }

    public void SetTriggered(bool value)
    {
        hasTriggered = value;

        if (triggerCol == null)
            triggerCol = GetComponent<Collider>();

        if (triggerCol != null)
            triggerCol.enabled = !value;
    }
}
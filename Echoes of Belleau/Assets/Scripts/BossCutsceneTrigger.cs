using UnityEngine;

public class BossCutsceneTrigger : MonoBehaviour
{
    [SerializeField] BossCutsceneManager cutsceneManager;
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

        if (!other.CompareTag("Rocket"))
            return;

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

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = !value;
    }
}
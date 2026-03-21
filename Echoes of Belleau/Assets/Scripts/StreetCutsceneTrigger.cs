using UnityEngine;

public class StreetCutsceneTrigger : MonoBehaviour
{
    [SerializeField] StreetCutsceneManager cutsceneManager;
    [SerializeField] bool disableTriggerAfterUse = true;

    bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;

            if (cutsceneManager != null)
                cutsceneManager.PlayCutscene();

            if (disableTriggerAfterUse)
                gameObject.SetActive(false);
        }
    }
}
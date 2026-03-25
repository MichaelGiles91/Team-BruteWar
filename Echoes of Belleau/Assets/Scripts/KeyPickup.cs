using NUnit.Framework.Internal.Filters;
using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [SerializeField] SearchObjective linkedSearchObjective;

    [Header("--- Objective Text ---")]
    [SerializeField] string nextHeader = "New Objective";
    [SerializeField] string nextText = "Reach the extraction point";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Door.Instance.isLocked = false;

            if (linkedSearchObjective != null)
                linkedSearchObjective.CompleteObjective();
            if (gameManager.instance != null)
            {
                gameManager.instance.updateObjectiveText(nextText, nextHeader);
            }
        }
    }
}

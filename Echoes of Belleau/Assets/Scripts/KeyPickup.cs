using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [SerializeField] SearchObjective linkedSearchObjective;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Door.Instance.isLocked = false;

            if (linkedSearchObjective != null)
                linkedSearchObjective.CompleteObjective();
        }
    }
}

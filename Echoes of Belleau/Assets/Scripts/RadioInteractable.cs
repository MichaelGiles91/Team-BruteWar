using UnityEngine;

public class RadioInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] bool singleUse = true;
    bool hasBeenUsed;

    public void Interact()
    {
        if (singleUse && hasBeenUsed) return;

        hasBeenUsed = true;

        // Example hooks into your existing systems:
        // Start tower defense phase here
        // gameManager.instance.StartTowerDefense();

        // Disable collider so it can't be used again
        GetComponent<Collider>().enabled = false;
    }
}
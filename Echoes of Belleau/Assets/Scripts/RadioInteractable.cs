using UnityEngine;

public class RadioInteractable : MonoBehaviour, IInteractable
{
    bool used;

    public void Interact()
    {
        if (used) return;
        used = true;

        Debug.Log("Radio activated");

        //gameManager.instance.StartTowerDefense();

        GetComponent<Collider>().enabled = false;
    }
}

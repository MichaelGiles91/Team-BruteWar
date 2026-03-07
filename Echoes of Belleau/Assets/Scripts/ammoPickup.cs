using UnityEngine;

public class ammoPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        player.PickedUpAmmo();

        Destroy(gameObject);
    }
}
using UnityEngine;

public class GunPickups : MonoBehaviour
{
    [SerializeField] gunStats gun;


    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        gun.ammoCur = gun.magSize;

        bool pickedUp = player.getGunStats(gun);
        if (pickedUp)
        {
            gameObject.SetActive(false);
        }
    }
}

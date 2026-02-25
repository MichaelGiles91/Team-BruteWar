using UnityEngine;

public class GunPickups : MonoBehaviour
{
    [SerializeField] gunStats gun;


    private void OnTriggerEnter(Collider other)
    {
        IPickup pik = other.GetComponent<IPickup>();

        if( pik != null )
        {
            gun.ammoCur = gun.magSize;
            pik.getGunStats(gun);
            Destroy(gameObject);

        }
    }
}

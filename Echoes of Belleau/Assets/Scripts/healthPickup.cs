using UnityEngine;

public class healthPickup : MonoBehaviour
{
    [SerializeField] int amount = 1;

    private void OnTriggerEnter(Collider other)
    {
        IPickup pik = other.GetComponent<IPickup>();

        if (pik != null)
        {
            pik.getMedkit(amount);
            Destroy(gameObject);
        }
    }
}

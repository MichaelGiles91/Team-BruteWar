using UnityEngine;

public class grenadePickup : MonoBehaviour
{
    [SerializeField] int amount = 0;

    private void OnTriggerEnter(Collider other)
    {
        IPickup pik = other.GetComponent<IPickup>();

        if (pik != null)
        {
            pik.getGrenade(amount);
            gameObject.SetActive(false);
        }
    }
}

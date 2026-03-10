using UnityEngine;

public class DDDogTagPickup : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        IPickup pik = other.GetComponent<PlayerController>();

        if(pik != null)
        {
            pik.GetDogTag();
            Destroy(gameObject);
        }
    }
}

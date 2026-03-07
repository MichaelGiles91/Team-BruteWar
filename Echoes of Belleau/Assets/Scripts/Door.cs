
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] GameObject model;

    bool playerInTrigger;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Update()
    {
        if (Input.GetButtonDown("Interact") && playerInTrigger)
        {
            model.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            model.SetActive(true);
            playerInTrigger = false;
        }
    }

    // Update is called once per frame

}

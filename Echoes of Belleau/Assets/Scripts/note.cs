using UnityEngine;

public class note : MonoBehaviour
{
    [SerializeField] GameObject model;
    [SerializeField] GameObject button;
    [SerializeField] GameObject UI;

    bool playerInTrigger;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Interact") && playerInTrigger)
        {
            UI.SetActive(true);
        }
        if (Input.GetButtonDown("Cancel"))
        {
            UI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            button.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            button.SetActive(false);
        }
    }
}

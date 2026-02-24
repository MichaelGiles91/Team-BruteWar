using UnityEngine;

public class Radio : MonoBehaviour
{
    [SerializeField] GameObject model;
    [SerializeField] GameObject button;

    bool playerInTrigger;
    bool used;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Interact") && playerInTrigger && !used)
        {
            used = true;
            DefenseManager.instance.startDefense();
            button.SetActive(false);
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
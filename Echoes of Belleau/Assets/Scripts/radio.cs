using UnityEngine;

public class Radio : MonoBehaviour
{
    [SerializeField] GameObject model;
    [SerializeField] GameObject button;

    bool playerInTrigger;
    bool used;

    private void Start()
    {
        used = false;
        button.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Interact") && playerInTrigger && !used)
        {
            used = true;
            button.SetActive(false);
            DefenseManager.instance.StartDefense();    
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (used)
        {
            return;
        }
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
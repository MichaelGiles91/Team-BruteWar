using UnityEngine;

public class note : MonoBehaviour
{
    [SerializeField] GameObject model;
    [SerializeField] GameObject button;
    [SerializeField] GameObject UI;

    bool playerInTrigger;

    void Update()
    {
        if (Input.GetButtonDown("Interact") && playerInTrigger)
        {
            if (gameManager.instance.menuActive == null)
            {
                gameManager.instance.statePause();

                gameManager.instance.menuActive = UI;
                UI.SetActive(true);
            }
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

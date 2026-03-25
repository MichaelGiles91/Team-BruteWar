using System.IO;
using UnityEngine;

public class note : MonoBehaviour
{
    [SerializeField] GameObject model;
    [SerializeField] GameObject button;
    [SerializeField] GameObject UI;

    [Header("--- Objective Markers ---")]
    [SerializeField] ObjMarker markerToDisable;
    [SerializeField] ObjMarker markerToEnable;

    bool playerInTrigger;
    bool used;

    void Update()
    {
        if (Input.GetButtonDown("Interact") && playerInTrigger && !used)
        {
            used = true;
            if (gameManager.instance.menuActive == null)
            {
                gameManager.instance.statePause();

                gameManager.instance.menuActive = UI;
                UI.SetActive(true);

                if (gameManager.instance != null)
                    gameManager.instance.updateObjectiveText("New Objective", "Find the officer with the keys");

                // Disable old marker
                if (markerToDisable != null)
                    markerToDisable.SetActive(false);

                // Enable new marker
                if (markerToEnable != null)
                    markerToEnable.SetActive(true);
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

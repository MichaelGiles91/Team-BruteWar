using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] float interactDistance = 3f;
    [SerializeField] LayerMask interactLayer;
    [SerializeField] Camera playerCamera;

    [SerializeField] TMP_Text interactPrompt;

    IInteractable currentInteractable;

    void Update()
    {
        if (gameManager.instance.isPaused)
        {
            HidePrompt();
            return;
        }

        CheckForInteractable();

        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact();
        }
    }

    void CheckForInteractable()
    {
        currentInteractable = null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            currentInteractable = hit.collider.GetComponent<IInteractable>();
        }

        ShowPrompt(currentInteractable != null);
    }

    void ShowPrompt(bool show)
    {
        if (interactPrompt != null)
            interactPrompt.gameObject.SetActive(show);
    }

    void HidePrompt()
    {
        if (interactPrompt != null)
            interactPrompt.gameObject.SetActive(false);
    }
}

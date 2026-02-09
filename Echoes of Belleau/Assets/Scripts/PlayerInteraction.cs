using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] float interactDistance = 3f;
    [SerializeField] LayerMask interactLayer;
    [SerializeField] Camera playerCamera;

    [SerializeField] TMP_Text interactPrompt;

    IInteractable currentInteractable;
    IHighlightable currentHighlight;

    void Update()
    {
        if (gameManager.instance.isPaused)
        {
            ClearHighlight();
            HidePrompt();
            return;
        }

        CheckForInteractable();

        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact();
            ClearHighlight();
        }
    }

    void CheckForInteractable()
    {
        IHighlightable newHighlight = null;
        IInteractable newInteractable = null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            newInteractable = hit.collider.GetComponent<IInteractable>();
            newHighlight = hit.collider.GetComponent<IHighlightable>();
        }

        if (newHighlight != currentHighlight)
        {
            ClearHighlight();
            currentHighlight = newHighlight;
            currentHighlight?.Highlight(true);
        }

        currentInteractable = newInteractable;
        ShowPrompt(currentInteractable != null);
    }

    void ClearHighlight()
    {
        if (currentHighlight != null)
        {
            currentHighlight.Highlight(false);
            currentHighlight = null;
        }
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

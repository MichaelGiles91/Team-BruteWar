using UnityEngine;

public class RadioInteractable : MonoBehaviour, IInteractable, IHighlightable
{
    [SerializeField] Renderer radioRenderer;
    [SerializeField] Material normalMaterial;
    [SerializeField] Material highlightMaterial;

    bool used;

    void Awake()
    {
        if (radioRenderer == null)
            radioRenderer = GetComponentInChildren<Renderer>();

        radioRenderer.sharedMaterial = normalMaterial;
    }

    public void Interact()
    {
        if (used) return;
        used = true;

        Highlight(false);
        GetComponent<Collider>().enabled = false;

        Debug.Log("Reinforcements enroute, hold the line until they arrive");

        DefenseManager.instance.startDefense();
    }

    public void Highlight(bool enable)
    {
        if (used) return;

        radioRenderer.sharedMaterial = enable ? highlightMaterial : normalMaterial;
    }
}
using UnityEngine;

public class RadioInteractable : MonoBehaviour, IInteractable, IHighlightable
{
    [SerializeField] Renderer radioRenderer;
    [SerializeField] Material normalMat;
    [SerializeField] Material highlightMat;

    bool used;

    void Awake()
    {
        radioRenderer.material = normalMat;
    }

    public void Interact()
    {
        if (used) return;
        used = true;

        Highlight(false);
        GetComponent<Collider>().enabled = false;

        Debug.Log("Radio activated");
        //defenceManager.instance.StartTowerDefense();
    }

    public void Highlight(bool enable)
    {
        if (used) return;

        radioRenderer.material = enable ? highlightMat : normalMat;
    }
}

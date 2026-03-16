using UnityEngine;

public class CheckpointPickup : MonoBehaviour
{
    [SerializeField] GameObject rootObject;

    void Awake()
    {
        if (rootObject == null)
            rootObject = gameObject;
    }

    public bool IsActive()
    {
        return rootObject.activeSelf;
    }

    public void RestoreState(bool activeState)
    {
        rootObject.SetActive(activeState);
    }
}

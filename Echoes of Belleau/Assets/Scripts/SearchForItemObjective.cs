using UnityEngine;

public class SearchObjective : MonoBehaviour
{
    [Header("--- Objective Info ---")]
    [SerializeField] string objectiveHeader = "Objective Updated";
    [SerializeField] string objectiveText = "Search the area for the item";

    [Header("--- Objective Marker ---")]
    [SerializeField] ObjMarker marker;

    bool isActive;
    bool isComplete;

    void Start()
    {
        if (marker != null)
            marker.SetActive(false);
    }

    public void ActivateObjective()
    {
        if (isComplete)
            return;

        isActive = true;

        if (marker != null)
            marker.SetActive(true);

        if (gameManager.instance != null)
            gameManager.instance.updateObjectiveText(objectiveText, objectiveHeader);
    }

    public void CompleteObjective()
    {
        if (!isActive || isComplete)
            return;

        isComplete = true;
        isActive = false;

        if (marker != null)
            marker.SetActive(false);


        if (gameManager.instance != null)
            gameManager.instance.CompleteCurrentObjectiveAndAdvance();
    }

    public bool IsActiveObjective()
    {
        return isActive;
    }

    public bool IsCompleteObjective()
    {
        return isComplete;
    }

    public void RestoreCheckpointState(bool wasActive, bool wasComplete)
    {
        isComplete = wasComplete;
        isActive = wasActive && !wasComplete;

        if (marker != null)
            marker.SetActive(isActive);

    }
}
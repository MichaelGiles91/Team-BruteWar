using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DefenseObjectiveZone : MonoBehaviour
{
    [Header("Objective UI")]
    [SerializeField] string headerText;
    [TextArea][SerializeField] string objectiveText;

    [Header("Next Objective UI")]
    [SerializeField] string nextObjectiveHeader;
    [TextArea][SerializeField] string nextObjectiveText;
    [SerializeField] float nextObjectiveDelay = 1f;

    DefenseManager defense;
    BoxCollider box;

    bool active;
    bool complete;

    void Awake()
    {
        box = GetComponent<BoxCollider>();
        box.isTrigger = true;

        defense = GetComponentInParent<DefenseManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (active || complete) return;
        if (!other.CompareTag("Player")) return;

        active = true;

        if (gameManager.instance != null)
        {
            gameManager.instance.updateObjectiveText(objectiveText, headerText);
            gameManager.instance.SetActiveObjectiveZone(box);
        }

        if (defense == null)
        {
            Debug.LogError("[DefenseObjectiveZone] No DefenseManager found in parent.");
            return;
        }

        defense.OnDefenseStateChanged += RefreshUI;
        RefreshUI();
    }

    void RefreshUI()
    {
        if (!active || complete || defense == null) return;

        if (gameManager.instance != null)
            gameManager.instance.updateObjEnemyCounter(defense.AliveEnemyCount);

        if (defense.FinalWaveSpawned && defense.AliveEnemyCount == 0)
            CompleteObjective();
    }

    void CompleteObjective()
    {
        complete = true;

        if (defense != null)
            defense.OnDefenseStateChanged -= RefreshUI;

        if (gameManager.instance != null)
        {
            gameManager.instance.updateObjectiveText("Defense cleared.", "Objective Complete!");
            gameManager.instance.CompleteCurrentObjectiveAndAdvance();
            gameManager.instance.SetActiveObjectiveZone(null);
        }

        StartCoroutine(ShowNextObjectiveAfterDelay());
    }

    IEnumerator ShowNextObjectiveAfterDelay()
    {
        yield return new WaitForSeconds(nextObjectiveDelay);

        if (gameManager.instance != null)
            gameManager.instance.updateObjectiveText(nextObjectiveText, nextObjectiveHeader);
    }

    void OnDestroy()
    {
        if (defense != null)
            defense.OnDefenseStateChanged -= RefreshUI;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class AreaObjective : MonoBehaviour
{
    [Header("Objective UI")]
    [SerializeField] string headerText = "Objective";
    [TextArea][SerializeField] string objectiveText = "Defeat all enemies in this area.";
    [Header("Next Objective UI")]
    [SerializeField] string nextObjectiveHeader = "Next Objective";
    [SerializeField] string nextObjectiveText = "Proceed to the church.";
    [SerializeField] float nextObjectiveDelay = 2f;

    [Header("Enemy Filtering")]
    [SerializeField] LayerMask enemyLayer;

    BoxCollider box;

    bool active;
    bool complete;

    HashSet<EnemyAIwRoam> trackedEnemies = new HashSet<EnemyAIwRoam>();

    void Awake()
    {
        box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!active && other.CompareTag("Player"))
        {
            StartObjective();
            return;
        }

        if (!active || complete) return;

        TryTrackEnemy(other);
    }

    void OnTriggerExit(Collider other)
    {
        EnemyAIwRoam enemy = other.GetComponentInParent<EnemyAIwRoam>();
        if (enemy != null && trackedEnemies.Remove(enemy))
        {
            enemy.OnDied -= HandleEnemyDied;
            UpdateObjectiveUI();
        }
    }

    void StartObjective()
    {
        active = true;

        if (gameManager.instance != null)
            gameManager.instance.updateObjectiveText(objectiveText, headerText);

        TrackAllEnemiesInside();

        UpdateObjectiveUI();

        if (trackedEnemies.Count == 0)
            CompleteObjective();
    }

    void CompleteObjective()
    {
        complete = true;

        if (gameManager.instance != null)
            gameManager.instance.updateObjectiveText("Area cleared.", "Objective Complete!");

        StartCoroutine(ShowNextObjectiveAfterDelay());

    }

    void UpdateObjectiveUI()
    {
        
        if (gameManager.instance != null)
            gameManager.instance.updateObjEnemyCounter(trackedEnemies.Count);

    }

    void TrackAllEnemiesInside()
    {
        Vector3 center = transform.TransformPoint(box.center);
        Vector3 halfExtents = Vector3.Scale(box.size, transform.lossyScale) * 0.5f;

        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation, enemyLayer, QueryTriggerInteraction.Collide);

        foreach (var h in hits)
            TryTrackEnemy(h);
    }

    void TryTrackEnemy(Collider col)
    {

        if ((enemyLayer.value & (1 << col.gameObject.layer)) == 0) return;

        EnemyAIwRoam enemy = col.GetComponentInParent<EnemyAIwRoam>();
        if (enemy == null) return;

        if (trackedEnemies.Add(enemy))
        {
            enemy.OnDied += HandleEnemyDied;
            UpdateObjectiveUI();
        }
    }

    void HandleEnemyDied(EnemyAIwRoam deadEnemy)
    {
        deadEnemy.OnDied -= HandleEnemyDied;
        trackedEnemies.Remove(deadEnemy);

        if (!active || complete) return;

        UpdateObjectiveUI();

        if (trackedEnemies.Count == 0)
            CompleteObjective();
    }

    IEnumerator ShowNextObjectiveAfterDelay()
    {
        yield return new WaitForSeconds(nextObjectiveDelay);

        if (gameManager.instance != null)
            gameManager.instance.updateObjectiveText(nextObjectiveText, nextObjectiveHeader);
    }


}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class AreaObjective : MonoBehaviour
{
    [Header("Objective UI")]
    [SerializeField] string headerText;
    [TextArea][SerializeField] string objectiveText;
    [Header("Next Objective UI")]
    [SerializeField] string nextObjectiveHeader;
    [SerializeField] string nextObjectiveText;
    [SerializeField] float nextObjectiveDelay;
    [SerializeField] GameObject nextObjective;

    [Header("Enemy Filtering")]
    [SerializeField] LayerMask enemyLayer;

    BoxCollider box;

    bool active;
    bool complete;

    HashSet<EnemyAI> trackedEnemies = new HashSet<EnemyAI>();

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
        EnemyAI enemy = other.GetComponentInParent<EnemyAI>();
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
        {
            gameManager.instance.updateObjectiveText(objectiveText, headerText);
            gameManager.instance.SetActiveObjectiveZone(box);
        }


        TrackAllEnemiesInside();
        UpdateObjectiveUI();

        if (trackedEnemies.Count == 0)
            CompleteObjective();
    }

    void CompleteObjective()
    {
        complete = true;

        if (gameManager.instance != null)
        {
            gameManager.instance.updateObjectiveText("Area cleared.", "Objective Complete!");
            gameManager.instance.CompleteCurrentObjectiveAndAdvance();
            gameManager.instance.SetActiveObjectiveZone(null);
        }

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
        if (col.isTrigger) return;

        if ((enemyLayer.value & (1 << col.gameObject.layer)) == 0) return;

        EnemyAI enemy = col.GetComponentInParent<EnemyAI>();
        if (enemy == null) return;

        if (trackedEnemies.Add(enemy))
        {
            enemy.OnDied += HandleEnemyDied;
            UpdateObjectiveUI();
        }
    }

    void HandleEnemyDied(EnemyAI deadEnemy)
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
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
    [SerializeField] GameObject invisWall;

    [Header("Enemy Filtering")]
    [SerializeField] LayerMask enemyLayer;

    BoxCollider box;

    bool active;
    bool complete;

    HashSet<SoldierEnemyController> trackedEnemies = new HashSet<SoldierEnemyController>();

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
        SoldierEnemyController enemy = other.GetComponentInParent<SoldierEnemyController>();
        if (enemy != null && trackedEnemies.Remove(enemy))
        {
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
            invisWall.SetActive(false);
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

        SoldierEnemyController enemy = col.GetComponentInParent<SoldierEnemyController>();
        if (enemy == null) return;
        if (!enemy.IsAlive()) return;

        if (trackedEnemies.Add(enemy))
        {
            enemy.OnDied += HandleEnemyDied;
            UpdateObjectiveUI();
        }
    }

    void HandleEnemyDied(SoldierEnemyController deadEnemy)
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

    public void RefreshTrackedEnemies()
    {
        foreach (SoldierEnemyController enemy in trackedEnemies)
        {
            if (enemy != null)
                enemy.OnDied -= HandleEnemyDied;
        }

        trackedEnemies.Clear();

        if (!active || complete)
            return;

        TrackAllEnemiesInside();
        UpdateObjectiveUI();

        if (trackedEnemies.Count == 0)
            CompleteObjective();
    }

    public bool IsActiveObjective()
    {
        return active;
    }

    public bool IsCompleteObjective()
    {
        return complete;
    }

    public void RestoreCheckpointState(bool savedActive, bool savedComplete)
    {
        foreach (SoldierEnemyController enemy in trackedEnemies)
        {
            if (enemy != null)
                enemy.OnDied -= HandleEnemyDied;
        }

        trackedEnemies.Clear();

        active = savedActive;
        complete = savedComplete;

        if (invisWall != null)
            invisWall.SetActive(!complete);

        if (!active)
            return;

        if (complete)
        {
            if (gameManager.instance != null)
                gameManager.instance.SetActiveObjectiveZone(null);

            return;
        }

        TrackAllEnemiesInside();
        UpdateObjectiveUI();

        if (gameManager.instance != null)
        {
            gameManager.instance.updateObjectiveText(objectiveText, headerText);
            gameManager.instance.SetActiveObjectiveZone(box);
        }

        if (trackedEnemies.Count == 0)
            CompleteObjective();
    }
}
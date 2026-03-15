using UnityEngine;

public class DefenseSpawningState : DefenseState
{
    private int currentEntryIndex;
    private int spawnedFromCurrentEntry;
    private float spawnTimer;

    public DefenseSpawningState(DefenseManagerL2 manager, DefenseStateMachine stateMachine)
        : base(manager, stateMachine)
    {
    }

    public override void Enter()
    {
        currentEntryIndex = 0;
        spawnedFromCurrentEntry = 0;
        spawnTimer = 0f;

        Debug.Log("Spawning " + manager.CurrentWave.waveName);
    }

    public override void Update()
    {
        if (manager.CurrentWave == null)
        {
            stateMachine.ChangeState(manager.CompleteState);
            return;
        }

        if (currentEntryIndex >= manager.CurrentWave.spawnEntries.Count)
        {
            stateMachine.ChangeState(manager.WaveActiveState);
            return;
        }

        DefenseManagerL2.SpawnEntry entry = manager.CurrentWave.spawnEntries[currentEntryIndex];

        if (entry == null || entry.enemyPrefab == null || entry.spawnPoint == null || entry.count <= 0)
        {
            currentEntryIndex++;
            spawnedFromCurrentEntry = 0;
            spawnTimer = 0f;
            return;
        }

        spawnTimer -= Time.deltaTime;

        if (spawnTimer > 0f)
            return;

        GameObject spawnedEnemy = Object.Instantiate(
            entry.enemyPrefab,
            entry.spawnPoint.position,
            entry.spawnPoint.rotation
        );

        manager.RegisterSpawnedEnemy(spawnedEnemy);
        spawnedFromCurrentEntry++;

        if (spawnedFromCurrentEntry >= entry.count)
        {
            currentEntryIndex++;
            spawnedFromCurrentEntry = 0;
            spawnTimer = 0f;
        }
        else
        {
            spawnTimer = entry.delayBetweenSpawns;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class DefenseManagerL2 : MonoBehaviour
{
    [System.Serializable]
    public class SpawnEntry
    {
        public GameObject enemyPrefab;
        public Transform spawnPoint;
        public int count = 1; // Number of enemies to spawn
        public float delayBetweenSpawns = 1f; // Delay between each spawn
    }

    [System.Serializable]
    public class WaveData
    {
        public string waveName = "Wave 1";
        public float startDelay = 3f; // Delay before the wave starts
        public List<SpawnEntry> spawnEntries = new List<SpawnEntry>(); // List of enemies to spawn in this wave
    }

    [Header("Waves")]
    public List<WaveData> waves = new List<WaveData>();

    [Header("Auto Start")]
    public bool startOnPlay = true;

    [Header("Between Waves")]
    public float delayAfterWaveClear = 5f; // Delay after clearing a wave before the next one starts

    public int CurrentWaveIndex { get; private set; } = 0; // Start at 0 so the first wave is 1
    public WaveData CurrentWave { get; private set; }

    public List<GameObject> aliveEnemies = new List<GameObject>();

    public DefenseStateMachine stateMachine;

    public DefenseIdleState IdleState { get; private set; }
    public DefenseCountdownState CountdownState { get; private set; }
    public DefenseSpawningState SpawningState { get; private set; }
    public DefenseWaveActiveState WaveActiveState { get; private set; }
    public DefenseCompleteState CompleteState { get; private set; }

    private void Awake()
    {
        stateMachine = new DefenseStateMachine();

        IdleState = new DefenseIdleState(this, stateMachine);
        CountdownState = new DefenseCountdownState(this, stateMachine);
        SpawningState = new DefenseSpawningState(this, stateMachine);
        WaveActiveState = new DefenseWaveActiveState(this, stateMachine);
        CompleteState = new DefenseCompleteState(this, stateMachine);
    }
    private void Start()
    {
        stateMachine.Initialize(IdleState);

        if (startOnPlay)
        {
            StartDefense();
        }
    }

    private void Update()
    {
        stateMachine.Update();
        CleanupDeadEnemies();
    }
    public void StartDefense()
    {
        if (waves == null || waves.Count == 0)
        {
            Debug.LogWarning("DefenseManagerL2 has no waves assigned.");
            return;
        }
        CurrentWaveIndex = 0; // Start at the first wave
        BeginNextWave(); // Start the first wave
    }
    public bool BeginNextWave()
    {
        CurrentWaveIndex++;

        if (CurrentWaveIndex >= waves.Count)
        {
            CurrentWave = null;
            return false; // No more waves to start
        }
        CurrentWave = waves[CurrentWaveIndex];
        return true; // Successfully started the next wave
    }
    public void RegisterSpawnedEnemy(GameObject enemy)
    {
        if (enemy == null) return;

        aliveEnemies.Add(enemy);
    }
    public void CleanupDeadEnemies()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--) // Iterate backwards to safely remove
        {
            if (aliveEnemies[i] == null) // Enemy has been destroyed
            {
                aliveEnemies.RemoveAt(i);
            }
        }
    }
    public bool AreAllEnemiesDefeated()
    {
        CleanupDeadEnemies(); // Ensure we have the latest status of alive enemies
        return aliveEnemies.Count == 0; // If no alive enemies remain, the wave is cleared
    }  
    public int GetAliveEnemyCount()
    {
        CleanupDeadEnemies(); // Ensure we have the latest status of alive enemies
        return aliveEnemies.Count;
    }
}

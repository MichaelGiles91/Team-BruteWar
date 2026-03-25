using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class wave
{
    public string waveName;
    public int baseEnemyCount;
    public int mediumEnemyCount;
    public int bossEnemyCount;
    public float timeBetweenSpawns = 1f;
}

public class DefenseManager : MonoBehaviour
{
    [Header("--- Waves ---")]
    [SerializeField] wave[] waves;
    [SerializeField] float timeBetweenWaves = 5f;
    [SerializeField] float spawnRange = 10f;

    [Header("--- Enemy Prefabs ---")]
    [SerializeField] GameObject enemyPrefabBase;
    [SerializeField] GameObject enemyPrefabMedium;
    [SerializeField] GameObject enemyPrefabBoss;

    [Header("--- Spawn Points ---")]
    [SerializeField] Transform[] spawnPoints;

    bool defenseActive;
    bool defenseComplete;
    int currentWave;

    HashSet<SoldierEnemyController> trackedEnemies = new HashSet<SoldierEnemyController>();

    public System.Action OnDefenseStateChanged;

    bool finalWaveSpawned;
    public bool FinalWaveSpawned => finalWaveSpawned;

    public int AliveEnemyCount => trackedEnemies.Count;

    string sceneName;

    public static DefenseManager instance;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
      sceneName = SceneManager.GetActiveScene().name;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !defenseActive && !defenseComplete)
        {
            StartDefense();
        }
    }

    public void StartDefense()
    {
        defenseActive = true;
        currentWave = 0;

        StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        finalWaveSpawned = false;

        for (int i = 0; i < waves.Length; i++)
        {
            currentWave = i + 1;

            gameManager.instance.updateObjectiveText($"Wave {currentWave} / {waves.Length}", "Enemy Reinforcements!");

            yield return StartCoroutine(SpawnWave(waves[i]));

            if (i == waves.Length - 1)
            {
                finalWaveSpawned = true;
                OnDefenseStateChanged?.Invoke();
            }

            yield return new WaitUntil(() => AliveEnemyCount == 0);
            OnDefenseStateChanged?.Invoke();

            if (i < waves.Length - 1)
            {
                gameManager.instance.updateObjectiveText("", "Next Wave Incoming");

                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }

        DefenseWavesComplete();
    }

    IEnumerator SpawnWave(wave w)
    {
        for (int i = 0; i < w.baseEnemyCount; i++)
        {
            SpawnEnemy(enemyPrefabBase);
            yield return new WaitForSeconds(w.timeBetweenSpawns);
        }

        for (int i = 0; i < w.mediumEnemyCount; i++)
        {
            SpawnEnemy(enemyPrefabMedium);
            yield return new WaitForSeconds(w.timeBetweenSpawns);
        }

        for (int i = 0; i < w.bossEnemyCount; i++)
        {
            SpawnEnemy(enemyPrefabBoss);
            yield return new WaitForSeconds(w.timeBetweenSpawns);
        }
    }

    void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null || spawnPoints.Length == 0)
        {
            return;
        }

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

        Vector3 spawnPOS = point.position + new Vector3(
            Random.Range(-spawnRange, spawnRange),
            0f,
            Random.Range(-spawnRange, spawnRange)
        );

        GameObject go = Instantiate(prefab, spawnPOS, point.rotation);


        SoldierEnemyController enemy = go.GetComponentInParent<SoldierEnemyController>();
        if (enemy == null)
            enemy = go.GetComponent<SoldierEnemyController>();

        if (enemy != null)
        {
            enemy.SetHomePoint(point);
            if (trackedEnemies.Add(enemy))
            {
                enemy.OnDied += HandleEnemyDied;
            }
        }

            OnDefenseStateChanged?.Invoke();
    }

    void HandleEnemyDied(SoldierEnemyController deadEnemy)
    {
        if (deadEnemy == null) return;

        deadEnemy.OnDied -= HandleEnemyDied;
        trackedEnemies.Remove(deadEnemy);

        OnDefenseStateChanged?.Invoke();
    }

    void DefenseWavesComplete()
    {
        defenseActive = false;
        defenseComplete = true;

        if (gameManager.instance != null && sceneName == "Scene1")
        {
            gameManager.instance.LoadSceneWithFade("Between Levels Cutscene");
        }
        else if(gameManager.instance != null && sceneName == "Level 2")
        {
            return;
        }
           
    }

    void OnDestroy()
    {
        foreach (var e in trackedEnemies)
        {
            if (e != null)
                e.OnDied -= HandleEnemyDied;
        }

        trackedEnemies.Clear();
    }
}
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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


    [Header("--- Defense Settings ---")]
    [SerializeField] float defenseTime = 300f;  //Defense Duration
    [SerializeField] GameObject objectiveObject;  // for radio prop later.

    [Header("--- Waves ---")]
    [SerializeField] wave[] waves; // configure each wave in the inspector
    [SerializeField] float timeBetweenWaves = 5f;
    [SerializeField] float spawnRange = 10f;

    [Header("--- Enemy Prefabs ---")]
    [SerializeField] GameObject enemyPrefabBase;
    [SerializeField] GameObject enemyPrefabMedium;
    [SerializeField] GameObject enemyPrefabBoss;

    [Header("--- Spawn Points ---")]
    [SerializeField] Transform[] spawnPoints;

    [Header("--- UI --")]
    [SerializeField] TMP_Text waveText;
    [SerializeField] TMP_Text timerText;
    [SerializeField] GameObject DefenseNotification;

    bool defenseActive;
    bool defenseComplete;
    int currentWave;
    float timer;
    List<GameObject> activeEnemies = new List<GameObject>();
    public static DefenseManager instance;
    private void Awake()
    {
        instance = this;
    }

    // Update is called once per frame

    void Start()
    {
        if (waveText != null)
        {
            waveText.text = "";
        }
        if (timerText != null)
        {
            timerText.text = "";
        }
    }
    void Update()
    {
        if (defenseActive)
        {
            timer -= Time.deltaTime;
            activeEnemies.RemoveAll(e => e == null);

            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(timer / 60f);
                int seconds = Mathf.FloorToInt(timer % 60f);
                timerText.text = string.Format("{0}:{1:00}", minutes, seconds);
            }
            if (timer <= 0)
            {
                defenseWon();
            }
        }
    }
    //when the player walks into the trigger collider, defense begins
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !defenseActive && !defenseComplete)
        {
            startDefense();
        }
    }
    public void startDefense()
    {
        defenseActive = true;
        timer = defenseTime;
        currentWave = 0;

        //Buffer so gameManager win screen doesnt trigget between waves
        //enemies call updateGameGoal(+1) on spawn and UpdateGameGoal(-1) on death
        // this keeps the count high so it never hits 0 until we want it to
        gameManager.instance.updateGameGoal(9999);

        StartCoroutine(runWaves());

        if (DefenseNotification != null)
        {
            StartCoroutine(showNotification());
        }
    }

    IEnumerator runWaves()
    {
        for (int i = 0; i < waves.Length; i++)
        {
            currentWave = i + 1;

            if (waveText != null)
            {
                waveText.text = "Wave " + currentWave + " / " + waves.Length;
            }
            yield return StartCoroutine(SpawnWaves(waves[i]));

            //spawn all enemies for this wave
            yield return new WaitUntil(() => activeEnemies.Count == 0);


            //pause between waves unless this wave is dead
            if (i < waves.Length - 1)
            {
                if (waveText != null)
                {
                    waveText.text = "Next Wave Incoming...";


                }
                yield return new WaitForSeconds(timeBetweenWaves);
            }

        }

    }

    IEnumerator SpawnWaves(wave wave)
    {
        // spawn base enemies one at a time
        for (int i = 0; i < wave.baseEnemyCount; i++)
        {
            spawnEnemy(enemyPrefabBase);
            yield return new WaitForSeconds(wave.timeBetweenSpawns);
        }
        for (int i = 0; i < wave.mediumEnemyCount; i++)
        {
            spawnEnemy(enemyPrefabMedium);
            yield return new WaitForSeconds(wave.timeBetweenSpawns);
        }
        for (int i = 0; i < wave.bossEnemyCount; i++)
        {
            spawnEnemy(enemyPrefabBoss);
            yield return new WaitForSeconds(wave.timeBetweenSpawns);
        }
    }
    void spawnEnemy(GameObject prefab)
    {
        //pick a random spawn point from the array
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];


        // add random offset to enemies so they dont stack on each other
        Vector3 spawnPOS = point.position + new Vector3(Random.Range(-spawnRange, spawnRange), 0f, Random.Range(-spawnRange, spawnRange));

        GameObject enemy = Instantiate(prefab, spawnPOS, point.rotation);
        activeEnemies.Add(enemy);

    }
    void defenseWon()
    {
        defenseActive = false;
        defenseComplete = true;


        StopAllCoroutines();

        if (waveText != null)
        {
            waveText.text = "Defense Complete!";
        }
        // remove buffer 
        // this triggeres the win screen through game manager
        gameManager.instance.updateGameGoal(-99999);

    }
    IEnumerator showNotification()
    {
        DefenseNotification.SetActive(true);
        yield return new WaitForSeconds(3f);
        DefenseNotification.SetActive(false);
    }
}

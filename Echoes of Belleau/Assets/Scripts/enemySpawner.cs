using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class enemySpawner : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefabBase; // Reference to the enemy prefab to spawn
    [SerializeField] GameObject enemyPrefabMedium;
    [SerializeField] GameObject enemyPrefabBoss;
    [SerializeField] int baseEnemyMax = 5;
    [SerializeField] int mediumEnemyMax = 5;
    [SerializeField] int bossEnemyMax = 1;
    [SerializeField] float spawnTime = 1f; // Time interval between spawns in seconds
    [SerializeField] float spawnRange = 10f; // Range within which enemies will be spawned around the spawner's position




    int baseEnemyCount;
    int mediumEnemyCount;
    int bossEnemyCount;// Counter to keep track of the number of enemies spawned

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SpawnEnemiesRoutine()); // Start the coroutine to spawn enemies at regular intervals
    }

      
    private IEnumerator SpawnEnemiesRoutine()
    {
        while (true) // loop to keep spawning
        {
            yield return new WaitForSeconds(spawnTime); // wait for the specified spawn time before spawning the next enemy

            Vector3 spawnPOS = transform.position + new Vector3(Random.Range
                (-spawnRange, spawnRange), 0f, Random.Range(-spawnRange, spawnRange)); // Calculate a random spawn position within the specified range around the spawner's position

            if (baseEnemyCount < baseEnemyMax)
            {
                Instantiate(enemyPrefabBase, spawnPOS, Quaternion.identity); // spawn the enemy at the spawner's position 
                baseEnemyCount++; // Increment the enemy count
            }
            else if (mediumEnemyCount < mediumEnemyMax)
            {
                Instantiate(enemyPrefabMedium, spawnPOS, Quaternion.identity);
                mediumEnemyCount++;
            }
            else if (bossEnemyCount < bossEnemyMax)
            {
                Instantiate(enemyPrefabBoss, spawnPOS, Quaternion.identity);
                bossEnemyCount++;
            }
            else
            {
                yield break; // Exit the loop if all enemy types have reached their maximum count
            }
        }
    }
}

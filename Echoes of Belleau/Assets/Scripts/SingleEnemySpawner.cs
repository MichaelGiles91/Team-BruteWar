using UnityEngine;

public class SingleEnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;

    EnemyAI currentEnemy;

    void Start()
    {
        SpawnEnemy();
    }

    public EnemyAI SpawnEnemy()
    {
        if (currentEnemy != null)
            return currentEnemy;

        GameObject spawnedEnemy = Instantiate(enemyPrefab, transform.position, transform.rotation);
        currentEnemy = spawnedEnemy.GetComponent<EnemyAI>();

        if (currentEnemy != null)
            currentEnemy.SetSpawnPoint(this);

        return currentEnemy;
    }

    public EnemyAI GetCurrentEnemy()
    {
        return currentEnemy;
    }

    public void ClearEnemyReference()
    {
        currentEnemy = null;
    }
}

//ADD THIS TO THE ENEMYAI SCRIPT WHENEVER CODY MERGES

//EnemySpawnPoint spawnPoint;

//public void SetSpawnPoint(EnemySpawnPoint point)
//{
//    spawnPoint = point;
//}

//private void OnDestroy()
//{
//    if (spawnPoint != null)
//        spawnPoint.ClearEnemyReference();
//}
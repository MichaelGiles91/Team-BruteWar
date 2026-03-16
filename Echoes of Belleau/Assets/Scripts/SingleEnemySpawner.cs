using UnityEngine;

public class SingleEnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;

    EnemyAIwRoam currentEnemy;

    void Start()
    {
        SpawnEnemy();
    }

    public EnemyAIwRoam SpawnEnemy()
    {
        if (currentEnemy != null)
            return currentEnemy;

        GameObject spawnedEnemy = Instantiate(enemyPrefab, transform.position, transform.rotation);
        currentEnemy = spawnedEnemy.GetComponent<EnemyAIwRoam>();

        if (currentEnemy != null)
            currentEnemy.SetSpawnPoint(this);

        return currentEnemy;
    }

    public EnemyAIwRoam GetCurrentEnemy()
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
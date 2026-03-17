using UnityEngine;

public class SingleSoldierSpawner : MonoBehaviour
{
    [SerializeField] private GameObject soldierPrefab;

    private SoldierEnemyController currentSoldier;

    private void Start()
    {
        SpawnSoldier();
    }

    public SoldierEnemyController SpawnSoldier()
    {
        if (currentSoldier != null)
            return currentSoldier;

        GameObject spawnedSoldier = Instantiate(soldierPrefab, transform.position, transform.rotation);
        currentSoldier = spawnedSoldier.GetComponent<SoldierEnemyController>();

        if (currentSoldier != null)
        {
            currentSoldier.SetSpawnPoint(this);
            currentSoldier.SetHomePoint(transform);
        }

        return currentSoldier;
    }

    public SoldierEnemyController GetCurrentSoldier()
    {
        return currentSoldier;
    }

    public void ClearSoldierReference()
    {
        currentSoldier = null;
    }
}

using UnityEngine;

public class SingleMedicSpawner : MonoBehaviour
{
    [SerializeField] private GameObject soldierPrefab;

    private MedicEnemyController currentSoldier;

    private void Start()
    {
        SpawnSoldier();
    }

    public MedicEnemyController SpawnSoldier()
    {
        if (currentSoldier != null)
            return currentSoldier;

        GameObject spawnedSoldier = Instantiate(soldierPrefab, transform.position, transform.rotation);
        currentSoldier = spawnedSoldier.GetComponent<MedicEnemyController>();

        if (currentSoldier != null)
        {
            currentSoldier.SetSpawnPoint(this);
            currentSoldier.SetHomePoint(transform);
        }

        return currentSoldier;
    }

    public MedicEnemyController GetCurrentSoldier()
    {
        return currentSoldier;
    }

    public void ClearSoldierReference()
    {
        currentSoldier = null;
    }
}

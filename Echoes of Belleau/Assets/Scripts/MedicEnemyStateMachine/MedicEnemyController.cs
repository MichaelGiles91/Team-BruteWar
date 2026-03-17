using UnityEngine;
using UnityEngine.AI;

public class MedicEnemyController : MonoBehaviour, IDamage
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float stoppingDistance = 0.2f;

    [Header("Patrol / Position")]
    private Transform homePoint;
    public float minIdleTime = 1.5f;
    public float maxIdleTime = 3f;

    [Header("Cover")]
    public float coverSearchRadius = 10f;
    public CoverPoint CurrentCover { get; set; }

    [Header("Healing")]
    public float healRange = 2.5f;
    public float healAmount = 25f;
    public float healDuration = 2f;
    public float waitBetweenPatients = 2f;
    public float allySearchRadius = 20f;

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public bool destroyOnDeath = true;

    [Header("Allies")]
    public LayerMask allyLayer;
    public SoldierEnemyController CurrentPatient { get; set; }

    private NavMeshAgent agent;
    private MedicStateMachine stateMachine;
    SingleMedicSpawner spawnPoint;


    public MedicIdleState IdleState { get; private set; }
    public MedicSeekCoverState SeekCoverState { get; private set; }
    public MedicMoveToCoverState MoveToCoverState { get; private set; }
    public MedicWaitState WaitState { get; private set; }
    public MedicMoveToPatientState MoveToPatientState { get; private set; }
    public MedicHealState HealState { get; private set; }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        stateMachine = new MedicStateMachine();

        IdleState = new MedicIdleState(this, stateMachine);
        SeekCoverState = new MedicSeekCoverState(this, stateMachine);
        MoveToCoverState = new MedicMoveToCoverState(this, stateMachine);
        WaitState = new MedicWaitState(this, stateMachine);
        MoveToPatientState = new MedicMoveToPatientState(this, stateMachine);
        HealState = new MedicHealState(this, stateMachine);
    }

    private void Start()
    {
        currentHealth = maxHealth;
        stateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        stateMachine.Update();
    }

    public void MoveTowards(Vector3 targetPosition, float speed)
    {
        if (agent == null) return;

        agent.isStopped = false;
        agent.speed = speed;
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(targetPosition);
    }

    public void StopMoving()
    {
        if (agent == null) return;

        agent.isStopped = true;
        agent.ResetPath();
    }
    public void SetSpawnPoint(SingleMedicSpawner spawner)
    {
        spawnPoint = spawner;
    }

    public bool HasReachedDestination()
    {
        if (agent == null || agent.pathPending) return false;
        return agent.remainingDistance <= agent.stoppingDistance;
    }

    public bool IsAtCover()
    {
        if (CurrentCover == null) return false;

        float distance = Vector3.Distance(transform.position, CurrentCover.transform.position);
        return distance <= stoppingDistance + 0.25f;
    }

    public bool IsPatientInRange()
    {
        if (CurrentPatient == null) return false;

        float distance = Vector3.Distance(transform.position, CurrentPatient.transform.position);
        return distance <= healRange;
    }

    public void FindBestCover()
    {
        CurrentCover = null;

        CoverPoint[] allCover = FindObjectsByType<CoverPoint>(FindObjectsSortMode.None);
        float bestScore = float.MaxValue;

        foreach (CoverPoint cover in allCover)
        {
            if (cover.isOccupied) continue;

            float distance = Vector3.Distance(transform.position, cover.transform.position);
            if (distance > coverSearchRadius) continue;

            if (distance < bestScore)
            {
                bestScore = distance;
                CurrentCover = cover;
            }
        }
    }

    public void FindClosestInjuredAlly()
    {
        CurrentPatient = null;

        SoldierEnemyController[] allAllies = FindObjectsByType<SoldierEnemyController>(FindObjectsSortMode.None);
        float bestDistance = float.MaxValue;

        foreach (SoldierEnemyController ally in allAllies)
        {
            if (ally == null) continue;
            if (ally.gameObject == gameObject) continue;
            if (ally.IsDead()) continue;
            if (!ally.NeedsHealing()) continue;

            float distance = Vector3.Distance(transform.position, ally.transform.position);
            if (distance > allySearchRadius) continue;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                CurrentPatient = ally;
            }
        }
    }


    public void HealCurrentPatient()
    {
        if (CurrentPatient == null) return;

        CurrentPatient.Heal(healAmount);
        Debug.Log(name + " healed " + CurrentPatient.name + " for " + healAmount);
    }
    public bool IsDead()
    {
        return currentHealth <= 0f;
    }
    private void Die()
    {
        Debug.Log(name + " Medic died.");

        StopMoving();

        if (CurrentCover != null)
        {
            CurrentCover.isOccupied = false;
            CurrentCover = null;
        }
        if (spawnPoint != null)
        {
            spawnPoint.ClearSoldierReference();
        }
        enabled = false;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (destroyOnDeath)
        {
            Destroy(gameObject, 2f);
        }
    }
    public void TakeDamage(float amount)
    {
        if (IsDead()) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log(name + " Medic took damage. Current Health: " + currentHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }
    public void takeDamage(int amount)
    {
        TakeDamage(amount);
    }
    public void SetHomePoint(Transform newHomePoint)
    {
        homePoint = newHomePoint;
    }

}


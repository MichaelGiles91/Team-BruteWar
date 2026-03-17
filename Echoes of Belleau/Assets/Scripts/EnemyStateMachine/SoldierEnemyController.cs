using UnityEngine;
using UnityEngine.AI;

public class SoldierEnemyController : MonoBehaviour, IDamage
{
    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float combatMoveSpeed = 3.5f;
    public float stoppingDistance = 0.2f;

    [Header("Patrol")]
    private Transform homePoint;
    public float patrolRadius = 8f;
    public float minIdleTime = 1.5f;
    public float maxIdleTime = 4f;

    [Header("Target")]
    [SerializeField] private string playerTag = "Player";
    private Transform target;
    public Transform Target => target;
    public float attackRange = 12f;

    [Header("Vision")]
    public float viewDistance = 20f;
    [Range(0f, 180f)]
    public float viewAngle = 90f;
    public Transform eyePoint;
    public LayerMask visionBlockers;
    public LayerMask targetLayer;

    [Header("Cover")]
    public float coverSearchRadius = 12f;
    public CoverPoint CurrentCover { get; set; }

    [Header("Combat")]
    public float fireRate = 0.75f;
    public float lostTargetReturnDelay = 3f;

    [Header("Weapon")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 25f;

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public bool destroyOnDeath = true;

    private SoldierStateMachine stateMachine;
    private NavMeshAgent agent;
    private SingleSoldierSpawner spawnPoint;

    public SoldierIdleState IdleState { get; private set; }
    public SoldierPatrolState PatrolState { get; private set; }
    public SoldierFallbackState ReturnToPostState { get; private set; }
    public SoldierSeekCoverState SeekCoverState { get; private set; }
    public SoldierMoveToCoverState MoveToCoverState { get; private set; }
    public SoldierShootState ShootState { get; private set; }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        stateMachine = new SoldierStateMachine();

        IdleState = new SoldierIdleState(this, stateMachine);
        PatrolState = new SoldierPatrolState(this, stateMachine);
        ReturnToPostState = new SoldierFallbackState(this, stateMachine);
        SeekCoverState = new SoldierSeekCoverState(this, stateMachine);
        MoveToCoverState = new SoldierMoveToCoverState(this, stateMachine);
        ShootState = new SoldierShootState(this, stateMachine);
    }

    private void Start()
    {
        currentHealth = maxHealth;
        FindPlayerTarget();
        stateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        if (IsDead()) return;

        if (target == null)
        {
            FindPlayerTarget();
        }

        stateMachine.Update();
    }
    private void FindPlayerTarget()
    {
        if (target != null)
            return;

        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            target = playerObject.transform;
        }
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

    public bool HasReachedDestination()
    {
        if (agent == null) return false;
        if (agent.pathPending) return false;

        return agent.remainingDistance <= agent.stoppingDistance;
    }

    public bool CanSeeTarget()
    {
        if (target == null)
            return false;

        Transform originTransform = eyePoint != null ? eyePoint : transform;
        Vector3 origin = originTransform.position;

        Vector3 targetPosition = target.position;
        Vector3 directionToTarget = targetPosition - origin;
        float distanceToTarget = directionToTarget.magnitude;

        if (distanceToTarget > viewDistance)
            return false;

        Vector3 flatDirectionToTarget = directionToTarget;
        flatDirectionToTarget.y = 0f;

        Vector3 forward = transform.forward;
        forward.y = 0f;

        if (flatDirectionToTarget.sqrMagnitude < 0.001f)
            return true;

        float angleToTarget = Vector3.Angle(forward, flatDirectionToTarget.normalized);

        if (angleToTarget > viewAngle * 0.5f)
            return false;

        Vector3 rayDirection = directionToTarget.normalized;

        if (Physics.Raycast(origin, rayDirection, out RaycastHit hit, viewDistance, visionBlockers | targetLayer))
        {
            return ((1 << hit.collider.gameObject.layer) & targetLayer) != 0;
        }

        return false;
    }

    public bool IsTargetInAttackRange()
    {
        if (target == null) return false;

        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= attackRange;
    }

    public Vector3 GetRandomPatrolPoint()
    {
        Vector3 center = homePoint != null ? homePoint.position : transform.position;

        for (int i = 0; i < 10; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            Vector3 randomPoint = new Vector3(
                center.x + randomCircle.x,
                center.y,
                center.z + randomCircle.y
            );

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return center;
    }
    public void SetSpawnPoint(SingleSoldierSpawner spawner)
    {
        spawnPoint = spawner;
    }

    public Vector3 GetHomePosition()
    {
        if (homePoint != null)
        {
            return new Vector3(homePoint.position.x, transform.position.y, homePoint.position.z);
        }

        return transform.position;
    }

    public bool IsAtCover()
    {
        if (CurrentCover == null) return false;

        float distance = Vector3.Distance(transform.position, CurrentCover.transform.position);
        return distance <= stoppingDistance + 0.25f;
    }

    public void FindBestCover()
    {
        CurrentCover = null;

        CoverPoint[] allCover = GameObject.FindObjectsByType<CoverPoint>(FindObjectsSortMode.None);

        float bestScore = float.MaxValue;

        foreach (CoverPoint cover in allCover)
        {
            if (cover.isOccupied) continue;

            float distanceToSoldier = Vector3.Distance(transform.position, cover.transform.position);
            if (distanceToSoldier > coverSearchRadius) continue;

            float score = distanceToSoldier;

            if (target != null)
            {
                float distanceFromCoverToTarget = Vector3.Distance(cover.transform.position, target.position);
                score += distanceFromCoverToTarget * 0.5f;
            }

            if (score < bestScore)
            {
                bestScore = score;
                CurrentCover = cover;
            }
        }
    }

    public void FireAtTarget()
    {
        if (target == null || bulletPrefab == null || firePoint == null)
            return;

        Debug.Log(name + " fires at " + target.name);

        Vector3 aimPoint = target.position;
        Vector3 shotDirection = (aimPoint - firePoint.position).normalized;

        if (shotDirection.sqrMagnitude <= 0.001f)
            return;

        Vector3 lookDirection = target.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shotDirection));

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = shotDirection * bulletSpeed;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, coverSearchRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Vector3 center = homePoint != null ? homePoint.position : transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, patrolRadius);
    }

    public bool NeedsHealing()
    {
        return currentHealth < maxHealth && currentHealth > 0f;
    }

    public bool IsDead()
    {
        return currentHealth <= 0f;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead()) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log(name + " took damage. Current Health: " + currentHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead()) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        Debug.Log(name + " was healed. Current Health: " + currentHealth);
    }

    private void Die()
    {
        Debug.Log(name + " died.");

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

    public void takeDamage(int amount)
    {
        TakeDamage(amount);
    }
    public void SetHomePoint(Transform newHomePoint)
    {
        homePoint = newHomePoint;
    }
}
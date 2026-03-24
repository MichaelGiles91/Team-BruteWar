using System;
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
    public Transform firePoint; // normal shoot position when not in cover
    public GameObject bulletPrefab;
    public float bulletSpeed = 25f;

    [Header("Hit Reaction")]
    public AudioSource audioSource;
    public AudioClip hitSound;
    public float hitStutterTime = 0.25f;
    public float alertDurationAfterHit = 3f;

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public bool destroyOnDeath = true;

    private SoldierStateMachine stateMachine;
    private NavMeshAgent agent;
    private SingleSoldierSpawner spawnPoint;
    private bool isDead;

    private float hitStutterTimer = 0f;
    private float alertTimer = 0f;
    private Vector3 lastKnownTargetPosition;

    public bool IsStuttering => hitStutterTimer > 0f;
    public bool IsAlerted => alertTimer > 0f;

    public SoldierIdleState IdleState { get; private set; }
    public SoldierPatrolState PatrolState { get; private set; }
    public SoldierFallbackState ReturnToPostState { get; private set; }
    public SoldierSeekCoverState SeekCoverState { get; private set; }
    public SoldierMoveToCoverState MoveToCoverState { get; private set; }
    public SoldierShootState ShootState { get; private set; }
    public event Action<SoldierEnemyController> OnDied;

    public event Action<SoldierEnemyController> OnDied;

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
        isDead = false;
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

        if (hitStutterTimer > 0f)
        {
            hitStutterTimer -= Time.deltaTime;
            StopMoving();
            return;
        }

        if (alertTimer > 0f)
        {
            alertTimer -= Time.deltaTime;
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
            lastKnownTargetPosition = target.position;
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

    public Transform GetCurrentFireOrigin()
    {
        if (CurrentCover != null && CurrentCover.firePosition != null)
            return CurrentCover.firePosition;

        return firePoint != null ? firePoint : transform;
    }

    public bool CanSeeTarget()
    {
        if (target == null)
            return false;

        Transform fireOrigin = GetCurrentFireOrigin();
        Transform originTransform = fireOrigin != null ? fireOrigin : (eyePoint != null ? eyePoint : transform);
        Vector3 origin = originTransform.position;

        Vector3 targetPosition = target.position;
        Vector3 directionToTarget = targetPosition - origin;
        float distanceToTarget = directionToTarget.magnitude;

        if (distanceToTarget > viewDistance)
        {
            return IsAlerted;
        }

        Vector3 flatDirectionToTarget = directionToTarget;
        flatDirectionToTarget.y = 0f;

        Vector3 forward = transform.forward;
        forward.y = 0f;

        if (flatDirectionToTarget.sqrMagnitude >= 0.001f)
        {
            float angleToTarget = Vector3.Angle(forward, flatDirectionToTarget.normalized);

            if (angleToTarget > viewAngle * 0.5f)
            {
                return IsAlerted;
            }
        }

        Vector3 rayDirection = directionToTarget.normalized;

        if (Physics.Raycast(origin, rayDirection, out RaycastHit hit, viewDistance, visionBlockers | targetLayer))
        {
            bool seesTarget = ((1 << hit.collider.gameObject.layer) & targetLayer) != 0;

            if (seesTarget)
            {
                lastKnownTargetPosition = target.position;
                return true;
            }
        }

        return IsAlerted;
    }

    public bool HasActualLineOfSightToTarget()
    {
        if (target == null)
            return false;

        Transform fireOrigin = GetCurrentFireOrigin();
        Transform originTransform = fireOrigin != null ? fireOrigin : (eyePoint != null ? eyePoint : transform);
        Vector3 origin = originTransform.position;

        Vector3 directionToTarget = target.position - origin;
        float distanceToTarget = directionToTarget.magnitude;

        if (distanceToTarget > viewDistance)
            return false;

        if (Physics.Raycast(origin, directionToTarget.normalized, out RaycastHit hit, viewDistance, visionBlockers | targetLayer))
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
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * patrolRadius;
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

    public Vector3 GetLastKnownTargetPosition()
    {
        return lastKnownTargetPosition;
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
            if (cover == null) continue;
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
        if (target == null || bulletPrefab == null)
            return;

        Transform fireOrigin = GetCurrentFireOrigin();
        if (fireOrigin == null)
            return;

        Vector3 aimPoint = target.position + Vector3.up * 1.2f;
        Vector3 shotDirection = (aimPoint - fireOrigin.position).normalized;

        if (shotDirection.sqrMagnitude <= 0.001f)
            return;

        if (Physics.Raycast(fireOrigin.position, shotDirection, out RaycastHit hit, viewDistance, visionBlockers | targetLayer))
        {
            bool hitTarget = ((1 << hit.collider.gameObject.layer) & targetLayer) != 0;
            if (!hitTarget)
            {
                return;
            }
        }

        Vector3 lookDirection = target.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        GameObject bullet = Instantiate(
            bulletPrefab,
            fireOrigin.position,
            Quaternion.LookRotation(shotDirection)
        );

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = shotDirection * bulletSpeed;
        }

        gameManager.instance.TriggerCombat();
    }

    public bool NeedsHealing()
    {
        return currentHealth < maxHealth && currentHealth > 0f;
    }

    public bool IsDead()
    {
        return currentHealth <= 0f || isDead;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead()) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        // Play hit sound
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        hitStutterTimer = hitStutterTime;
        if (!IsAtCover())
        {
            FindBestCover();

            if (CurrentCover != null)
            {
                stateMachine.ChangeState(SeekCoverState);
            }
        }

        if (target != null)
        {
            lastKnownTargetPosition = target.position;
            alertTimer = alertDurationAfterHit;

            Vector3 lookDirection = target.position - transform.position;
            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }

            if (agent != null && !IsAtCover())
            {
                MoveTowards(lastKnownTargetPosition, combatMoveSpeed);
            }
        }

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

        isDead = true;

        DevilDogMode devilDog = FindFirstObjectByType<DevilDogMode>();
        if (devilDog != null)
        {
            devilDog.AddKillPoints();
        }

        OnDied?.Invoke(this);

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
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    private void OnDestroy()
    {
        if (spawnPoint != null)
        {
            spawnPoint.ClearSoldierReference();
        }

        if (CurrentCover != null)
        {
            CurrentCover.isOccupied = false;
            CurrentCover = null;
        }
    }

    public void takeDamage(int amount)
    {
        TakeDamage(amount);
    }

    public void RestoreCheckpointState(Vector3 pos, Quaternion rot, float health, bool alive)
    {
        gameObject.SetActive(true);
        enabled = true;

        transform.SetPositionAndRotation(pos, rot);

        currentHealth = health;
        isDead = !alive;
        hitStutterTimer = 0f;
        alertTimer = 0f;

        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
            agent.ResetPath();
        }

        if (CurrentCover != null)
        {
            CurrentCover.isOccupied = false;
            CurrentCover = null;
        }

        if (target == null && gameManager.instance != null && gameManager.instance.player != null)
        {
            target = gameManager.instance.player.transform;
            lastKnownTargetPosition = target.position;
        }

        if (alive)
        {
            stateMachine.Initialize(IdleState);
        }
        else
        {
            enabled = false;

            if (agent != null)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }

            gameObject.SetActive(false);
        }
    }

    public void SetHomePoint(Transform newHomePoint)
    {
        homePoint = newHomePoint;
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

        if (CurrentCover != null && CurrentCover.firePosition != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(CurrentCover.firePosition.position, 0.15f);
        }
    }
    public bool IsHitStaggered()
    {
        return hitStutterTimer > 0f;
    }
}
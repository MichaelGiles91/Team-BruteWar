using UnityEngine;
using UnityEngine.AI;

public class TankBossController : MonoBehaviour, IDamage
{
    public enum TankPhase
    {
        Phase1Patrol,
        Phase2BossFight,
        Dead
    }

    [Header("Phase")]
    public TankPhase currentPhase = TankPhase.Phase1Patrol;

    [Header("Movement")]
    public float patrolSpeed = 3f;
    public float chaseSpeed = 5f;
    public float stoppingDistance = 2f;

    [Header("Patrol Route")]
    public Transform[] patrolPoints;
    public int currentPatrolIndex = 0;

    [Header("Target")]
    public Transform target;
    public float detectionRange = 30f;

    [Header("Weapon - Machine Gun")]
    public Transform machineGunFirePoint;
    public GameObject machineGunBulletPrefab;
    public float machineGunFireRate = 0.15f;
    public float machineGunBulletSpeed = 35f;

    [Header("Weapon - Cannon")]
    public Transform cannonFirePoint;
    public GameObject cannonBulletPrefab;
    public float cannonFireRate = 3f;
    public float cannonBulletSpeed = 20f;

    [Header("Health")]
    public float maxHealth = 500f;
    public float currentHealth = 500f;
    public bool destroyOnDeath = false;

    private TankStateMachine stateMachine;
    private NavMeshAgent agent;

    public TankPatrolState PatrolState { get; private set; }
    public TankChaseState ChaseState { get; private set; }
    public TankDeadState DeadState { get; private set; }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        stateMachine = new TankStateMachine();

        PatrolState = new TankPatrolState(this, stateMachine);
        ChaseState = new TankChaseState(this, stateMachine);
        DeadState = new TankDeadState(this, stateMachine);
    }

    private void Start()
    {
        currentHealth = maxHealth;

        if (currentPhase == TankPhase.Phase1Patrol)
        {
            stateMachine.Initialize(PatrolState);
        }
        else
        {
            stateMachine.Initialize(ChaseState);
        }
    }

    private void Update()
    {
        if (currentPhase == TankPhase.Dead)
            return;

        stateMachine.Update();
    }

    public void SetPhase2()
    {
        if (currentPhase == TankPhase.Dead)
            return;

        currentPhase = TankPhase.Phase2BossFight;
        stateMachine.ChangeState(ChaseState);
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
    public Transform GetCurrentPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return null;

        if (currentPatrolIndex < 0 || currentPatrolIndex >= patrolPoints.Length)
            currentPatrolIndex = 0;

        return patrolPoints[currentPatrolIndex];
    }

    public void AdvancePatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        currentPatrolIndex++;

        if (currentPatrolIndex >= patrolPoints.Length)
        {
            currentPatrolIndex = 0;
        }
    }
    //public Transform GetCurrentPatrolPoint()
    //{
    //    if (patrolPoints == null || patrolPoints.Length == 0)
    //        return null;

    //    return patrolPoints[currentPatrolIndex];
    //}

    //public void AdvancePatrolPoint()
    //{
    //    if (patrolPoints == null || patrolPoints.Length == 0)
    //        return;

    //    currentPatrolIndex++;

    //    if (currentPatrolIndex >= patrolPoints.Length)
    //    {
    //        currentPatrolIndex = 0;
    //    }
    //}

    public bool IsTargetInRange(float range)
    {
        if (target == null) return false;

        return Vector3.Distance(transform.position, target.position) <= range;
    }

    public void FaceTarget()
    {
        if (target == null) return;

        Vector3 lookDirection = target.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    public void FireMachineGun()
    {
        if (target == null || machineGunBulletPrefab == null || machineGunFirePoint == null)
            return;

        Vector3 aimPoint = target.position + Vector3.up * 1.2f;
        Vector3 shotDirection = (aimPoint - machineGunFirePoint.position).normalized;

        if (shotDirection.sqrMagnitude <= 0.001f)
            return;

        GameObject bullet = Instantiate(
            machineGunBulletPrefab,
            machineGunFirePoint.position,
            Quaternion.LookRotation(shotDirection)
        );

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = shotDirection * machineGunBulletSpeed;
        }
    }

    public void FireCannon()
    {
        if (target == null || cannonBulletPrefab == null || cannonFirePoint == null)
            return;

        Vector3 aimPoint = target.position + Vector3.up * 1f;
        Vector3 shotDirection = (aimPoint - cannonFirePoint.position).normalized;

        if (shotDirection.sqrMagnitude <= 0.001f)
            return;

        GameObject shell = Instantiate(
            cannonBulletPrefab,
            cannonFirePoint.position,
            Quaternion.LookRotation(shotDirection)
        );

        Rigidbody rb = shell.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = shotDirection * cannonBulletSpeed;
        }
    }

    public void takeDamage(int amount)
    {
        TakeDamage(amount);
    }

    public void TakeDamage(float amount)
    {
        if (currentPhase == TankPhase.Dead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log(name + " tank took damage. Current Health: " + currentHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        currentPhase = TankPhase.Dead;
        stateMachine.ChangeState(DeadState);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}

using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls the boss tank behavior, weapons, health and phase transitions.
/// Uses a simple state machine with patrol, chase (boss fight) and dead states.
/// Implements IDamage so external systems can apply damage.
/// </summary>
public class TankBossController : MonoBehaviour, IDamage
{
    #region Types

    /// <summary>
    /// High level phases the boss can be in. These affect available behaviour and state machine initialization.
    /// </summary>
    public enum TankPhase
    {
        Phase1Patrol,
        Phase2BossFight,
        Dead
    }

    #endregion

    #region Inspector Fields

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

    #endregion

    #region Private State

    // Core state machine that drives behaviour.
    private TankStateMachine stateMachine;
    // NavMeshAgent used for movement and pathfinding.
    private NavMeshAgent agent;

    // Public read-only references to each state so external callers (states) can request transitions.
    public TankPatrolState PatrolState { get; private set; }
    public TankChaseState ChaseState { get; private set; }
    public TankDeadState DeadState { get; private set; }

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        // Cache references and create the state machine + state instances.
        agent = GetComponent<NavMeshAgent>();
        stateMachine = new TankStateMachine();

        PatrolState = new TankPatrolState(this, stateMachine);
        ChaseState = new TankChaseState(this, stateMachine);
        DeadState = new TankDeadState(this, stateMachine);
    }

    private void Start()
    {
        // Initialize health and start with the appropriate state depending on configured phase.
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
        // Do nothing when dead.
        if (currentPhase == TankPhase.Dead)
            return;

        // Drive current state update logic.
        stateMachine.Update();
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize detection range in the editor for tuning.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    #endregion

    #region Phase Management

    /// <summary>
    /// Moves the boss into phase 2 (boss fight) and changes the state to chase.
    /// Safe to call multiple times; will early-out if already dead.
    /// </summary>
    public void SetPhase2()
    {
        if (currentPhase == TankPhase.Dead)
            return;

        currentPhase = TankPhase.Phase2BossFight;
        stateMachine.ChangeState(ChaseState);
    }

    #endregion

    #region Movement (NavMeshAgent)

    /// <summary>
    /// Command the NavMeshAgent to move towards a world position at the provided speed.
    /// </summary>
    /// <param name="targetPosition">Destination in world space.</param>
    /// <param name="speed">Desired movement speed.</param>
    public void MoveTowards(Vector3 targetPosition, float speed)
    {
        if (agent == null) return;

        agent.isStopped = false;
        agent.speed = speed;
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(targetPosition);
    }

    /// <summary>
    /// Stops the NavMeshAgent immediately and clears the current path.
    /// </summary>
    public void StopMoving()
    {
        if (agent == null) return;

        agent.isStopped = true;
        agent.ResetPath();
    }

    /// <summary>
    /// Returns true when the agent has reached its destination (within stopping distance).
    /// Handles pathPending and null agent cases.
    /// </summary>
    public bool HasReachedDestination()
    {
        if (agent == null) return false;
        if (agent.pathPending) return false;

        return agent.remainingDistance <= agent.stoppingDistance;
    }

    #endregion

    #region Patrol Helpers

    /// <summary>
    /// Returns the current patrol point transform or null if no patrol route is configured.
    /// Ensures the patrol index is within bounds.
    /// </summary>
    public Transform GetCurrentPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return null;

        if (currentPatrolIndex < 0 || currentPatrolIndex >= patrolPoints.Length)
            currentPatrolIndex = 0;

        return patrolPoints[currentPatrolIndex];
    }

    /// <summary>
    /// Advance to the next patrol point, wrapping to the first point when the end is reached.
    /// </summary>
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

    #endregion

    #region Targeting Helpers

    /// <summary>
    /// Returns true if the configured target is within the provided range.
    /// Uses world-space distance check.
    /// </summary>
    public bool IsTargetInRange(float range)
    {
        if (target == null) return false;

        return Vector3.Distance(transform.position, target.position) <= range;
    }

    /// <summary>
    /// Rotate the tank to face the configured target horizontally (y ignored).
    /// Useful to orient weapons before firing.
    /// </summary>
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

    #endregion

    #region Weapons

    /// <summary>
    /// Fire the machine gun by spawning a bullet prefab and setting its velocity.
    /// Requires a non-null fire point and bullet prefab.
    /// </summary>
    public void FireMachineGun()
    {
        if (target == null || machineGunBulletPrefab == null || machineGunFirePoint == null)
            return;

        // Aim slightly above target's origin for better hit placement.
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
            // Use linearVelocity to match existing project usage.
            rb.linearVelocity = shotDirection * machineGunBulletSpeed;
        }
    }

    /// <summary>
    /// Fire the cannon by spawning a shell prefab and setting its velocity.
    /// </summary>
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

    #endregion

    #region Health & Damage

    /// <summary>
    /// Forwarding method for IDamage compatibility. Calls the float-based TakeDamage.
    /// Keep the original name if other systems expect it.
    /// </summary>
    public void takeDamage(int amount)
    {
        TakeDamage(amount);
    }

    /// <summary>
    /// Applies damage to the boss. When health reaches zero the boss transitions to the Dead phase and dead state.
    /// </summary>
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

    /// <summary>
    /// Handle death transition: set phase and change to Dead state.
    /// Additional death effects / destruction can be added here.
    /// </summary>
    private void Die()
    {
        currentPhase = TankPhase.Dead;
        stateMachine.ChangeState(DeadState);
    }

    #endregion
}

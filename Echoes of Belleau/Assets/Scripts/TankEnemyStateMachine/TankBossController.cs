using UnityEngine;
using UnityEngine.AI;
//using UnityEngine.SceneManagement;

/// <summary>
/// Controls the boss tank behavior, weapons, health and phase transitions.
/// Uses a simple state machine with patrol, chase (boss fight) and dead states.
/// Implements IDamage so generic weapons can hit it, but can be configured
/// to only take damage from rockets.
/// </summary>
public class TankBossController : MonoBehaviour, IDamage
{
    #region Types

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
    public string playerTag = "Player";
    public Transform target;
    public float detectionRange = 30f;

    [Header("Weapon - Machine Gun")]
    public Transform machineGunFirePoint;
    public GameObject machineGunBulletPrefab;
    public float machineGunFireRate = 0.15f;
    public float machineGunBulletSpeed = 35f;

    [Header("Machine Gun Ammo")]
    public int machineGunMagazineSize = 30;
    public float machineGunReloadTime = 3f;

    [Header("Weapon - Cannon")]
    public Transform cannonFirePoint;
    public GameObject cannonBulletPrefab;
    public float cannonFireRate = 3f;
    public float cannonBulletSpeed = 20f;

    [Header("Health")]
    public float maxHealth = 500f;
    public float currentHealth = 500f;
    public bool destroyOnDeath = false;

    [Header("Damage Rules")]
    public bool onlyTakeRocketDamage = true;

    #endregion

    #region Private State

    private TankStateMachine stateMachine;
    private NavMeshAgent agent;

    private int currentMachineGunAmmo;
    private bool isReloadingMachineGun;
    private float reloadTimer;

    public TankPatrolState PatrolState { get; private set; }
    public TankChaseState ChaseState { get; private set; }
    public TankDeadState DeadState { get; private set; }

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            enabled = false;
            return;
        }

        stateMachine = new TankStateMachine();

        PatrolState = new TankPatrolState(this, stateMachine);
        ChaseState = new TankChaseState(this, stateMachine);
        DeadState = new TankDeadState(this, stateMachine);
    }

    private void Start()
    {
        currentHealth = maxHealth;
        currentMachineGunAmmo = machineGunMagazineSize;
        isReloadingMachineGun = false;
        reloadTimer = 0f;

        FindPlayerTarget();

        if (stateMachine == null)
        {   
            enabled = false;
            return;
        }

        if (currentPhase == TankPhase.Phase1Patrol)
        {
            stateMachine.Initialize(PatrolState);
        }
        else if (currentPhase == TankPhase.Phase2BossFight)
        {
            stateMachine.Initialize(ChaseState);
        }
        else
        {
            stateMachine.Initialize(DeadState);
        }
    }

    private void Update()
    {
        if (currentPhase == TankPhase.Dead)
            return;

        if (target == null)
        {
            FindPlayerTarget();
        }

        if (stateMachine == null)
        {
            enabled = false;
            return;
        }

        UpdateMachineGunReload();
        stateMachine.Update();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }


    #endregion

    #region Phase Management

    public void SetPhase2()
    {
        if (currentPhase != TankPhase.Phase1Patrol)
            return;

        currentPhase = TankPhase.Phase2BossFight;
        stateMachine.ChangeState(ChaseState);
    }

    #endregion

    #region Movement

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

    #endregion

    #region Patrol Helpers

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

    #endregion

    #region Targeting Helpers

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

    #endregion

    #region Weapons

    public void FireMachineGun()
    {
        if (isReloadingMachineGun)
        {
            return;
        }
        if (currentMachineGunAmmo <= 0)
        {
            StartReloadMachineGun();
            return;
        }

        if (target == null )
        {
            return;
        }
        if (machineGunBulletPrefab == null)
        {
            return;
        }
        if (machineGunFirePoint == null)
        {
            return;
        }
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

        currentMachineGunAmmo--;
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

    public void StartReloadMachineGun()
    {
        if (isReloadingMachineGun)
            return;

        isReloadingMachineGun = true;
        reloadTimer = machineGunReloadTime;
    }

    public void UpdateMachineGunReload()
    {
        if (!isReloadingMachineGun)
            return;

        reloadTimer -= Time.deltaTime;

        if (reloadTimer <= 0f)
        {
            isReloadingMachineGun = false;
            currentMachineGunAmmo = machineGunMagazineSize;
        }
    }

    public bool IsReloadingMachineGun()
    {
        return isReloadingMachineGun;
    }

    #endregion

    #region Health & Damage

    /// <summary>
    /// Generic damage entry from IDamage.
    /// If onlyTakeRocketDamage is enabled, normal damage is ignored.
    /// </summary>
    public void takeDamage(int amount)
    {
        if (onlyTakeRocketDamage)
        {
            return;
        }

        TakeDamage(amount);
    }

    /// <summary>
    /// Generic damage application.
    /// Used only when onlyTakeRocketDamage is false.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (currentPhase == TankPhase.Dead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Rocket-only damage entry.
    /// </summary>
    public void ApplyRocketDamage(float amount)
    {
        if (currentPhase == TankPhase.Dead) return;

        if (currentPhase == TankPhase.Phase1Patrol)
        {
            SetPhase2();
        }

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);


        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        currentPhase = TankPhase.Dead;
        stateMachine.ChangeState(DeadState);
        //if (gameManager.instance != null)
        //{
        //    gameManager.instance.LoadSceneWithFade("Outro Scene (placeholder)");
        //}
    }
    private void FindPlayerTarget()
    {
        if (target != null)
            return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            target = playerObject.transform;
        }
    }

    #endregion
}
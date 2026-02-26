using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;

    [SerializeField] int HP;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int FOV; // Field of view angle for detecting the player, still has to be in sphere collider for attack
    [SerializeField] int roamDist;
    [SerializeField] int roamPauseTime;

    [SerializeField] GameObject grenadePrefab;
    [SerializeField] float grenadeCooldown = 6f;
    [SerializeField] float grenadeThrowForce = 12f;
    [SerializeField] float grenadeUpForce = 4f;
    [SerializeField] float grenadeThrowDistance = 10f; // Distance from the player at which the enemy will attempt to throw a grenade


    //[SerializeField] float grenadeMinRange = 6f; // Minimum distance to the player before throwing a grenade
    //[SerializeField] float grenadeMaxRange = 15f; // Maximum distance to the
    [SerializeField, Range(0f, 1f)] float grenadeThrowChance = 0.3f; // Chance to throw a grenade when the player is within range

    [SerializeField] GameObject bullet;
    [SerializeField] float shootRate;
    [SerializeField] Transform shootPos;

    [SerializeField] float reactionTime = 0.25f;
    [SerializeField] float accuracyDegrees = 2.5f;
    [SerializeField] float movingAccuracyPenalty = 2.0f;

    [SerializeField] int magSize = 6; // Number of bullets in a magazine
    [SerializeField] float reloadTime = 1.6f; // Time it takes to reload the weapon

    [SerializeField] float hurtRepositionDistance = 6f;
    [SerializeField] float hurtRepositionChance = 0.6f;

    [SerializeField] float investigateRadius = 2.0f;

    Vector3 lastKnownPlayerPos;
    bool hasLastKnown;

    Color colorOrg;

    float shootTimer;
    float roamTimer;
    float angleToPlayer;
    float stoppingDistOrig;
    float reactionTimer;
    int currentAmmo;
    bool reloading;
    bool isReacting;
    bool playerInTrigger;
    float grenadeTimer;

    GameObject heldGrenade;

    Vector3 playerDir;
    Vector3 startingPos;
    public event Action<EnemyAI> OnDied;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrg = model.material.color;
        stoppingDistOrig = agent.stoppingDistance;
        startingPos = transform.position;
        currentAmmo = magSize;
        grenadeTimer = grenadeCooldown; 
    }

    // Update is called once per frame
    void Update()
    {
        shootTimer += Time.deltaTime;
        grenadeTimer += Time.deltaTime;


        if (agent.remainingDistance < 0.01f)
        {
            roamTimer += Time.deltaTime;
        }

        if (playerInTrigger)
        {
            if (canSeePlayer())
            {
                if (!isReacting)
                {
                    isReacting = true;
                    reactionTimer = reactionTime;
                }

                reactionTimer -= Time.deltaTime;

                if (reactionTimer <= 0f)
                {
                    if (!TryThrowGrenade()) // Attempt to throw a grenade at the player if they are within range and the cooldown has passed
                    {
                        TryShoot(); // Attempt to shoot at the player if they are visible
                    }
                }
            }

            else
            {
                isReacting = false;
                checkRoam(); // If the player is in the trigger but not visible, check if we should roam
            }
        }
        else
        {
            isReacting = false;
            checkRoam(); // If the player is not in the trigger, continue roaming
        }
    }

    void checkRoam()
    {
        if (agent.remainingDistance < 0.01f && roamTimer >= roamPauseTime)
        {
            roam();
        }
    }

    void roam()
    {
        roamTimer = 0;
        agent.stoppingDistance = 0;

        Vector3 ranPos = Random.insideUnitSphere * roamDist;
        ranPos += startingPos;

        NavMeshHit hit;
        NavMesh.SamplePosition(ranPos, out hit, roamDist, 1);
        agent.SetDestination(hit.position);

    }

    bool canSeePlayer()
    {
        playerDir = gameManager.instance.player.transform.position - transform.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        Debug.DrawRay(transform.position, playerDir);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, playerDir, out hit))
        {

            if (angleToPlayer <= FOV && hit.collider.CompareTag("Player"))
            {
                lastKnownPlayerPos = gameManager.instance.player.transform.position;
                hasLastKnown = true;


                agent.stoppingDistance = stoppingDistOrig;
                agent.SetDestination(lastKnownPlayerPos);

                if (agent.remainingDistance < agent.stoppingDistance)
                    faceTarget();

                return true;
            }
        }

        agent.stoppingDistance = 0;
        return false;
    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(playerDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            agent.stoppingDistance = 0;

            if (hasLastKnown)
            {
                Vector3 investigatePoint = GetOffsetPointOnNavmesh(lastKnownPlayerPos, investigateRadius);
                agent.SetDestination(investigatePoint);
            }
        }
    }

    Vector3 GetOffsetPointOnNavmesh(Vector3 center, float radius)
    {

        for (int i = 0; i < 6; i++)
        {
            Vector2 r = Random.insideUnitCircle * radius;
            Vector3 candidate = center + new Vector3(r.x, 0f, r.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, radius, NavMesh.AllAreas))
                return hit.position;
        }

        if (NavMesh.SamplePosition(center, out NavMeshHit centerHit, radius, NavMesh.AllAreas))
            return centerHit.position;

        return center;
    }

    void shoot()
    {
        shootTimer = 0;

        Vector3 aimPoint = gameManager.instance.player.transform.position + Vector3.up * 1.2f; // Aim at the player's chest
        Vector3 dir = (aimPoint - shootPos.position).normalized; // Direction from the shoot position to the aim point

        float spread = accuracyDegrees;
        if (agent.velocity.sqrMagnitude > 0.1f) spread += movingAccuracyPenalty; // Increase spread if the enemy is moving

        Vector3 shotDir = ApplySpread(dir, spread);
        Quaternion rot = Quaternion.LookRotation(shotDir);

        Instantiate(bullet, shootPos.position, rot);

    }

    void TryShoot()
    {
        if (reloading) return;

        if (shootTimer < shootRate) return;

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        shoot();

        currentAmmo--;
    }
    void HoldGrenade()
    {
        heldGrenade = Instantiate(grenadePrefab, shootPos.position, shootPos.rotation);
        heldGrenade.transform.SetParent(shootPos);
        heldGrenade.transform.localPosition = Vector3.zero; // Ensure the grenade is positioned correctly relative to the shootPos
        heldGrenade.transform.localRotation = Quaternion.identity; // Ensure the grenade has no local rotation relative to the shootPos


        Rigidbody rb = heldGrenade.GetComponent<Rigidbody>();
        if ( (rb != null))
        {
            rb.isKinematic = true; // Make the grenade not affected by physics while held
            rb.useGravity = false; // Disable gravity while held
        }

        Collider col = heldGrenade.GetComponent<Collider>();
        if (col != null) col.enabled = false; // Disable the collider while held to prevent collisions with the enemy
    }
    void ThrowHeldGrenade()
    {
        if (heldGrenade == null) return;

        heldGrenade.transform.SetParent(null); // Detach the grenade from the enemy

        Rigidbody rb = heldGrenade.GetComponent<Rigidbody>();
        Collider col = heldGrenade.GetComponent<Collider>();

        if (col != null) col.enabled = true; // Re-enable the collider when thrown
        if (rb != null)
        {
            rb.isKinematic = false; // Make the grenade affected by physics again
            rb.useGravity = true; // Re-enable gravity when thrown

            rb.linearVelocity = Vector3.zero; // Reset velocity to ensure consistent throwing force
            rb.angularVelocity = Vector3.zero;

            Vector3 aimpoint = gameManager.instance.player.transform.position + Vector3.up * 1.2f; // Aim at the player's chest
            Vector3 throwDir = (aimpoint - shootPos.position).normalized;

            damage dmg = heldGrenade.GetComponent<damage>();
            if (dmg != null)
            {
                dmg.Arm(); // enables explode after thrown
            }
                
            Vector3 force = (throwDir * grenadeThrowForce) + (Vector3.up * grenadeUpForce);
            rb.AddForce(force, ForceMode.VelocityChange);
        }
        heldGrenade = null; // Clear the reference to the held grenade
    }
    bool TryThrowGrenade()
    {
        if (grenadePrefab == null || shootPos == null) return false;
        if (grenadeTimer < grenadeCooldown) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, gameManager.instance.player.transform.position);
        if (distanceToPlayer > grenadeThrowDistance) return false;

        if (agent.remainingDistance > agent.stoppingDistance + 1f)
            return false; // Ensure the enemy is close enough to the player before throwing

        if (Random.value > grenadeThrowChance) 
            return false;

        if (heldGrenade == null)
            HoldGrenade();

        ThrowHeldGrenade();
        grenadeTimer = 0f;
        shootTimer = 0f;

        return true;
    }


    Vector3 ApplySpread(Vector3 direction, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        Vector2 rand = Random.insideUnitCircle * Mathf.Tan(radians);

        Quaternion rot = Quaternion.LookRotation(direction);
        Vector3 spreadDir = rot * (Vector3.forward + new Vector3(rand.x, rand.y, 0f));
        return spreadDir.normalized;
    }

    System.Collections.IEnumerator Reload()
    {
        reloading = true;
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = magSize;
        reloading = false;
    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        if (HP > 0 && Random.value < hurtRepositionChance)
        {
            RepositionAwayFromPlayer();
        }

        agent.SetDestination(gameManager.instance.player.transform.position);

        if (HP <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(flashRed());
        }
    }

    void RepositionAwayFromPlayer()
    {
        Vector3 away = (transform.position - gameManager.instance.player.transform.position).normalized; // Direction away from the player
        Vector3 target = transform.position + away * hurtRepositionDistance; // Target position away from the player

        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, hurtRepositionDistance, 1))
        {
            agent.stoppingDistance = 0f;
            agent.SetDestination(hit.position);
            roamTimer = 0f; // Reset roam timer to prevent immediate roaming after repositioning
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrg;
    }

    void Die()
    {
        OnDied?.Invoke(this);
        Destroy(gameObject);
    }
}

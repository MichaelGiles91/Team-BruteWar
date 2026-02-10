using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class enemyAi : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;

    [SerializeField] int HP;
    [SerializeField] int faceTargetSpeed;

    [SerializeField] GameObject bullet;
    [SerializeField] float shootRate;
    [SerializeField] Transform shootPOS; //Needs to be a transform to add position

    float shootTimer;

    bool playerInTrigger;

    Color colorOrig;
    Vector3 playerDir;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;
        gameManager.instance.UpdateGameGoal(1); // part of win condition
    }

    // Update is called once per frame
    void Update()
    {
        shootTimer += Time.deltaTime; 
        playerDir = gameManager.instance.player.transform.position - transform.position; // Get the direction from the enemy to the player by subtracting the enemy's position from the player's position

        if (playerInTrigger)
        {
            // Add a navmesh to the enemy in order to use the NavMeshAgent component, and bake the navmesh in the scene for it to work. Then, set
            // the destination of the NavMeshAgent to the player's position in order for the enemy to follow the player.
            agent.SetDestination(gameManager.instance.player.transform.position); // Set the destination of the NavMeshAgent to the player's position

            if (agent.remainingDistance < agent.stoppingDistance)
                faceTarget(); // If the enemy is within stopping distance of the player, call the faceTarget function to make the enemy look at the player

            if (shootTimer >= shootRate)
            {
                shoot();
            }

        }

    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(playerDir); // Get the rotation needed to look in the direction of the player
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
        // Lerp is important to make your games look polished, as it allows you to smoothly transition between two values. In this case, it allows the enemy to smoothly rotate towards the player instead of snapping to the player's direction.
    }

    private void OnTriggerEnter(Collider other) //detect if player is in spherecollider
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other) // know when player leaves spherecollider
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
        }
    }

    void shoot()
    {
        shootTimer = 0;
        Instantiate(bullet, shootPOS.position, transform.rotation);
    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        if (HP <= 0)
        {
            gameManager.instance.UpdateGameGoal(-1); // part of win condition
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(flashRed());
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);//Always make sure this is a float
        model.material.color = colorOrig;
    }

}

using UnityEngine;
using System.Collections;


public class PlayerController : MonoBehaviour, IDamage

{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [SerializeField] int HP;
    [SerializeField] int speed;
    [SerializeField] int sprintMod;
    [SerializeField] int jumpSpeed;
    [SerializeField] int jumpMax;
    [SerializeField] int gravity;
    
    [SerializeField] int shootDamage;
    [SerializeField] int shootDist;
    [SerializeField] float shootRate;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform shootPos;


    [SerializeField] float stamina;
    [SerializeField] float staminaDrainRate;
    [SerializeField] float staminaRegenRate;
    [SerializeField] float staminaJumpDrain;

    int jumpCount;
    int HPOrig;
    float staminaOrig;
    int speedOrig;
    bool sprintDisable = false;

    float shootTimer;

    Vector3 moveDir;
    Vector3 playerVel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = HP;

        staminaOrig = stamina;
        speedOrig = speed;
        UpdatePlayerUI();
    }

    // Update is called once per frame
    void Update()
    {
        movement();
        sprint();
    }

    void movement()
    {
        shootTimer += Time.deltaTime;

        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.red);

        if (controller.isGrounded)
        {
            playerVel = Vector3.zero;
            jumpCount = 0;
        }

        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);

        jump();
        controller.Move(playerVel * Time.deltaTime);

        playerVel.y -= gravity * Time.deltaTime;

        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
            shoot();
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax && stamina > staminaJumpDrain)
        {

            stamina -= staminaJumpDrain;
            playerVel.y = jumpSpeed;
            jumpCount++;
        } else if (stamina <= staminaJumpDrain)
        {
            // Add some sort of feedback for not being able to jump, like a sound effect or a UI element
        }
    }

    void sprint()
    {
        
        if (Input.GetButton("Sprint") && stamina > 0 && !sprintDisable)
        {
            stamina -= staminaDrainRate * Time.deltaTime;
            speed = speedOrig * sprintMod;

        }
        else if (stamina <= 0)
        {
            speed = speedOrig;
            sprintDisable = true;
            outOfStamina();
            stamina = 1f;
        } else if (stamina < staminaOrig)
        {
            speed = speedOrig;
            stamina += staminaRegenRate * Time.deltaTime;
        } else if (stamina >= staminaOrig)
        {
                sprintDisable = false;
        }
            gameManager.instance.playerStaminaBar.fillAmount = stamina / staminaOrig;
    }

    void shoot()
    {
        shootTimer = 0;
        Instantiate(bullet, shootPos.position, Camera.main.transform.rotation);

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreLayer))
        {
            Debug.Log(hit.collider.name);
            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(shootDamage);
            }
        }
    }
    public void takeDamage(int amount)
    {
        HP -= amount;
        UpdatePlayerUI();

        StartCoroutine(flashScreen());

        if (HP <= 0)
        {
            gameManager.instance.youLose();
        }
    }

    IEnumerator flashScreen()
    {
        gameManager.instance.playerDamageFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.playerDamageFlash.SetActive(false);
    }

    public void UpdatePlayerUI()
    {
        float damageTaken = HPOrig - HP;
        gameManager.instance.playerHPBar.fillAmount = damageTaken / HPOrig;

        gameManager.instance.playerStaminaBar.fillAmount = stamina / staminaOrig;
    }

    public void RespawnReset()
    {
        HP = HPOrig;
        UpdatePlayerUI();

        // clear any falling momentum state
        playerVel = Vector3.zero;
        jumpCount = 0;
    }

    IEnumerator outOfStamina()
    {
        yield return new WaitForSeconds(1f);
    }

}

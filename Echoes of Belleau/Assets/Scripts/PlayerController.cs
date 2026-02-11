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
    [SerializeField] int Stamina;

    [Header("---Combat Stats---")]
    [SerializeField] GameObject bullet;
    [SerializeField] Transform shootPos;
    [SerializeField] float shootRate;
    [SerializeField] int ammoCount;

    int jumpCount;
    int HPOrig;
    int StaminaOrig;
    int ammoCountOrig;
    public int AmmoCount => ammoCount;


    float shootTimer;

    Vector3 moveDir;
    Vector3 playerVel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = HP;
        StaminaOrig = Stamina;
        ammoCountOrig = ammoCount;
        gameManager.instance.updateAmmoAmount(ammoCount);
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
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;
        }
    }

    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
        }
    }

    void shoot()
    {
        shootTimer = 0;
        if (ammoCount > 0)
        {
            Instantiate(bullet, shootPos.position, Camera.main.transform.rotation);
            ammoCount--;
            gameManager.instance.updateAmmoAmount(ammoCount);
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

        gameManager.instance.playerStaminaBar.fillAmount = Stamina / StaminaOrig;
    }

    public void RespawnReset()
    {
        HP = HPOrig;
        UpdatePlayerUI();

        // clear any falling momentum state
        playerVel = Vector3.zero;
        jumpCount = 0;
    }

    public void useStamina(int amount)
    {
        Stamina -= amount;
        UpdatePlayerUI();

        gameManager.instance.playerDamageFlash.SetActive(false);
    }

}
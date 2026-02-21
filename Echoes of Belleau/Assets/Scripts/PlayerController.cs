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

    [Header("---Combat Stats---")]
    [SerializeField] GameObject bullet;
    [SerializeField] Transform shootPos;
    [SerializeField] float shootRate;
    [SerializeField] int ammoCount;
    int ammoCountOrig;
    public int AmmoCount => ammoCount;

    [Header("---Stamina Stats---")]
    [SerializeField] float stamina;
    [SerializeField] float staminaDrainRate;
    [SerializeField] float staminaRegenRate;
    [SerializeField] float staminaJumpDrain;

    [Header("---Stamina Bar Shake---")]
    [SerializeField] float shakeAmount;
    [SerializeField] float shakeDuration;

    int jumpCount;
    int HPOrig;

    float staminaOrig;
    int speedOrig;
    bool sprintDisable = false;
    bool isShaking = false;

    float shootTimer;

    public Animator animator;

    Vector3 moveDir;
    Vector3 playerVel;
    Vector3 StamBarOrigPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = HP;
        staminaOrig = stamina;
        StamBarOrigPos = gameManager.instance.playerStaminaBar.rectTransform.anchoredPosition;
        speedOrig = speed;
        ammoCountOrig = ammoCount;
        gameManager.instance.updateAmmoAmount(ammoCount);

        UpdatePlayerUI();
    }

    // Update is called once per frame
    void Update()
    {
        movement();
        sprint();
        gameManager.instance.updateCompass(transform.eulerAngles.y);
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

        reload();

        updateAnimations();

        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
            shoot();
            
    }

    void updateAnimations()
    {
        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");

        bool isWalkingFwd = vertical > 0.1f && speed <= speedOrig && controller.isGrounded;
        bool isWalkingBck = vertical < -0.1f && speed <= speedOrig && controller.isGrounded;
        bool isWalkingRight = horizontal > 0.1f && speed <= speedOrig && controller.isGrounded;
        bool isWalkingLeft = horizontal < -0.1f && speed <= speedOrig && controller.isGrounded;

        bool isRunningFwd = vertical > 0.1f && speed > speedOrig && controller.isGrounded;
        bool isRunningBck = vertical < -0.1f && speed > speedOrig && controller.isGrounded;
        bool isRunningRight = horizontal > 0.1f && speed > speedOrig && controller.isGrounded;
        bool isRunningLeft = horizontal < -0.1f && speed > speedOrig && controller.isGrounded;

        animator.SetBool("isWalkingFwd", isWalkingFwd);
        animator.SetBool("isWalkingBck", isWalkingBck);
        animator.SetBool("isWalkingLeft", isWalkingLeft);
        animator.SetBool("isWalkingRight", isWalkingRight);

        animator.SetBool("isRunningFwd", isRunningFwd);
        animator.SetBool("isRunningBck", isRunningBck);
        animator.SetBool("isRunningLeft", isRunningLeft);
        animator.SetBool("isRunningRight", isRunningRight);
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (jumpCount < jumpMax && stamina > staminaJumpDrain)
            {
                stamina -= staminaJumpDrain;
                playerVel.y = jumpSpeed;
                jumpCount++;
            }
            else if (stamina <= staminaJumpDrain)
            {
                StartCoroutine(shakeStaminaBar());
            }
        }
    }

    void sprint()
    {

        if (Input.GetButton("Sprint"))
        {
            if (!sprintDisable && stamina > 0f)
            {
                stamina -= staminaDrainRate * Time.deltaTime;
                speed = speedOrig * sprintMod;
            }
            else
            {
                speed = speedOrig;
                if (stamina < staminaOrig)
                    stamina += staminaRegenRate * Time.deltaTime;
                if (!isShaking)
                    StartCoroutine(shakeStaminaBar());


            }
        }
        else
        {
            speed = speedOrig;

            if (stamina < staminaOrig)
                stamina += staminaRegenRate * Time.deltaTime;

            if (stamina >= staminaOrig)
                sprintDisable = false;
        }

        if (stamina <= 0f)
        {
            sprintDisable = true;
            stamina = 0f;
        }
        if (sprintDisable && !isShaking)
        {
            StartCoroutine(shakeStaminaBar());
        }
        gameManager.instance.playerStaminaBar.fillAmount = stamina / staminaOrig;
    }

    void shoot()
    {
        shootTimer = 0;
        if (ammoCount > 0)
        {
            Instantiate(bullet, shootPos.position, Camera.main.transform.rotation);
            ammoCount--;
            gameManager.instance.updateAmmoAmount(ammoCount);
            animator.SetTrigger("Fire");
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

    void reload()
    {
        if (Input.GetButtonDown("Reload") && ammoCount < ammoCountOrig)
        {
            ammoCount = ammoCountOrig;
            gameManager.instance.updateAmmoAmount(ammoCount);
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

    IEnumerator shakeStaminaBar()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeAmount;
            float y = Random.Range(-1f, 1f) * shakeAmount;
            gameManager.instance.playerStaminaBar.rectTransform.anchoredPosition = new Vector3(StamBarOrigPos.x + x, StamBarOrigPos.y + y, StamBarOrigPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        gameManager.instance.playerStaminaBar.rectTransform.anchoredPosition = StamBarOrigPos;
    }

}
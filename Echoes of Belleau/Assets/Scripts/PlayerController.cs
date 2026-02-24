using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour, IDamage, IPickup

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
    [SerializeField] List<gunStats> gunList = new List<gunStats>();
    [SerializeField] int shootDamage;
    [SerializeField] int shootDist;
    [SerializeField] float shootRate;
    [SerializeField] int ammoCount;
    [SerializeField] int medkitCount;
    [SerializeField] int medkitHealAmount = 25;
    int ammoCountOrig;
    int medkitCountOrig;
    public int AmmoCount => ammoCount;
    public int MedkitCount => medkitCount;

    [Header("---Stamina Stats---")]
    [SerializeField] float stamina;
    [SerializeField] float staminaDrainRate;
    [SerializeField] float staminaRegenRate;
    [SerializeField] float staminaJumpDrain;

    [Header("---Stamina Bar Shake---")]
    [SerializeField] float shakeAmount;
    [SerializeField] float shakeDuration;

    [SerializeField] Transform weaponGripTarget;
    [SerializeField] LeftHandIKBinder leftHandIKBinder;
    [SerializeField] UnityEngine.Animations.Rigging.RigBuilder rigBuilder;

    [SerializeField] AudioSource aud;
    int jumpCount;
    int HPOrig;

    float staminaOrig;
    int speedOrig;
    bool sprintDisable = false;
    bool isShaking = false;

    int gunListPos;
    float shootTimer;
    GameObject currentGunInstance;
    Transform activeMuzzle;

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
        medkitCountOrig = medkitCount;
        gameManager.instance.updateMedkitAmount(medkitCount);

        UpdatePlayerUI();
    }

    // Update is called once per frame
    void Update()
    {
        movement();
        sprint();
        gameManager.instance.updateCompass(transform.eulerAngles.y);
        if (Input.GetButtonDown("UseMedkit"))
        {
            UseMedkit();
        }
        selectGun();
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

        shootTimer = 0f;
        ammoCount--;
        gameManager.instance.updateAmmoAmount(ammoCount);

        animator.SetTrigger("Fire");

        if (gunList[gunListPos].shootSound != null && gunList[gunListPos].shootSound.Length > 0)
        {
            AudioClip clip = gunList[gunListPos].shootSound[Random.Range(0, gunList[gunListPos].shootSound.Length)];
            aud.PlayOneShot(clip, gunList[gunListPos].shootSoundVol);
        }

        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreLayer))
        {
            targetPoint = hit.point;

            if (!hit.collider.isTrigger)
            {
                IDamage dmg = hit.collider.GetComponent<IDamage>();
                if (dmg != null)
                    dmg.takeDamage(shootDamage);
            }
        }
        else
        {
            targetPoint = Camera.main.transform.position + Camera.main.transform.forward * shootDist;
        }

        GameObject bulletToFire = gunList[gunListPos].bulletPrefab;

        Vector3 aimDir = (targetPoint - activeMuzzle.position).normalized;
        GameObject newBullet = Instantiate(bulletToFire, activeMuzzle.position, Quaternion.LookRotation(aimDir));

        damage bulletDmg = newBullet.GetComponent<damage>();
        if (bulletDmg != null)
            bulletDmg.SetHitEffect(gunList[gunListPos].hitEffect);
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
    public void ShowGun(bool state)
    {
        if (currentGunInstance != null)
            currentGunInstance.SetActive(state);
    }

    public void getMedkit(int amount)
    {
        medkitCount += amount;
        gameManager.instance.updateMedkitAmount(medkitCount);
    }

    public void getGunStats(gunStats gun)
    {
        gunList.Add(gun);
        gunListPos = gunList.Count - 1;
        changeGun();
    }

    void changeGun()
    {
        shootDamage = gunList[gunListPos].shootDamage;
        shootDist = gunList[gunListPos].shootDist;
        shootRate = gunList[gunListPos].shootRate;

        if (currentGunInstance != null)
            Destroy(currentGunInstance);

        currentGunInstance = Instantiate(gunList[gunListPos].gunModel);

        int fpsLayer = LayerMask.NameToLayer("FPSArms");
        if (fpsLayer >= 0)
            SetLayerRecursively(currentGunInstance, fpsLayer);

        Transform rightHandGrip = currentGunInstance.transform.Find("RightHandGrip");
        if (rightHandGrip == null)
        {
            Debug.LogError($"{currentGunInstance.name} missing RightHandGrip. Cannot equip.");
            Destroy(currentGunInstance);
            return;
        }
        else
        {
            if (weaponGripTarget == null)
            {
                Debug.LogError("weaponGripTarget is not assigned on PlayerController.");
                Destroy(currentGunInstance);
                return;
            }

            AlignWeaponToGrip(currentGunInstance.transform, rightHandGrip, weaponGripTarget);


            currentGunInstance.transform.SetParent(weaponGripTarget, true);
        }


        Transform muzzle = currentGunInstance.transform.Find("Muzzle");
        if (muzzle == null)
        {
            Debug.LogError($"{currentGunInstance.name} missing Muzzle. Shooting will not work.");
            activeMuzzle = null;
        }
        else
        {
            activeMuzzle = muzzle;
        }

        if (leftHandIKBinder != null)
        {
            leftHandIKBinder.BindToWeapon(currentGunInstance);
        }

        if (gunList[gunListPos].overrideController != null)
        {
            animator.runtimeAnimatorController = gunList[gunListPos].overrideController;
        }

        StartCoroutine(RebuildRigNextFrame());
    }

    static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    void selectGun()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
            Debug.Log($"Scroll input | gunListPos: {gunListPos} | gunList.Count: {gunList.Count}");

        if (Input.GetAxis("Mouse ScrollWheel") > 0 && gunListPos < gunList.Count - 1)
        {
            gunListPos++;
            changeGun();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && gunListPos > 0)
        {
            gunListPos--;
            changeGun();
        }
    }

    static void AlignWeaponToGrip(Transform weaponRoot, Transform weaponGripAnchor, Transform gripTarget)
    {
        Quaternion desiredWeaponRotation = gripTarget.rotation * Quaternion.Inverse(weaponGripAnchor.localRotation);

        Vector3 desiredWeaponPosition = gripTarget.position - (desiredWeaponRotation * weaponGripAnchor.localPosition);

        weaponRoot.SetPositionAndRotation(desiredWeaponPosition, desiredWeaponRotation);
    }

    IEnumerator RebuildRigNextFrame()
    {
        yield return null;
        if (rigBuilder != null)
        {
            rigBuilder.Build();
        }
    }


    void UseMedkit()
    {
        if (medkitCount <= 0)
            return;

        if (HP >= HPOrig)
            return;

        medkitCount--;

        HP += medkitHealAmount;
        HP = Mathf.Clamp(HP, 0, HPOrig);

        gameManager.instance.updateMedkitAmount(medkitCount);
        UpdatePlayerUI();
    }
}
using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;

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
    [SerializeField] GameObject bullet;
    [SerializeField] Transform shootPos;
    [SerializeField] List<gunStats> gunList = new List<gunStats>();
    [SerializeField] int shootDamage;
    [SerializeField] int shootDist;
    [SerializeField] float shootRate;
    [SerializeField] GameObject gunModel;
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
    int gunListPos;
    GameObject currentGunInstance;
    Transform activeMuzzle;

    float staminaOrig;
    int speedOrig;
    bool sprintDisable = false;
    bool isShaking = false;

    float shootTimer;

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
        reload();
        gameManager.instance.updateCompass(transform.eulerAngles.y);
    }

    void movement()
    {
        shootTimer += Time.deltaTime;

        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.red);

        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel = Vector3.zero;
        }

        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);

        jump();
        controller.Move(playerVel * Time.deltaTime);

        playerVel.y -= gravity * Time.deltaTime;

        if (Input.GetButton("Fire1") && gunList.Count > 0 && gunList[gunListPos].ammoCur > 0 && shootTimer >= shootRate)
            shoot();

        selectGun();
        reload();
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

        gunList[gunListPos].ammoCur--;
        //aud.PlayOneShot(gunList[gunListPos].shootSound[Random.Range(0, gunList[gunListPos].shootSound.Length)], gunList[gunListPos].shootSoundVol);

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

        GameObject bulletToFire = gunList.Count > 0 && gunList[gunListPos].bulletPrefab != null
            ? gunList[gunListPos].bulletPrefab : bullet;
        Transform spawnPoint = activeMuzzle != null ? activeMuzzle.transform : shootPos;
        Vector3 aimDir = (targetPoint - spawnPoint.position).normalized;
        GameObject newBullet = Instantiate(bulletToFire, spawnPoint.position, Quaternion.LookRotation(aimDir));
        if (gunList.Count > 0)
        {
            damage bulletDmg = newBullet.GetComponent<damage>();
            if (bulletDmg != null)
                bulletDmg.SetHitEffect(gunList[gunListPos].hitEffect);
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
        if (Input.GetButtonDown("Reload") && gunList.Count > 0)
        {
            gunList[gunListPos].ammoCur = gunList[gunListPos].ammoMax;
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

        currentGunInstance = Instantiate(gunList[gunListPos].gunModel, gunModel.transform);
        currentGunInstance.transform.localPosition = Vector3.zero;
        currentGunInstance.transform.localRotation = Quaternion.identity;
        Transform muzzle = currentGunInstance.transform.Find("Muzzle");
        activeMuzzle = muzzle != null ? muzzle : shootPos;

        }

    void selectGun()
    {
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
}
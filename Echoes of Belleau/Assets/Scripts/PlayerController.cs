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
    [SerializeField] int Stamina;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform shootPos;

    [SerializeField] List<gunStats> gunList = new List<gunStats>();
    [SerializeField] int shootDamage;
    [SerializeField] int shootDist;
    [SerializeField] float shootRate;

    [SerializeField] GameObject gunModel;

    [SerializeField] AudioSource aud;
    int jumpCount;
    int HPOrig;
    int StaminaOrig;

    int gunListPos;
    float shootTimer;
    GameObject currentGunInstance;
    Transform activeMuzzle;

    Vector3 moveDir;
    Vector3 playerVel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = HP;

        StaminaOrig = Stamina;
        UpdatePlayerUI();
    }

    // Update is called once per frame
    void Update()
    {
        movement();
        sprint();
        selectGun();
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

        if (gunList.Count > 0 && gunList[gunListPos].shootSound.Length > 0)
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

        GameObject bulletToFire = gunList.Count > 0 && gunList[gunListPos].bulletPrefab != null
            ? gunList[gunListPos].bulletPrefab : bullet;
        Transform spawnPoint = activeMuzzle != null ? activeMuzzle : shootPos;
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
    public void ShowGun(bool state)
    {
        if (currentGunInstance != null)
            currentGunInstance.SetActive(state);
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
}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;


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
    [SerializeField] int ammoMax;
    int ammoCountOrig;

    [SerializeField] int grenadeCount;
    [SerializeField] int grenadeMax;
    int grenadeCountOrig;

    [SerializeField] int medkitCount;
    [SerializeField] int medkitHealAmount = 25;
    int medkitCountOrig;

    public int AmmoCount => ammoCount;
    public int GrenadeCount => grenadeMax;
    public int MedkitCount => medkitCount;

    [Header("--- Low Health Indicator ---")]
    [SerializeField] float lowHealthThreshold = 25f;
    [SerializeField] float pulseSpeed = 2.5f;
    [SerializeField] float minAlpha = 0.2f;
    [SerializeField] float maxAlpha = 0.7f;

    bool lowHealthActive;

    [Header("---Devil Dog Mode")]
    [SerializeField] DevilDogMode devilDogMode;
    
    [Header("---Stamina Stats---")]
    [SerializeField] float stamina;
    [SerializeField] float staminaDrainRate;
    [SerializeField] float staminaRegenRate;
    [SerializeField] float staminaJumpDrain;

    [Header("---Stamina Bar Shake---")]
    [SerializeField] float shakeAmount;
    [SerializeField] float shakeDuration;
    
    [Header("--- Recoil ---")]    
    [SerializeField] float maxRecoilPitch = 20f;
    [SerializeField] float maxRecoilYaw = 6f;
    [SerializeField] float crosshairReturnSpeed = 10f;

    float recoilPitch;
    float recoilYaw;
    float targetRecoilPitch;
    float targetRecoilYaw;

    float currentCrosshairSpread;
    float targetCrosshairSpread;

    [Header("---Stress Stats---")]
    [SerializeField] float stress;
    [SerializeField] float maxStress = 100f;
    [SerializeField] float stressRecoveryDelay = 3f;
    [SerializeField] float stressRecoveryRate = 12f;

    [SerializeField] float damageStressAmount = 20f;
    [SerializeField] float suppressionStressAmount = 12f;
    [SerializeField] float explosionStressAmount = 25f;

    [Header("---Stress Effects---")]
    [SerializeField] float maxSpreadStressPenalty = 0.15f;
    //[SerializeField] float maxMoveAimPenalty = 0.1f;
    [SerializeField] Volume stressVolume;

    [Header("--- Suppression / Cover ---")]
    [SerializeField] LayerMask coverMask;
    [SerializeField] Transform suppressionCheckPoint;
    [SerializeField] float coverCheckHeightOffset = 1.2f;
    [SerializeField] float coverStressRecoveryBonus = 18f;
    [SerializeField] float suppressionGraceTime = 1.5f;
    [SerializeField] float coverRayLength = 6f;

    bool isUnderCover;
    float suppressionTimer;

    float stressSafeTimer;
    public float StressPercent => stress / maxStress;

    [SerializeField] Transform weaponGripTarget;
    [SerializeField] LeftHandIKBinder leftHandIKBinder;
    [SerializeField] UnityEngine.Animations.Rigging.RigBuilder rigBuilder;

    [Header("---Audio--")]
    [SerializeField] AudioSource aud;
    SFXType? currentBreathingLoop = null;
    [SerializeField] float walkStepInterval = 0.45f;
    [SerializeField] float runStepInterval = 0.30f;
    [SerializeField] float footstepResetGrace = 0.12f;

    float footstepTimer = 0f;
    float footstepGraceTimer = 0f;

    int jumpCount;
    int HPOrig;
    float staminaOrig;
    int speedOrig;
    bool sprintDisable = false;
    bool isShaking = false;
    bool isSprinting;
    bool wasSprinting;
    RectTransform stamShakeRect;


    bool wasAirborne;
    float lastYVelocity;
    bool isReloading;


    int gunListPos;
    float shootTimer;
    GameObject currentGunInstance;
    Transform activeMuzzle;
    public bool canShoot = true;
    Coroutine sprintShootDelayRoutine;

    public GameObject knife;
    [SerializeField] KnifeDamage GetKnifeDamage;
    bool canMelee = true;

    [Header("Grenade")]
    [SerializeField] GameObject grenadePrefab;
    [SerializeField] float grenadeCooldown = 6f;
    [SerializeField] float grenadeThrowForce = 12f;
    [SerializeField] float grenadeUpForce = 4f;
    [SerializeField] Transform grenadePos;
    float grenadeTimer;
    GameObject heldGrenade;

    public Animator animator;
    ParticleSystem activeMuzzleFlash;
    Light muzzleLight;
    Coroutine muzzleLightRoutine;

    Vignette stressVignette;
    ChromaticAberration stressChromatic;
    FilmGrain stressFilmGrain;
    DepthOfField stressDepthOfField;
    LensDistortion stressLensDistortion;

    public float CurrentRecoilPitch => recoilPitch;
    public float CurrentRecoilYaw => recoilYaw;

    float moveSlowMult = 1f;
    
    public bool IsMoving
    {
        get
        {
            Vector3 horizontalVel = controller.velocity;
            horizontalVel.y = 0f;
            return horizontalVel.magnitude > 0.1f;
        }
    }

    Vector3 moveDir;
    Vector3 playerVel;
    Vector3 StamBarOrigPos;

    List<GameObject> gunInstances = new List<GameObject>();
    List<bool> gunAvailable = new List<bool>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = HP;
        footstepTimer = 0f;
        footstepGraceTimer = 0f;
        staminaOrig = stamina;
        StamBarOrigPos = gameManager.instance.playerStaminaBar.rectTransform.anchoredPosition;
        speedOrig = speed;
        ammoCountOrig = ammoCount;
        devilDogMode = GetComponent<DevilDogMode>();
        gameManager.instance.updateAmmoAmount(ammoCount, ammoMax);
        grenadeCountOrig = grenadeCount;
        grenadeTimer = grenadeCooldown;
        gameManager.instance.updateGrenadeAmount(grenadeCount);
        medkitCountOrig = medkitCount;
        gameManager.instance.updateMedkitAmount(medkitCount);
        gameManager.instance.UpdateWeaponIcon(null);

        for (int i = 0; i < gunList.Count; i++)
        {
            if (CreateGunInstance(gunList[i], out GameObject instance))
            {
                gunInstances.Add(instance);
                gunAvailable.Add(true);

                if (gunList[i] != null && gunList[i].sourceAsset == null)
                    gunList[i].sourceAsset = gunList[i];
            }
            else
            {
                gunList.RemoveAt(i);
                i--;
            }
        }

        gunListPos = 0;

        if (gunInstances.Count > 0)
        {
            changeGun();
        }

        //stamina bar setup
        stress = 0f;
        stressSafeTimer = 0f;
        if (stressVolume != null && stressVolume.profile != null)
        {
            stressVolume.profile.TryGet(out stressVignette);
            stressVolume.profile.TryGet(out stressChromatic);
            stressVolume.profile.TryGet(out stressFilmGrain);
            stressVolume.profile.TryGet(out stressDepthOfField);
            stressVolume.profile.TryGet(out stressLensDistortion);
        }

        RectTransform fillRect = gameManager.instance.playerStaminaBar.rectTransform;
        stamShakeRect = fillRect.parent as RectTransform;
        if (stamShakeRect == null) stamShakeRect = fillRect;
        StamBarOrigPos = stamShakeRect.localPosition;

        UpdatePlayerUI();

        if (SFXManager.instance != null)
        {
            SFXManager.instance.StopFootstepLoop();
            SFXManager.instance.StopBreathingLoop();
        }

        footstepTimer = 0f;
        currentBreathingLoop = null;
    }

    
    void Update()
    {
        movement();
        sprint();
        UpdateStress();
        UpdateBreathingAudio();
        UpdateFootstepAudio();
        ApplyRecoil();
        UpdateCrosshair();
        gameManager.instance.updateCompass(transform.eulerAngles.y);
        grenadeTimer += Time.deltaTime;
        UpdateLowHealthIndicator();
        if (Input.GetButtonDown("ThrowGrenade"))
        {
            HoldGrenade();
            UseGrenade();
        }
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
        controller.Move((speed * moveSlowMult) * Time.deltaTime * moveDir);

        jump();
        controller.Move(playerVel * Time.deltaTime);

        lastYVelocity = playerVel.y;
        playerVel.y -= gravity * Time.deltaTime;

        reload();

        updateAnimations();

        HandleLandingSound();

        if (Input.GetButton("Fire1") && shootTimer >= shootRate && !isSprinting && canShoot)
            shoot();
        Melee();

    }

    void updateAnimations()
    {
        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");
        float currentMoveSpeed = speed * moveSlowMult;

        bool isWalkingFwd = vertical > 0.1f && currentMoveSpeed <= speedOrig && controller.isGrounded;
        bool isWalkingBck = vertical < -0.1f && currentMoveSpeed <= speedOrig && controller.isGrounded;
        bool isWalkingRight = horizontal > 0.1f && currentMoveSpeed <= speedOrig && controller.isGrounded;
        bool isWalkingLeft = horizontal < -0.1f && currentMoveSpeed <= speedOrig && controller.isGrounded;

        bool isRunningFwd = vertical > 0.1f && currentMoveSpeed > speedOrig && controller.isGrounded;
        bool isRunningBck = vertical < -0.1f && currentMoveSpeed > speedOrig && controller.isGrounded;
        bool isRunningRight = horizontal > 0.1f && currentMoveSpeed > speedOrig && controller.isGrounded;
        bool isRunningLeft = horizontal < -0.1f && currentMoveSpeed > speedOrig && controller.isGrounded;

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
                if (SFXManager.instance != null)
                {
                    SFXManager.instance.PlayJump();
                }
                jumpCount++;
            }
            else if (stamina <= staminaJumpDrain)
            {
                TryShakeStaminaBar();
            }
        }
    }

    void sprint()
    {
        bool currentlySprinting = false;

        bool sprintHeld = Input.GetButton("Sprint");

        if (sprintHeld && !sprintDisable && stamina > 0f)
        {
            currentlySprinting = true;
            stamina -= staminaDrainRate * Time.deltaTime;
            speed = speedOrig * sprintMod;
        }
        else
        {
            currentlySprinting = false;
            speed = speedOrig;

            if (stamina < staminaOrig)
            {
                stamina += staminaRegenRate * Time.deltaTime;
            }

            if (stamina >= staminaOrig)
            {
                stamina = staminaOrig;
                sprintDisable = false;
            }
        }

        if (wasSprinting && !currentlySprinting)
        {
            if (sprintShootDelayRoutine != null)
                StopCoroutine(sprintShootDelayRoutine);

            sprintShootDelayRoutine = StartCoroutine(SprintShootDelay());
        }

        wasSprinting = currentlySprinting;
        isSprinting = currentlySprinting;

        if (stamina <= 0f)
        {
            stamina = 0f;
            isSprinting = false;
            sprintDisable = true;
            TryShakeStaminaBar();
        }

        if (sprintDisable)
            TryShakeStaminaBar();

        gameManager.instance.playerStaminaBar.fillAmount = stamina / staminaOrig;
    }

    void shoot()
    {
        if (ammoCount <= 0 && (devilDogMode == null || !devilDogMode.isActive)) 
        {
            if (gunList[gunListPos].emptyShotSound != null && gunList[gunListPos].emptyShotSound.Length > 0)
            {
                AudioClip clip = gunList[gunListPos].emptyShotSound[Random.Range(0, gunList[gunListPos].emptyShotSound.Length)];
                aud.PlayOneShot(clip, gunList[gunListPos].emptyShotSoundVol);
            }
            return; 
        }

        gameManager.instance.TriggerCombat();

        shootTimer = 0f;
        if (devilDogMode == null || !devilDogMode.isActive)
        {
            ammoCount--;
            SaveAmmoToGunStats();
            gameManager.instance.updateAmmoAmount(ammoCount, ammoMax);
        }

        if (activeMuzzleFlash != null)
        {
            activeMuzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            activeMuzzleFlash.Play(true);
            if (muzzleLight != null)
            {
                if (muzzleLightRoutine != null)
                    StopCoroutine(muzzleLightRoutine);

                muzzleLightRoutine = StartCoroutine(FlashMuzzleLight());
            }
        }

        if (gunList[gunListPos].shootSound != null && gunList[gunListPos].shootSound.Length > 0)
        {
            AudioClip clip = gunList[gunListPos].shootSound[Random.Range(0, gunList[gunListPos].shootSound.Length)];
            aud.PlayOneShot(clip, gunList[gunListPos].shootSoundVol);
        }

        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreLayer, QueryTriggerInteraction.Ignore))
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

        float currentStressPenalty = maxSpreadStressPenalty * StressPercent;
        float weaponSpray = gunList[gunListPos].sprayAmount;
        float totalSpray = weaponSpray + currentStressPenalty;

        aimDir.x += Random.Range(-totalSpray, totalSpray);
        aimDir.y += Random.Range(-totalSpray, totalSpray);
        aimDir.z += Random.Range(-totalSpray * 0.35f, totalSpray * 0.35f);
        aimDir.Normalize();

        GameObject newBullet = Instantiate(bulletToFire, activeMuzzle.position, Quaternion.LookRotation(aimDir));

        damage bulletDmg = newBullet.GetComponent<damage>();
        if (bulletDmg != null)
            bulletDmg.SetHitEffect(gunList[gunListPos].hitEffect);

        AddRecoil();
        CheckSingleUseWeaponAfterShot();
    }

    public void takeDamage(int amount)
    {

        if (devilDogMode != null && devilDogMode.isActive)
        {
            return;
        }

        HP -= amount;

        AddStress(damageStressAmount);
        UpdatePlayerUI();

        StartCoroutine(flashScreen());

        gameManager.instance.TriggerCombat(8f);

        if (HP <= 0)
        {
            if (SFXManager.instance != null)
            {
                SFXManager.instance.StopBreathingLoop();
                SFXManager.instance.StopFootstepLoop();
                SFXManager.instance.PlayDeath();
            }

            currentBreathingLoop = null;
            footstepTimer = 0f;

            gameManager.instance.youLose();
        }
        else if (SFXManager.instance != null)
        {
            SFXManager.instance.PlayHurt();
        }
    }

    void reload()
    {
        if (isReloading) return;
        if (!Input.GetButtonDown("Reload")) return;

        int magSize = ammoCountOrig;
        if (ammoCount >= magSize) return;
        if (ammoMax <= 0) return;

        StartCoroutine(ReloadWait(gunList[gunListPos].reloadTime));
    }

    IEnumerator ReloadWait(float reloadTime)
    {
        isReloading = true;

        if (gunList[gunListPos].reloadSound != null && gunList[gunListPos].reloadSound.Length > 0)
        {
            AudioClip clip = gunList[gunListPos].reloadSound[Random.Range(0, gunList[gunListPos].reloadSound.Length)];
            aud.PlayOneShot(clip, gunList[gunListPos].reloadSoundVol);
        }

        yield return new WaitForSeconds(reloadTime);

        int magSize = ammoCountOrig;
        int need = magSize - ammoCount;
        int load = Mathf.Min(need, ammoMax);

        ammoCount += load;
        ammoMax -= load;

        SaveAmmoToGunStats();
        gameManager.instance.updateAmmoAmount(ammoCount, ammoMax);

        isReloading = false;
    }

    IEnumerator flashScreen()
    {
        gameManager.instance.playerDamageFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.playerDamageFlash.SetActive(false);
    }

    void UpdateLowHealthIndicator()
    {
        Image indicator = gameManager.instance.lowHealthIndicator;

        if (HP <= lowHealthThreshold)
        {
            if (!lowHealthActive)
            {
                indicator.gameObject.SetActive(true);
                lowHealthActive = true;
            }

            Color c = indicator.color;

            float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            c.a = Mathf.Lerp(minAlpha, maxAlpha, pulse);

            indicator.color = c;
        }
        else
        {
            if (lowHealthActive)
            {
                indicator.gameObject.SetActive(false);
                lowHealthActive = false;
            }
        }
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
        stress = 0f;
        stressSafeTimer = 0f;
        UpdatePlayerUI();
        footstepTimer = 0f;
        footstepGraceTimer = 0f;

        playerVel = Vector3.zero;
        jumpCount = 0;

        if (SFXManager.instance != null)
        {
            SFXManager.instance.StopBreathingLoop();
            SFXManager.instance.StopFootstepLoop();
        }

        currentBreathingLoop = null;
        footstepTimer = 0f;
    }

    void TryShakeStaminaBar()
    {
        if (!isShaking)
            StartCoroutine(shakeStaminaBar());
    }

    IEnumerator shakeStaminaBar()
    {
        isShaking = true;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeAmount;
            float y = Random.Range(-1f, 1f) * shakeAmount;

            stamShakeRect.localPosition = StamBarOrigPos + new Vector3(x, y, 0f);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        stamShakeRect.localPosition = StamBarOrigPos;
        isShaking = false;
    }

    public void ShowGun(bool state)
    {
        if (currentGunInstance != null)
            currentGunInstance.SetActive(state);
    }

    public void getGrenade(int amount)
    {
        grenadeCount += amount;
        gameManager.instance.updateGrenadeAmount(grenadeCount);
    }

    public void getMedkit(int amount)
    {
        medkitCount += amount;
        gameManager.instance.updateMedkitAmount(medkitCount);
        if (!gameManager.instance.HasShownTutorial("MedKitPickup"))
        {
            gameManager.instance.ShowTutorial("MedKitPickup", "Medkits", "You will find Medkits scattered around the level. If you have one available, press H to use the Medkit and heal half your total health. Press Esc to continue.");
        }
    }

    public bool getGunStats(gunStats gun)
    {
        if (gun == null) return false;

        int existingIndex = FindGunIndexBySourceAsset(gun);

        if (gun.uniquePickup && existingIndex != -1)
        {
            if (gunAvailable[existingIndex])
            {
                return false;
            }

            gunStats existingGun = gunList[existingIndex];

            gunAvailable[existingIndex] = true;
            existingGun.ammoCur = existingGun.magSize;
            existingGun.ammoMax = existingGun.ammoMaxOrig;

            gunListPos = existingIndex;
            changeGun();
            return true;
        }

        gunStats newGunStats = Instantiate(gun);
        newGunStats.sourceAsset = gun;
        newGunStats.ammoCur = newGunStats.magSize;

        gunList.Add(newGunStats);

        if (!CreateGunInstance(newGunStats, out GameObject instance))
        {
            gunList.RemoveAt(gunList.Count - 1);
            return false;
        }

        gunInstances.Add(instance);
        gunAvailable.Add(true);

        gunListPos = gunList.Count - 1;
        changeGun();
        return true;
    }

    void SaveAmmoToGunStats()
    {
        if (gunList == null || gunList.Count == 0) return;
        gunList[gunListPos].ammoCur = ammoCount;
        gunList[gunListPos].ammoMax = ammoMax;
    }

    public void PickedUpAmmo()
    {
        if (gunList == null || gunList.Count == 0) return;

        for (int i = 0; i < gunList.Count; i++)
        {
            if (gunList[i] == null) continue;

            if (!gameManager.instance.HasShownTutorial("AmmoPickup"))
            {
                gameManager.instance.ShowTutorial(
                    "AmmoPickup"
                    ,"Ammo"
                    ,"You will find Ammo Crates scattered around the level. When you walk up to these crates, it will automatically add one magazine worth of ammo to your currently equiped weapon's reserves and one grenade. Press Esc to continue."
                    );
            }

            if (!gunList[i].singleUseWeapon)
            {
                gunList[i].ammoMax += gunList[i].pickupSize;
                gunList[i].ammoMax = Mathf.Min(gunList[i].ammoMax, gunList[i].ammoMaxOrig);
            }
        }

        grenadeCount++;
        ammoMax = gunList[gunListPos].ammoMax;
        gameManager.instance.updateAmmoAmount(ammoCount, ammoMax);
        gameManager.instance.updateGrenadeAmount(grenadeCount);
    }

    void changeGun()
    {
        if (gunList.Count == 0 || gunInstances.Count == 0 || gunAvailable.Count == 0)
        {
            currentGunInstance = null;
            return;
        }

        if (gunListPos < 0 || gunListPos >= gunList.Count || gunListPos >= gunInstances.Count || gunListPos >= gunAvailable.Count)
        {
            return;
        }

        if (!gunAvailable[gunListPos])
        {
            int fallbackIndex = FindNextAvailableGunIndex(gunListPos, 1);
            if (fallbackIndex == -1)
            {
                currentGunInstance = null;
                activeMuzzle = null;
                activeMuzzleFlash = null;
                muzzleLight = null;
                ammoCount = 0;
                ammoMax = 0;

                gameManager.instance.updateAmmoAmount(0, 0);
                gameManager.instance.UpdateWeaponIcon(null);

                if (leftHandIKBinder != null)
                    leftHandIKBinder.BindToWeapon(null);

                return;
            }

            gunListPos = fallbackIndex;
        }

        shootDamage = gunList[gunListPos].shootDamage;
        shootDist = gunList[gunListPos].shootDist;
        shootRate = gunList[gunListPos].shootRate;
        ammoCount = gunList[gunListPos].ammoCur;
        ammoMax = gunList[gunListPos].ammoMax;
        ammoCountOrig = gunList[gunListPos].magSize;

        recoilPitch = 0f;
        recoilYaw = 0f;
        targetRecoilPitch = 0f;
        targetRecoilYaw = 0f;
        currentCrosshairSpread = 0f;
        targetCrosshairSpread = 0f;

        if (gameManager.instance != null)
            gameManager.instance.SetCrosshairSpread(0f);

        if (gunList[gunListPos].equipSound != null && gunList[gunListPos].equipSound.Length > 0)
        {
            AudioClip clip = gunList[gunListPos].equipSound[Random.Range(0, gunList[gunListPos].equipSound.Length)];
            aud.PlayOneShot(clip, gunList[gunListPos].equipSoundVol);
        }


        gameManager.instance.updateAmmoAmount(ammoCount, ammoMax);

        foreach (var gun in gunInstances)
            gun.SetActive(false);

        currentGunInstance = gunInstances[gunListPos];
        currentGunInstance.SetActive(true);

        foreach (Collider col in currentGunInstance.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        int fpsLayer = LayerMask.NameToLayer("Fps Arms");
        if (fpsLayer >= 0)
            SetLayerRecursively(currentGunInstance, fpsLayer);

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

        activeMuzzleFlash = null;

        ParticleSystem flashPrefab = gunList[gunListPos].muzzleFlash;
        if (flashPrefab != null && activeMuzzle != null)
        {
            activeMuzzleFlash = Instantiate(flashPrefab, activeMuzzle);
            activeMuzzleFlash.transform.localPosition = Vector3.zero;
            activeMuzzleFlash.transform.localRotation = Quaternion.identity;
            activeMuzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            muzzleLight = null;

            if (activeMuzzleFlash != null)
            {
                muzzleLight = activeMuzzleFlash.GetComponentInChildren<Light>(true);
                if (muzzleLight != null)
                    muzzleLight.enabled = false;
            }
        }

        if (leftHandIKBinder != null)
        {
            leftHandIKBinder.BindToWeapon(currentGunInstance);
        }

        if (gunList[gunListPos].overrideController != null)
        {
            animator.runtimeAnimatorController = gunList[gunListPos].overrideController;
        }

        if (gameManager.instance != null)
            gameManager.instance.UpdateWeaponIcon(gunList[gunListPos].weaponIcon);

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
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            int nextIndex = FindNextAvailableGunIndex(gunListPos, 1);
            if (nextIndex != -1 && nextIndex != gunListPos)
            {
                SaveAmmoToGunStats();
                gunListPos = nextIndex;
                changeGun();
            }
        }
        else if (scroll < 0f)
        {
            int nextIndex = FindNextAvailableGunIndex(gunListPos, -1);
            if (nextIndex != -1 && nextIndex != gunListPos)
            {
                SaveAmmoToGunStats();
                gunListPos = nextIndex;
                changeGun();
            }
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

    void HoldGrenade()
    {
        if (grenadeCount <= 0)
            return;

            heldGrenade = Instantiate(grenadePrefab, grenadePos.position, grenadePos.rotation);
            heldGrenade.transform.SetParent(grenadePos);
            heldGrenade.transform.localPosition = Vector3.zero; // Ensure the grenade is positioned correctly relative to the shootPos
            heldGrenade.transform.localRotation = Quaternion.identity; // Ensure the grenade has no local rotation relative to the shootPos


            Rigidbody rb = heldGrenade.GetComponent<Rigidbody>();
            if ((rb != null))
            {
                rb.isKinematic = true; // Make the grenade not affected by physics while held
                rb.useGravity = false; // Disable gravity while held
            }

            Collider col = heldGrenade.GetComponent<Collider>();
            if (col != null) col.enabled = false; // Disable the collider while held to prevent collisions with the enemy
    }

    void UseGrenade()
    {
        if (grenadeCount <= 0)
            return;

        grenadeCount--;

        if (heldGrenade == null)
        {
            return;
        }

        heldGrenade.transform.SetParent(null);

        Rigidbody rb = heldGrenade.GetComponent<Rigidbody>();
        Collider col = heldGrenade.GetComponent<Collider>();

        if (col != null) col.enabled = true;
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 aimpoint = Camera.main.transform.position + Camera.main.transform.forward;
            Vector3 throwDir = (aimpoint - grenadePos.position).normalized;

            damage dmg = heldGrenade.GetComponent<damage>();
            if (dmg != null)
            {
                dmg.Arm();
            }

            Vector3 force = (throwDir * grenadeThrowForce) + (Vector3.up * grenadeUpForce);
            rb.AddForce(force, ForceMode.VelocityChange);
        }
        heldGrenade = null;

        gameManager.instance.updateGrenadeAmount(grenadeCount);
        UpdatePlayerUI();
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

        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlayMedkit();
        }

        gameManager.instance.updateMedkitAmount(medkitCount);
        UpdatePlayerUI();
    }

    IEnumerator FlashMuzzleLight()
    {
        muzzleLight.enabled = true;
        muzzleLight.intensity = Random.Range(5f, 8f);

        yield return new WaitForSeconds(0.05f);

        muzzleLight.enabled = false;
    }
    public void GetDogTag()
    {
        if(devilDogMode != null)
        {
            devilDogMode.AddDogTagPoints();
        }
    }

    IEnumerator SprintShootDelay()
    {
        canShoot = false;
        yield return new WaitForSeconds(0.6f);
        canShoot = true;
    }


    public int GetHP()
    {
        return HP;
    }

    public void SetHP(int HPPoints)
    {
        HP = HPPoints;
        UpdatePlayerUI();
    }

    public void SetAmmo(int AmmoValue)
    {
        ammoCount = AmmoValue;
        gameManager.instance.updateAmmoAmount(ammoCount,ammoMax);
    }

    public void SetMedkits(int Medkits)
    {
        medkitCount = Medkits;
        gameManager.instance.updateMedkitAmount(medkitCount);
    }

    public float GetDevilDogPoints()
    {
        return devilDogMode.GetCurrentPoints();
    }

    public void SetDevilDogPoints(float dogpoints)
    {
        devilDogMode.SetCurrentPoints(dogpoints);
    }

    public int GetAmmoMax()
    {
        return ammoMax;
    }

    public int GetGrenades()
    {
        return grenadeCount;
    }

    public void SetGrenades(int grenades)
    {
        grenadeCount = grenades;
        gameManager.instance.updateGrenadeAmount(grenadeCount);
    }

    bool CreateGunInstance(gunStats gun, out GameObject instance)
    {
        instance = Instantiate(gun.gunModel);

        Transform rightHandGrip = instance.transform.Find("RightHandGrip");
        if (rightHandGrip == null)
        {
            Debug.LogError($"{instance.name} missing RightHandGrip.");
            Destroy(instance);
            instance = null;
            return false;
        }

        AlignWeaponToGrip(instance.transform, rightHandGrip, weaponGripTarget);
        instance.transform.SetParent(weaponGripTarget, true);

        int fpsLayer = LayerMask.NameToLayer("Fps Arms");
        if (fpsLayer != -1)
        {
            SetLayerRecursively(instance, fpsLayer);
        }

        foreach (Collider col in instance.GetComponentsInChildren<Collider>(true))
        {
            col.enabled = false;
        }

        instance.SetActive(false);
        return true;
    }

    public void RestoreWeaponsWithoutReinstantiating(List<WeaponCheckpointData> savedWeapons, int savedGunIndex)
    {
        if (savedWeapons == null)
            return;

        SaveAmmoToGunStats();

        // Hide all current weapon instances first
        foreach (GameObject gunObj in gunInstances)
        {
            if (gunObj != null)
                gunObj.SetActive(false);
        }

        List<gunStats> restoredGunList = new List<gunStats>();
        List<GameObject> restoredInstances = new List<GameObject>();
        List<bool> restoredAvailability = new List<bool>();

        for (int i = 0; i < savedWeapons.Count; i++)
        {
            WeaponCheckpointData saved = savedWeapons[i];
            if (saved == null || saved.gunRef == null)
                continue;

            int existingIndex = FindGunIndexByReference(saved.gunRef);

            if (existingIndex != -1)
            {
                gunStats existingGun = gunList[existingIndex];
                GameObject existingInstance = gunInstances[existingIndex];

                existingGun.ammoCur = saved.ammoCur;
                existingGun.ammoMax = saved.ammoMax;

                restoredGunList.Add(existingGun);
                restoredInstances.Add(existingInstance);
                restoredAvailability.Add(existingGun.ammoCur > 0 || !existingGun.singleUseWeapon);
            }
        }

        gunList = restoredGunList;
        gunInstances = restoredInstances;
        gunAvailable = restoredAvailability;

        if (gunList.Count == 0 || gunInstances.Count == 0)
        {
            gunListPos = 0;
            currentGunInstance = null;
            activeMuzzle = null;
            activeMuzzleFlash = null;
            muzzleLight = null;
            gameManager.instance.UpdateWeaponIcon(null);
            gameManager.instance.updateAmmoAmount(0, 0);
            return;
        }

        gunListPos = Mathf.Clamp(savedGunIndex, 0, gunList.Count - 1);
        changeGun();
    }

    int FindGunIndexByReference(gunStats targetGun)
    {
        for (int i = 0; i < gunList.Count; i++)
        {
            if (gunList[i] == targetGun)
                return i;
        }

        return -1;
    }

    public List<WeaponCheckpointData> GetWeaponCheckpointData()
    {
        SaveAmmoToGunStats();

        List<WeaponCheckpointData> data = new List<WeaponCheckpointData>();

        foreach (gunStats gun in gunList)
        {
            if (gun == null) continue;

            WeaponCheckpointData entry = new WeaponCheckpointData();
            entry.gunRef = gun;
            entry.ammoCur = gun.ammoCur;
            entry.ammoMax = gun.ammoMax;

            data.Add(entry);
        }

        return data;
    }

    public int GetSelectedGunIndex()
    {
        return gunListPos;
    }

    public void SetMoveSlow(float multiplier)
    {
        moveSlowMult = multiplier;
        
    }

    public void ResetMoveSlow()
    {
        moveSlowMult = 1f;
        
    }

    void UpdateStress()
    {
        if (suppressionTimer > 0f)
        {
            suppressionTimer -= Time.deltaTime;
        }
        else
        {
            isUnderCover = false;
        }

        if (stressSafeTimer > 0f)
        {
            stressSafeTimer -= Time.deltaTime;
        }
        else if (stress > 0f)
        {
            float recoveryRate = stressRecoveryRate;

            if (isUnderCover)
                recoveryRate += coverStressRecoveryBonus;

            stress -= recoveryRate * Time.deltaTime;
            stress = Mathf.Clamp(stress, 0f, maxStress);
        }

        UpdateStressEffects();
    }

    public void AddStress(float amount)
    {
        stress += amount;

        if (!gameManager.instance.HasShownTutorial("StressSystem"))
        {
            gameManager.instance.ShowTutorial("StressSystem", "Stress System", "Taking damage increases stress. The higher your stress, the more blurry your vision gets and the more your accuracy is reduced. Press Esc to continue.");
        }

        stress = Mathf.Clamp(stress, 0f, maxStress);
        stressSafeTimer = stressRecoveryDelay;
    }

    public void AddSuppressionStress()
    {
        AddStress(suppressionStressAmount);
    }

    public void AddExplosionStress()
    {
        AddStress(explosionStressAmount);
    }

    void UpdateStressEffects()
    {
        float t = StressPercent;

       
        float strongT = t * t;

        if (stressVignette != null)
        {
            stressVignette.intensity.value = Mathf.Lerp(0.12f, 0.5f, strongT);
            stressVignette.smoothness.value = Mathf.Lerp(0.2f, 0.75f, strongT);
        }

        if (stressChromatic != null)
        {
            stressChromatic.intensity.value = Mathf.Lerp(0f, 0.65f, strongT);
        }

        if (stressFilmGrain != null)
        {
            stressFilmGrain.intensity.value = Mathf.Lerp(0f, 0.5f, strongT);
        }

        if (stressLensDistortion != null)
        {
            stressLensDistortion.intensity.value = Mathf.Lerp(0f, -0.28f, strongT);
            stressLensDistortion.scale.value = Mathf.Lerp(1f, 0.92f, strongT);
        }

        if (stressDepthOfField != null)
        {
            stressDepthOfField.mode.value = DepthOfFieldMode.Gaussian;
            stressDepthOfField.gaussianStart.value = Mathf.Lerp(12f, 3f, strongT);
            stressDepthOfField.gaussianEnd.value = Mathf.Lerp(40f, 8f, strongT);
            stressDepthOfField.gaussianMaxRadius.value = Mathf.Lerp(0.3f, 1.6f, strongT);
        }
    }

    void Melee()
    {
        if (Input.GetButtonDown("Melee") && canMelee)
        {
            StartCoroutine(MeleeDelay());
            knife.SetActive(true);
            animator.SetTrigger("Melee");
            GetKnifeDamage.DoKnifeHit();
            StartCoroutine(gunHide());
            StartCoroutine(knifeHideDelay());
        }
    }

    IEnumerator knifeHideDelay()
    {
        yield return new WaitForSeconds(.3f);
        knife.SetActive(false);
    }

    IEnumerator MeleeDelay()
    {
        canMelee = false;
        yield return new WaitForSeconds(0.6f);
        canMelee = true;
    }

    IEnumerator gunHide()
    {
        currentGunInstance.SetActive(false);
        yield return new WaitForSeconds(.3f);
        currentGunInstance.SetActive(true);
    }
    void HandleLandingSound()
    {
        if (!controller.isGrounded)
        {
            wasAirborne = true;
            return;
        }

        if (controller.isGrounded && wasAirborne)
        {
            if (lastYVelocity < -2f)
            {
                if (SFXManager.instance != null)
                {
                    SFXManager.instance.PlayLand();
                }
            }

            wasAirborne = false;
        }
    }

    void SetBreathingLoop(SFXType? newType)
    {
        if (currentBreathingLoop == newType)
            return;

        currentBreathingLoop = newType;

        if (SFXManager.instance == null)
            return;

        if (newType == null)
        {
            SFXManager.instance.StopBreathingLoop();
            return;
        }

        SFXManager.instance.PlayBreathingLoop(newType.Value);
    }

    void UpdateBreathingAudio()
    {
        if (sprintDisable || stamina <= 0f)
        {
            SetBreathingLoop(SFXType.BreathingOutOfBreathLoop);
            return;
        }

        if (isSprinting && HasMoveInput())
        {
            SetBreathingLoop(SFXType.BreathingRunLoop);
            return;
        }

        SetBreathingLoop(SFXType.BreathingIdleLoop);
    }

    void UpdateFootstepAudio()
    {
        bool movingInput = HasMoveInput();
        bool groundedEnough = controller.isGrounded || playerVel.y <= 0.1f;
        bool running = isSprinting && !sprintDisable && stamina > 0f;

        if (groundedEnough && movingInput)
        {
            footstepGraceTimer = footstepResetGrace;
            footstepTimer += Time.deltaTime;
        }
        else
        {
            footstepGraceTimer -= Time.deltaTime;

            if (footstepGraceTimer <= 0f)
            {
                footstepTimer = 0f;
                return;
            }
        }

        float stepInterval = running ? runStepInterval : walkStepInterval;

        if (footstepTimer >= stepInterval)
        {
            footstepTimer -= stepInterval;

            if (SFXManager.instance != null)
            {
                if (running)
                    SFXManager.instance.PlayRunFootstep();
                else
                    SFXManager.instance.PlayWalkFootstep();
            }
        }
    }

    bool HasMoveInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        return horizontal != 0f || vertical != 0f;
    }

    void CheckSingleUseWeaponAfterShot()
    {
        if (gunList.Count == 0) return;
        if (gunListPos < 0 || gunListPos >= gunList.Count) return;

        gunStats currentGun = gunList[gunListPos];
        if (currentGun == null) return;

        if (currentGun.singleUseWeapon && ammoCount <= 0)
        {
            DeactivateCurrentGun();
        }
    }

    void DeactivateCurrentGun()
    {
        if (gunList.Count == 0 || gunInstances.Count == 0 || gunAvailable.Count == 0) return;
        if (gunListPos < 0 || gunListPos >= gunList.Count || gunListPos >= gunInstances.Count || gunListPos >= gunAvailable.Count) return;

        gunStats oldGunStats = gunList[gunListPos];

        gunAvailable[gunListPos] = false;
        oldGunStats.ammoCur = 0;
        oldGunStats.ammoMax = 0;

        int nextGunIndex = FindNextAvailableGunIndex(gunListPos, 1);

        if (nextGunIndex == -1)
        {
            foreach (var gun in gunInstances)
            {
                if (gun != null)
                    gun.SetActive(false);
            }

            currentGunInstance = null;
            activeMuzzle = null;
            activeMuzzleFlash = null;
            muzzleLight = null;
            ammoCount = 0;
            ammoMax = 0;

            gameManager.instance.updateAmmoAmount(0, 0);
            gameManager.instance.UpdateWeaponIcon(null);

            if (leftHandIKBinder != null)
                leftHandIKBinder.BindToWeapon(null);

            return;
        }

        gunListPos = nextGunIndex;
        changeGun();
    }

    int FindGunIndexBySourceAsset(gunStats gunAsset)
    {
        for (int i = 0; i < gunList.Count; i++)
        {
            if (gunList[i] == null) continue;

            if (gunList[i].sourceAsset == gunAsset)
                return i;
        }

        return -1;
    }

    int FindNextAvailableGunIndex(int startIndex, int direction)
    {
        if (gunAvailable.Count == 0) return -1;

        int index = startIndex;

        for (int i = 0; i < gunAvailable.Count; i++)
        {
            index += direction;

            if (index >= gunAvailable.Count)
                index = 0;
            else if (index < 0)
                index = gunAvailable.Count - 1;

            if (gunAvailable[index])
                return index;
        }

        return -1;
    }

    void AddRecoil()
    {
        if (gunList.Count == 0 || gunListPos < 0 || gunListPos >= gunList.Count) return;

        gunStats currentGun = gunList[gunListPos];

        targetRecoilPitch += currentGun.verticalRecoil;
        targetRecoilYaw += Random.Range(-currentGun.horizontalRecoil, currentGun.horizontalRecoil);

        targetRecoilPitch = Mathf.Clamp(targetRecoilPitch, 0f, maxRecoilPitch);
        targetRecoilYaw = Mathf.Clamp(targetRecoilYaw, -maxRecoilYaw, maxRecoilYaw);

        targetCrosshairSpread += currentGun.crosshairKick;
    }

    void ApplyRecoil()
    {
        if (gunList.Count == 0 || gunListPos < 0 || gunListPos >= gunList.Count) return;

        gunStats currentGun = gunList[gunListPos];

        recoilPitch = Mathf.Lerp(recoilPitch, targetRecoilPitch, Time.deltaTime * 18f);
        recoilYaw = Mathf.Lerp(recoilYaw, targetRecoilYaw, Time.deltaTime * 18f);

        targetRecoilPitch = Mathf.Lerp(targetRecoilPitch, 0f, Time.deltaTime * currentGun.recoilResetSpeed);
        targetRecoilYaw = Mathf.Lerp(targetRecoilYaw, 0f, Time.deltaTime * currentGun.recoilResetSpeed);
    }

    void UpdateCrosshair()
    {
        if (gunList.Count == 0 || gunListPos < 0 || gunListPos >= gunList.Count)
            return;

        targetCrosshairSpread = Mathf.Lerp(targetCrosshairSpread, 0f, Time.deltaTime * crosshairReturnSpeed);
        currentCrosshairSpread = Mathf.Lerp(currentCrosshairSpread, targetCrosshairSpread, Time.deltaTime * 20f);

        if (gameManager.instance != null)
            gameManager.instance.SetCrosshairSpread(currentCrosshairSpread);
    }

    public void TryApplySuppression(Vector3 threatPosition)
    {
        suppressionTimer = suppressionGraceTime;

        bool covered = IsThreatBlockedByCover(threatPosition);

        if (covered)
        {
            isUnderCover = true;
            return;
        }

        isUnderCover = false;
        AddSuppressionStress();
    }

    bool IsThreatBlockedByCover(Vector3 threatPosition)
    {
        Vector3 checkOrigin;

        if (suppressionCheckPoint != null)
            checkOrigin = suppressionCheckPoint.position;
        else
            checkOrigin = transform.position + Vector3.up * coverCheckHeightOffset;

        Vector3 dirToThreat = threatPosition - checkOrigin;
        float dist = dirToThreat.magnitude;

        if (dist <= 0.01f)
            return false;

        dirToThreat.Normalize();

        if (Physics.Raycast(checkOrigin, dirToThreat, out RaycastHit hit, Mathf.Min(dist, coverRayLength), coverMask, QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        return false;
    }
}
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour, IDamage
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [SerializeField] int HP;
    [SerializeField] int Speed;
    [SerializeField] int SprintMod;
    [SerializeField] int JumpSpeed;
    [SerializeField] int JumpMax;
    [SerializeField] int gravity;

    [SerializeField] int shootDamage;
    [SerializeField] int shootDist;
    [SerializeField] float shootRate;

    int jumpCount;
    int HPOrig;
    float shootTimer;

    Vector3 moveDir;
    Vector3 playerVel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = HP;
        updatePlayerUI();


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

        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist);
        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel = Vector3.zero;
        }
        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * Speed * Time.deltaTime);

        Jump();
        controller.Move(playerVel * Time.deltaTime);

        playerVel.y -= gravity * Time.deltaTime;

        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
        {
            shoot();
        }

    }
    void Jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < JumpMax)
        {
            playerVel.y = JumpSpeed;
            jumpCount++;
        }
    }
    void sprint()
    {
        if (Input.GetButtonDown("sprint"))
        {
            Speed *= SprintMod;
        }
        else if (Input.GetButtonUp("sprint"))
        {
            Speed /= SprintMod;
        }
    }
    void shoot()
    {
        shootTimer = 0;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist))
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
        updatePlayerUI();
        StartCoroutine(flashScreen());

        if (HP <= 0)
        {
            gameManager.instance.youLose();
        }
    }

    IEnumerator flashScreen()
    {
        gameManager.instance.PlayerDamageFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.PlayerDamageFlash.SetActive(false);
    }

    public void updatePlayerUI()
    {
        if (gameManager.instance.playerHPBar != null)
        {
            gameManager.instance.playerHPBar.fillAmount = (float)HP / HPOrig;
        }
    }
    public void RespawnReset()
    {
        HP = HPOrig;
        updatePlayerUI();

        // clear any falling momentum state
        playerVel = Vector3.zero;
        jumpCount = 0;
    }
}

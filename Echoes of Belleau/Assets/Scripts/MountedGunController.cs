using UnityEngine;

public class MountedGunController : MonoBehaviour , IInteractable, IHighlightable
{
    [Header("Gun Stats")]
    [SerializeField] float fireRate = 0.08f;
    [SerializeField] int damage = 30;
    [SerializeField] float shootDist = 150f;
    [SerializeField] float sensitivity = 2f;

    [Header("Rotation Limits")]
    [SerializeField] float minYaw = -90f;
    [SerializeField] float maxYaw = 90f;
    [SerializeField] float minPitch = -25f;
    [SerializeField] float maxPitch = 5f;

    [Header("Scene References")]
    [SerializeField] Transform gunPivot;
    [SerializeField] Transform barrelPivot;
    [SerializeField] Transform firePoint;
    [SerializeField] Transform cameraPoint;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] LayerMask ignoreLayer;

    [Header("Highlight")]
    [SerializeField] Renderer gunRenderer;
    [SerializeField] Material normalMat;
    [SerializeField] Material highlightMat;

    bool isMounted;
    float shootTimer;
    float currentYaw;
    float currentPitch;

    PlayerController playerCtrl;
    cameraController camCtrl;
    PlayerInteraction playerInteract;

    Camera mainCam;
    Transform camOriginalParent;
    Vector3 camOriginalLocalPos;
    Quaternion camOriginalLocalRot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCam = Camera.main;
    }

    public void Interact()
    {
        if (!isMounted)
        {
            Mount();
        }
    }
    public void Highlight(bool state)
    {
        if(gunRenderer && normalMat && highlightMat)
        {
            gunRenderer.material = state ? highlightMat : normalMat;
        }
    }
    void Mount()
    {
        Transform playerRoot = mainCam.transform.root;
        playerCtrl = playerRoot.GetComponent<PlayerController>();
        camCtrl = mainCam.GetComponent<cameraController>();
        playerInteract = playerRoot.GetComponentInChildren<PlayerInteraction>();

        playerCtrl.enabled = false;
        camCtrl.enabled = false;
        playerInteract.enabled = false;

        camOriginalParent = mainCam.transform.parent;
        camOriginalLocalPos = mainCam.transform.localPosition;
        camOriginalLocalRot = mainCam.transform.localRotation;

        mainCam.transform.SetParent(cameraPoint);
        mainCam.transform.localPosition = Vector3.zero;
        mainCam.transform.localRotation = Quaternion.identity;

        currentYaw = gunPivot.localEulerAngles.y;
        currentPitch = barrelPivot != null ? barrelPivot.localEulerAngles.x : 0f;

        playerCtrl.ShowGun(false);

        isMounted = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Dismount()
    {
        isMounted = false;

        mainCam.transform.SetParent(camOriginalParent);
        mainCam.transform.localPosition = camOriginalLocalPos;
        mainCam.transform.localRotation = camOriginalLocalRot;

        playerCtrl.enabled = true;
        camCtrl.enabled = true;
        playerInteract.enabled = true;
        playerCtrl.ShowGun(true);

    }
    // Update is called once per frame
    void Update()
    {
        if (!isMounted) { return; }
        if (gameManager.instance.isPaused) { return; }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Dismount();
            return;
        }
        HandleRotation();
        HandleShooting();
    }
    void HandleRotation()
    {
        currentYaw += Input.GetAxisRaw("Mouse X") * sensitivity;
        currentPitch -= Input.GetAxisRaw("Mouse Y") * sensitivity;

        currentYaw = Mathf.Clamp(currentYaw, minYaw, maxYaw);
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        gunPivot.localRotation = Quaternion.Euler(0f, currentYaw, 0f);

        if(barrelPivot != null)
        {
            barrelPivot.localRotation = Quaternion.Euler(currentPitch,0f,0f);

        }
    }
    void HandleShooting()
    {
        shootTimer -= Time.deltaTime;
        if (Input.GetButton("Fire1") && shootTimer <= 0f)
        {
            Fire();
            shootTimer = fireRate;

        }
    }
    void Fire()
    {
        Debug.Log("Fire() called");

        if(Physics.Raycast(mainCam.transform.position, mainCam.transform.forward, out RaycastHit hit, shootDist, ~ignoreLayer))
        {
            if (!hit.collider.isTrigger)
                hit.collider.GetComponent<IDamage>()?.takeDamage(damage);
        }

        Debug.Log("bulletPrefab: " + bulletPrefab + "  firePoint: " + firePoint);
        Debug.Log("FirePoint forward direction: " + firePoint.forward);
        Debug.DrawRay(firePoint.position, firePoint.forward * 20f, Color.red, 3f);
        if(bulletPrefab && firePoint)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Debug.Log("Bullet instantiated at " + firePoint.position);
        }
    }
}

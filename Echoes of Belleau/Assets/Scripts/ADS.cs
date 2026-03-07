using KevinIglesias;
using UnityEngine;

public class ADS : MonoBehaviour
{
    [SerializeField] Transform weaponRoot;
    [SerializeField] Transform hipPosition;
    [SerializeField] Transform adsPosition;


    public Camera playerCamera;
    public float normalFov = 60f;
    public float adsFOV = 40f;
    public bool isADS;
    public float ADSspeed = 8f;

    void Start()
    {
        playerCamera = Camera.main;
    }

    public void HandleInput()
    {
        isADS = Input.GetButton("ADS");
    }

    public void UpdateWeaponPosition()
    {
        Transform target;

        if (isADS)
        {
            target = adsPosition;
        }
        else
        {
            target = hipPosition;
        }

        weaponRoot.localPosition = Vector3.Lerp(weaponRoot.localPosition, target.localPosition, ADSspeed * Time.deltaTime);
        weaponRoot.localRotation = Quaternion.Lerp(weaponRoot.localRotation, target.localRotation, ADSspeed * Time.deltaTime);
    }
    public void UpdateCamFOV()
    {
        float targetFOV;

        if (isADS)
        {
            targetFOV = adsFOV;
        }
        else
        {
            targetFOV = normalFov;
        }
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, ADSspeed * Time.deltaTime);
    }
    void Update()
     {
        HandleInput();
        UpdateWeaponPosition();
        UpdateCamFOV();
     }
}

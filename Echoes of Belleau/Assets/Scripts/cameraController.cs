using UnityEngine;

public class cameraController : MonoBehaviour
{
    [SerializeField] int sens;
    [SerializeField] int lockVertMin, lockVertMax;
    [SerializeField] bool invertY;
    [SerializeField] PlayerController player;

    float camRotX;
    float yawRotY;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sens * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sens * Time.deltaTime;

        if (invertY)
            camRotX += mouseY;
        else
            camRotX -= mouseY;

        camRotX = Mathf.Clamp(camRotX, lockVertMin, lockVertMax);

        yawRotY += mouseX;

        float recoilPitch = 0f;
        float recoilYaw = 0f;

        if (player != null)
        {
            recoilPitch = player.CurrentRecoilPitch;
            recoilYaw = player.CurrentRecoilYaw;
        }

        transform.localRotation = Quaternion.Euler(camRotX - recoilPitch, recoilYaw, 0f);
        transform.parent.localRotation = Quaternion.Euler(0f, yawRotY, 0f);
    }
}
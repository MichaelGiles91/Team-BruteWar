using UnityEngine;

public class dynamicCrosshair : MonoBehaviour
{
    private RectTransform reticle;

    [SerializeField] float restingSize = 100;
    [SerializeField] float walkMaxSize = 300;
    [SerializeField] float sprintMaxSize = 400;
    [SerializeField] float speed = 1;

    [Header("Jump Expansion")]
    [SerializeField] float jumpSizeBoost = 300f;
    [SerializeField] float jumpHoldTime = 0.35f;

    private float currentSize;
    private float jumpBoostTimer;

    private void Start()
    {
        reticle = GetComponent<RectTransform>();
        currentSize = restingSize;
    }

    private void Update()
    {
        
        if (Input.GetButtonDown("Jump"))
        {
            jumpBoostTimer = jumpHoldTime;
        }
        jumpBoostTimer -= Time.deltaTime;

        float targetSize = restingSize;

        if (isSprinting)
        {
            targetSize = sprintMaxSize;
        }
        else if (isMoving)
        {
            targetSize = walkMaxSize;
        }


        if (jumpBoostTimer > 0f)
            targetSize += jumpSizeBoost;

        currentSize = Mathf.Lerp(currentSize, targetSize, Time.deltaTime * speed);

        reticle.sizeDelta = new Vector2(currentSize, currentSize);
    }


    bool isMoving
    {
        get
        {
            return
                Input.GetAxis("Horizontal") != 0 ||
                Input.GetAxis("Vertical") != 0 ||
                Input.GetAxis("Mouse X") != 0 ||
                Input.GetAxis("Mouse Y") != 0;
        }
    }

    bool isSprinting
    {
        get
        {
            return Input.GetKey(KeyCode.LeftShift) && isMoving;
        }
    }
}

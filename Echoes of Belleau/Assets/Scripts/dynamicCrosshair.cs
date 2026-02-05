using UnityEngine;

public class dynamicCrosshair : MonoBehaviour
{
    private RectTransform reticle;

    [SerializeField] float restingSize;
    [SerializeField] float maxSize;
    [SerializeField] float speed;

    [Header("Jump Expansion")]
    [SerializeField] float jumpSizeBoost = 20f;
    [SerializeField] float jumpHoldTime = 0.15f;

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

        float targetSize = isMoving ? maxSize : restingSize;

        
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
}

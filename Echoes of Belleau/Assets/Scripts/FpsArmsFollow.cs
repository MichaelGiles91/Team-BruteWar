using UnityEngine;

public class FPSArmsFollow : MonoBehaviour
{
    [SerializeField] Transform armsMount;
    [SerializeField] float positionLerp;
    [SerializeField] float rotationLerp;

    void Start()
    {
        if (!armsMount) return;
        transform.SetPositionAndRotation(armsMount.position, armsMount.rotation);
    }

    void LateUpdate()
    {
        if (!armsMount) return;

        transform.position = Vector3.Lerp(transform.position, armsMount.position, 1f - Mathf.Exp(-positionLerp * Time.deltaTime));

        transform.rotation = Quaternion.Slerp(transform.rotation, armsMount.rotation, 1f - Mathf.Exp(-rotationLerp * Time.deltaTime));
    }
}
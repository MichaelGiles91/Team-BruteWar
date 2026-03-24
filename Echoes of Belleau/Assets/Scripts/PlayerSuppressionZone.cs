using UnityEngine;

public class PlayerSuppressionZone : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [SerializeField] float suppressionCooldown = 0.1f;

    float nextSuppressionTime;

    private void OnTriggerEnter(Collider other)
    {
        if (player == null) return;

        if (((1 << other.gameObject.layer) & LayerMask.GetMask("enemy bullet")) == 0)
            return;

        if (Time.time < nextSuppressionTime)
            return;

        nextSuppressionTime = Time.time + suppressionCooldown;

        player.TryApplySuppression(other.transform.position);
    }
}
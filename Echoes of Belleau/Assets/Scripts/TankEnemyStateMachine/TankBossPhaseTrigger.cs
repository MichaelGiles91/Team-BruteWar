using UnityEngine;

public class TankBossPhaseTrigger : MonoBehaviour
{
    [SerializeField] private TankBossController tankBoss;

    private void OnTriggerEnter(Collider other)
    {
        if (tankBoss == null)
            return;

        if (!other.CompareTag("Player"))
            return;

        tankBoss.SetPhase2();

        gameObject.SetActive(false);
    }
}
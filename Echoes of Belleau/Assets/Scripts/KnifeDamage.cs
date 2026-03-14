using System.Collections.Generic;
using UnityEngine;

public class KnifeDamage : MonoBehaviour
{
    [Header("--- Damage ---")]
    [SerializeField] int damageAmount;

    [Header("--- Hit Detection ---")]
    [SerializeField] Transform hitPoint;
    [SerializeField] Vector3 hitBoxSize = new Vector3(1f, 1f, 1.5f);
    [SerializeField] LayerMask hitMask;
    [SerializeField] QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("--- Optional FX ---")]
    [SerializeField] ParticleSystem hitEffect;

    public void DoKnifeHit()
    {
        if (hitPoint == null)
        {
            Debug.LogWarning("KnifeDamage: hitPoint is not assigned.");
            return;
        }

        Collider[] hits = Physics.OverlapBox(hitPoint.position, hitBoxSize * 0.5f, hitPoint.rotation, hitMask, triggerInteraction);

        HashSet<IDamage> damagedTargets = new HashSet<IDamage>();

        for (int i = 0; i < hits.Length; i++)
        {
            IDamage dmg = hits[i].GetComponentInParent<IDamage>();
            if (dmg == null) continue;
            if (damagedTargets.Contains(dmg)) continue;

            damagedTargets.Add(dmg);
            dmg.takeDamage(damageAmount);

            if (hitEffect != null)
            {
                Instantiate(hitEffect, hits[i].ClosestPoint(hitPoint.position), Quaternion.identity);
            }
        }
    }
}
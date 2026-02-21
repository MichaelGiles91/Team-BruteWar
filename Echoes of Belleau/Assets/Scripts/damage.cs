using UnityEngine;
using System.Collections;

public class damage : MonoBehaviour
{
    enum damageType { bullet, stationary, DOT, explosive }

    [SerializeField] damageType type;
    [SerializeField] Rigidbody rd;

    [SerializeField] int damageAmount;
    [SerializeField] float damageRate;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;
    [SerializeField] ParticleSystem hitEffect;

    [SerializeField] float fuseTime = 2.5f;
    [SerializeField] float explosionRadius = 4f;
    [SerializeField] LayerMask explosionMask = ~0;
    [SerializeField] ParticleSystem explosionEffect;

    bool isDamaging;
    bool armed;
    bool fuseStarted;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (type == damageType.bullet)
        {
            rd.linearVelocity = transform.forward * speed;
            Destroy(gameObject, destroyTime);

        }
        //else if (type == damageType.explosive)
        //{
         //   Invoke(nameof(Explode), fuseTime);
        //}
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            return;
        }

        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg != null && type != damageType.DOT)
        {
            dmg.takeDamage(damageAmount);
        }
        if (type == damageType.bullet)
        {
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
        if (type == damageType.explosive) // If it's an explosive, we want to explode on impact, regardless of whether it hit something that can take damage or not
        {
            if (armed)
            {
                Explode();
            }

            return;
        }

    }
    public void Arm()
    {
        if (type != damageType.explosive) return;
        if (fuseStarted) return;

        fuseStarted = true;
        armed = true;

        Invoke(nameof(Explode), fuseTime);
    }

    void Explode()
    {
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, explosionMask);

        for (int i = 0; i < hits.Length; i++)
        {
            IDamage dmg = hits[i].GetComponent<IDamage>();
            if (dmg != null)
                dmg.takeDamage(damageAmount);
        }

        Destroy(gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger)
        {
            return;
        }

        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg != null && type == damageType.DOT && !isDamaging)
        {
            StartCoroutine(damageOther(dmg));
        }
    }

    IEnumerator damageOther(IDamage d)
    {
        isDamaging = true;
        d.takeDamage(damageAmount);
        yield return new WaitForSeconds(damageRate);
        isDamaging = false;
    }
}
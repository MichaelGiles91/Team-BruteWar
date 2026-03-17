using UnityEngine;
using System.Collections;

public class damage : MonoBehaviour
{
    enum damageType { bullet, stationary, DOT, explosive }

    [SerializeField] damageType type;
    [SerializeField] int damageAmount;
    [SerializeField] float damageRate;

    [Header("Bullet")]
    [SerializeField] int speed;
    [SerializeField] int destroyTime;
    [SerializeField] ParticleSystem hitEffect;
    [SerializeField] Rigidbody rb;

    [Header("Explosion")]
    [SerializeField] float fuseTime = 2.5f;
    [SerializeField] float explosionRadius = 4f;
    [SerializeField] LayerMask explosionMask = ~0;
    [SerializeField] ParticleSystem explosionEffect;

    [Header("DOT")]
    [SerializeField] bool onlyDamageWhileMoving = false;
    [SerializeField] float slowMult = 0.5f;

    bool isDamaging;
    bool armed;
    bool fuseStarted;

    public void SetHitEffect(ParticleSystem effect)
    {
        hitEffect = effect;
    }
    void Start()
    {
        if (type == damageType.bullet)
        {
            rb.linearVelocity = transform.forward * speed;
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
        if (other.isTrigger) return;

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
        if (type == damageType.DOT)
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.SetMoveSlow(slowMult);
            }
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
            if (onlyDamageWhileMoving)
            {
                PlayerController pc = other.GetComponent<PlayerController>();
                if (pc == null || !pc.IsMoving)
                {
                    return;
                }
            }

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

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger) return;

        if (type == damageType.DOT)
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.ResetMoveSlow();
            }
        }
    }

}
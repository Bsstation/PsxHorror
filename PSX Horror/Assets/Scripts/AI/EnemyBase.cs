using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyBase : LifeBase
{
    [Header("Enemy Value")]
    public Transform target;
    public Transform aimTarget;

    protected Animator anim;
    protected NavMeshAgent agent;
    protected CharacterController controller;
    protected AudioSource audioSource;

    public float hitFactor;
    public float reactionDamage;
    public bool canCritical;

    protected bool inGround;

    public bool chasing;

    [Header("Stats")]
    public float damage;

    [System.Serializable]
    public class FOV
    {
        public float maxRadious;
        public float maxAngle;
        public bool isInFov;
    }

    public FOV fov;

    public bool falled;

    private void OnEnable()
    {
        chasing = false;
    }

    public override void OnDamage(float damage)
    {
        if (!falled && anim && !inGround)
            hitFactor += damage;

        if (!chasing)
            chasing = true;

        DamageExtra();
    }

    public abstract void DamageExtra();

    public override void OnDie()
    {
        Dying();
    }

    // Start is called before the first frame update
    public void NewStart()
    {
        Start();

        if (GetComponent<Animator>())
            anim = GetComponent<Animator>();
        else if (transform.parent.GetComponentInChildren<Animator>())
            anim = transform.parent.GetComponentInChildren<Animator>();

        if(GetComponent<NavMeshAgent>())
            agent = GetComponent<NavMeshAgent>();
        else if (transform.parent.GetComponentInChildren<NavMeshAgent>())
            agent = transform.parent.GetComponentInChildren<NavMeshAgent>();

        if(GetComponent<CharacterController>())
            controller = GetComponent<CharacterController>();
        else if (transform.parent.GetComponentInChildren<NavMeshAgent>())
            controller = transform.parent.GetComponentInChildren<CharacterController>();

        if (GameObject.FindGameObjectWithTag("Player"))
            target = GameObject.FindGameObjectWithTag("Player").transform;

        if (GetComponent<AudioSource>())
            audioSource = GetComponent<AudioSource>();
        else if (transform.parent.GetComponentInChildren<AudioSource>())
            audioSource = transform.parent.GetComponentInChildren<AudioSource>();
    }

    // Update is called once per frame
    public void Update()
    {
        if (!dead)
        {
            HitCount();
            Behaviour();
        }
    }

    public static bool isFOV(Transform checkObject, Transform target, float maxAngle, float maxRadious, LayerMask layer)
    {
        Collider[] hitColliders = Physics.OverlapSphere(checkObject.position, maxRadious);

        foreach (Collider col in hitColliders)
        {
            if (col.transform == target)
            {
                Vector3 direction = col.transform.position - checkObject.position;
                float angle = Vector3.Angle(direction, checkObject.forward);

                if (angle <= maxAngle)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(checkObject.position + checkObject.up, direction.normalized, out hit, maxRadious, layer))
                    {
                        return true;
                    }
                }

            }
        }

        return false;
    }

    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fov.maxRadious);

        Vector3 fovline1 = Quaternion.AngleAxis(fov.maxAngle, transform.up) * transform.forward * fov.maxRadious;
        Vector3 fovline2 = Quaternion.AngleAxis(-fov.maxAngle, transform.up) * transform.forward * fov.maxRadious;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, fovline1);
        Gizmos.DrawRay(transform.position, fovline2);

        if (!fov.isInFov)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.green;

        if (target)
        {
            Gizmos.DrawRay(transform.position, (target.position - transform.position).normalized * fov.maxRadious);
        }

        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, transform.forward * fov.maxRadious);
    }
    */

    bool react = false;

    void HitCount()
    {
        hitFactor = (hitFactor > 0) ? hitFactor -= Time.deltaTime : 0;
        if (react && hitFactor <= reactionDamage * 0.5f)
        {
            react = false;
        }

        if(hitFactor > reactionDamage && hitFactor <= reactionDamage * 1.5f)
        {
            if (react == false)
            {
                anim.CrossFade("Hit", 0.1f);
                react = true;
            }
        }else if(hitFactor > reactionDamage * 1.5f)
        {
            if (react)
            {
                HeavyReaction();
                hitFactor = 0;
                react = false;
            }
        }
    }

    public abstract void Behaviour();
    public abstract void Attack();

    public abstract void Dying();

    public abstract void HeavyReaction();

    public abstract void CriticalReaction(float damage, Vector3 pos);
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion;

public enum TypeEnemy { Stone, Gun, Knife, Wood, Hand}

public class RagdollEnemy : EnemyBase
{
    [System.Serializable]
    public struct Weapons
    {
        public TypeEnemy typeEnemy;

        public GameObject stone;
        public GameObject gun;
        public GameObject knife;
        public GameObject wood;

        public Transform stoneSpawner;
        public Transform bulletSpawner;

        public AudioClip shotClip;
    }

    [Header("AI")]
    public Weapons weapons;

    [Space]
    public float attackTime;
    public float attackRate;
    public bool canAttack;

    public RootMotion.Dynamics.PuppetMaster puppetMaster;

    bool noMove, fallTime;
    float forward;

    bool aiming;

    LaunchBehaviour launch;
    public ParticleSystem[] muzzleFlash;
    Transform meleeReceiver;

    new void Start()
    {
        NewStart();

        launch = gameObject.AddComponent<LaunchBehaviour>();
        launch.testing = false;
        launch.height = 0.05f;

        hipsForward = Quaternion.Inverse(puppetMaster.muscles[0].transform.rotation) * puppetMaster.targetRoot.forward;
        hipsUp = Quaternion.Inverse(puppetMaster.muscles[0].transform.rotation) * puppetMaster.targetRoot.up;

        meleeReceiver = transform.parent.GetChild(3);

        InitWeapons();
    }

    new void Update()
    {
        if (!dead)
        {
            inGround = (!anim.IsInTransition(0) && anim.GetCurrentAnimatorStateInfo(0).IsTag("Fall")) ||
                (anim.IsInTransition(0) && anim.GetAnimatorTransitionInfo(0).IsUserName("Fall"));

            noMove = (!anim.IsInTransition(0) && anim.GetCurrentAnimatorStateInfo(0).IsTag("NoMove")) ||
                (anim.IsInTransition(0) && anim.GetAnimatorTransitionInfo(0).IsUserName("NoMove"));

            aiming = (!anim.IsInTransition(0) && anim.GetCurrentAnimatorStateInfo(0).IsTag("Aim")) ||
                (anim.IsInTransition(0) && anim.GetAnimatorTransitionInfo(0).IsUserName("Aim"));

            fov.isInFov = isFOV(transform, target, fov.maxAngle, fov.maxRadious, FXController.instance.layers.enemyCheckForPlayer);

            if (fov.isInFov) chasing = true;

            attackTime = (attackTime > 0) ? attackTime -= Time.deltaTime : attackTime = 0;
            canAttack = attackTime == 0;

            if (falled)
            {
                puppetMaster.muscleWeight = Mathf.Lerp(puppetMaster.muscleWeight, 0.01f, 1 * Time.deltaTime);
                puppetMaster.pinWeight = Mathf.Lerp(puppetMaster.pinWeight, 0, 1 * Time.deltaTime);

                Vector3 pos = new Vector3(aimTarget.position.x, transform.position.y, aimTarget.transform.position.z);
                meleeReceiver.position = pos;
            }
            else
            {
                puppetMaster.muscleWeight = Mathf.Lerp(puppetMaster.muscleWeight, 1, 0.5f * Time.deltaTime);
                puppetMaster.pinWeight = Mathf.Lerp(puppetMaster.pinWeight, 1, 0.5f * Time.deltaTime);

                meleeReceiver.position = transform.position;
            }
        }

        base.Update();
    }

    #region Weapons
    void InitWeapons()
    {
        weapons.stone.SetActive(false);
        weapons.gun.SetActive(false);
        weapons.wood.SetActive(false);
        weapons.knife.SetActive(false);

        switch (weapons.typeEnemy)
        {
            case TypeEnemy.Gun:
                weapons.gun.SetActive(true);
                break;
            case TypeEnemy.Stone:
                weapons.stone.SetActive(true);
                break;
            case TypeEnemy.Knife:
                weapons.knife.SetActive(true);
                break;
            case TypeEnemy.Wood:
                weapons.wood.SetActive(true);
                break;
            case TypeEnemy.Hand:
                break;
        }
    }

    //bhv
    void StoneBhv()
    {
        if (target && chasing && !target.GetComponent<LifeBase>().dead)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            agent.SetDestination(target.position);
            anim.ResetTrigger("180");

            if (!inGround && !falled && !noMove && !fallTime)
            {
                Vector3 direction = target.position - transform.position;
                float angle = Vector3.Angle(direction, transform.forward);

                if (angle <= 140)
                {
                    if (canAttack)
                    {
                        if (distance >= 5f)
                            forward = 2;
                        else if (distance < 5f && distance > 4f)
                            forward = 1;
                        else if (distance <= 4f)
                        {
                            forward = 0f;
                            if (!anim.GetCurrentAnimatorStateInfo(3).IsName("Hit") && 
                                isFOV(transform, target, 60, 4f, FXController.instance.layers.enemyCheckForPlayer))
                                Attack();
                            else
                                forward = 1;
                        }
                        agent.isStopped = false;
                    }
                    else
                    {
                        if (distance >= 3f)
                            forward = 1;
                        else if (distance < 3f && distance > 2f)
                            forward = 0f;
                        else if (distance <= 2f)
                            forward = -1f;
                        agent.isStopped = false;
                    }
                }
                else
                    anim.SetTrigger("180");
            }
            else
            {
                forward = 0;
                agent.isStopped = true;
            }
        }
        else
        {
            forward = 0;
            agent.isStopped = true;
        }
    }
    void GunBhv()
    {
        if (target && chasing && !target.GetComponent<LifeBase>().dead)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            agent.SetDestination(target.position);
            anim.ResetTrigger("180");

            if (!inGround && !falled && !noMove && !fallTime)
            {
                Vector3 direction = target.position - transform.position;
                float angle = Vector3.Angle(direction, transform.forward);

                if (angle <= 140)
                {
                    if (canAttack)
                    {
                        if (distance >= 6f)
                            forward = 2;
                        else if (distance < 6f && distance > 4f)
                            forward = 1;
                        else if (distance <= 4f)
                        {
                            forward = 0f;
                            if (!anim.GetCurrentAnimatorStateInfo(3).IsName("Hit") &&
                                isFOV(transform, target, 60, 4f, FXController.instance.layers.enemyCheckForPlayer))
                                Attack();
                            else
                                forward = 1;
                        }
                        agent.isStopped = false;
                    }
                    else
                    {
                        if (distance >= 3f)
                            forward = 1;
                        else if (distance < 3f && distance > 2f)
                            forward = 0f;
                        else if (distance <= 2f)
                            forward = -1f;
                        agent.isStopped = false;
                    }
                }
                else
                    anim.SetTrigger("180");
            }
            else
            {
                forward = 0;
                agent.isStopped = true;
            }
        }
        else
        {
            forward = 0;
            agent.isStopped = true;
        }
    }
    void KnifeBhv()
    {
        if (target && chasing && !target.GetComponent<LifeBase>().dead)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            agent.SetDestination(target.position);
            anim.ResetTrigger("180");

            if (!inGround && !falled && !noMove && !fallTime)
            {
                Vector3 direction = target.position - transform.position;
                float angle = Vector3.Angle(direction, transform.forward);

                if (angle <= 140)
                {
                    if (canAttack)
                    {
                        if (distance >= 4f)
                            forward = 2;
                        else if (distance < 4f && distance > 1f)
                            forward = 1;
                        else if (distance <= 1f)
                        {
                            forward = 0f;
                            if (!anim.GetCurrentAnimatorStateInfo(3).IsName("Hit") &&
                                isFOV(transform, target, 60, 1.5f, FXController.instance.layers.enemyCheckForPlayer))
                                Attack();
                            else
                                forward = 1;
                        }
                        agent.isStopped = false;
                    }
                    else
                    {
                        if (distance >= 3f)
                            forward = 1;
                        else if (distance < 3f && distance > 2f)
                            forward = 0f;
                        else if (distance <= 2f)
                            forward = -1f;
                        agent.isStopped = false;
                    }
                }
                else
                    anim.SetTrigger("180");
            }
            else
            {
                forward = 0;
                agent.isStopped = true;
            }
        }
        else
        {
            forward = 0;
            agent.isStopped = true;
        }
    }
    void WoodBhv()
    {
        if (target && chasing && !target.GetComponent<LifeBase>().dead)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            agent.SetDestination(target.position);
            anim.ResetTrigger("180");

            if (!inGround && !falled && !noMove && !fallTime)
            {
                Vector3 direction = target.position - transform.position;
                float angle = Vector3.Angle(direction, transform.forward);

                if (angle <= 140)
                {
                    if (canAttack)
                    {
                        if (distance >= 4f)
                            forward = 2;
                        else if (distance < 4f && distance > 1f)
                            forward = 1;
                        else if (distance <= 1f)
                        {
                            forward = 0f;
                            if (!anim.GetCurrentAnimatorStateInfo(3).IsName("Hit") &&
                                isFOV(transform, target, 60, 1.5f, FXController.instance.layers.enemyCheckForPlayer))
                                Attack();
                            else
                                forward = 1;
                        }
                        agent.isStopped = false;
                    }
                    else
                    {
                        if (distance >= 3f)
                            forward = 1;
                        else if (distance < 3f && distance > 2f)
                            forward = 0f;
                        else if (distance <= 2f)
                            forward = -1f;
                        agent.isStopped = false;
                    }
                }
                else
                    anim.SetTrigger("180");
            }
            else
            {
                forward = 0;
                agent.isStopped = true;
            }
        }
        else
        {
            forward = 0;
            agent.isStopped = true;
        }
    }
    void HandBhv()
    {
        if (target && chasing && !target.GetComponent<LifeBase>().dead)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            agent.SetDestination(target.position);
            anim.ResetTrigger("180");

            if (!inGround && !falled && !noMove && !fallTime)
            {
                Vector3 direction = target.position - transform.position;
                float angle = Vector3.Angle(direction, transform.forward);

                if (angle <= 140)
                {
                    if (canAttack)
                    {
                        if (distance >= 4f)
                            forward = 2;
                        else if (distance < 4f && distance > 1f)
                            forward = 1;
                        else if (distance <= 1f)
                        {
                            forward = 0f;
                            if (!anim.GetCurrentAnimatorStateInfo(3).IsName("Hit") &&
                                isFOV(transform, target, 60, 1.5f, FXController.instance.layers.enemyCheckForPlayer))
                                Attack();
                            else
                                forward = 1;
                        }
                        agent.isStopped = false;
                    }
                    else
                    {
                        if (distance >= 3f)
                            forward = 1;
                        else if (distance < 3f && distance > 2f)
                            forward = 0f;
                        else if (distance <= 2f)
                            forward = -1f;
                        agent.isStopped = false;
                    }
                }
                else
                    anim.SetTrigger("180");
            }
            else
            {
                forward = 0;
                agent.isStopped = true;
            }
        }
        else
        {
            forward = 0;
            agent.isStopped = true;
        }
    }

    //hit events
    void LaunchStone()
    {
        GameObject stone = Instantiate(weapons.stone, weapons.stoneSpawner.position, weapons.stoneSpawner.rotation);

        HitCollider hitCollider = stone.AddComponent<HitCollider>();
        hitCollider.enemy = true;
        hitCollider.damage = damage;

        stone.layer = 20;

        MeshCollider mesh = stone.AddComponent<MeshCollider>();
        mesh.convex = true;

        Rigidbody rigid = stone.AddComponent<Rigidbody>();

        Transform newTarget = new GameObject().transform;
        newTarget.position = target.position + Vector3.up * 1.5f;

        launch.Launch(rigid, newTarget);

        Destroy(stone, 5);
        Destroy(newTarget.gameObject);

        attackTime = attackRate;

        int random = Random.Range(0, 10);
        if(random > 3)
        {
            weapons.typeEnemy = TypeEnemy.Hand;
            InitWeapons();
        }
    }
    void GunFire()
    {
        FXController.instance.FireMuzzle(weapons.bulletSpawner);

        audioSource.PlayOneShot(weapons.shotClip);

        Vector3 destiny = weapons.bulletSpawner.position + weapons.bulletSpawner.forward * 10;

        if (isFOV(transform, target, 15, 10f, FXController.instance.layers.enemyCheckForPlayer) &&
            !target.GetComponent<PlayerController>().undamage)
        {
            target.GetComponent<LifeBase>().TakeDamage(damage, transform.position);
            attackTime = attackRate;

            destiny = target.position + Vector3.up * 1.5f;
        }

        FXController.instance.SpawnTrail(weapons.stoneSpawner.position, destiny);
    }
    void MeleeHit()
    {
        if (isFOV(transform, target, 60, 1.5f, FXController.instance.layers.enemyCheckForPlayer) &&
            !target.GetComponent<PlayerController>().undamage)
        {
            target.GetComponent<LifeBase>().TakeDamage(damage, transform.position);
            attackTime = attackRate;
        }
    }
    //attacks
    void MeleeAttack()
    {
        if (!anim.IsInTransition(0) && !inGround && !falled && !fallTime && !noMove)
        {
            anim.CrossFade("Attack", 0.1f, 0, 0.15f);
            attackTime = attackRate;
        }
    }
    void StoneAttack()
    {
        if (!anim.IsInTransition(0) && !inGround && !falled && !fallTime && !noMove)
        {
            anim.CrossFade("Attack", 0.1f, 0, 0.15f);
            attackTime = attackRate;
        }
    }
    void GunAttack()
    {
        if (!anim.IsInTransition(0) && !inGround && !falled && !fallTime && !noMove)
        {
            anim.CrossFade("Aim", 0.2f, 0, 0.15f);
            attackTime = attackRate;
        }
    }
    #endregion

    public override void Attack()
    {
        switch (weapons.typeEnemy)
        {
            case TypeEnemy.Gun:
                GunAttack();
                break;
            case TypeEnemy.Stone:
                StoneAttack();
                break;
            case TypeEnemy.Knife:
                MeleeAttack();
                break;
            case TypeEnemy.Wood:
                MeleeAttack();
                break;
            case TypeEnemy.Hand:
                MeleeAttack();
                break;
        }
    }

    public override void DamageExtra()
    {
        if (falled && !fallTime)
        {
            attackTime = 1;

            WakeUp();
        }
    }

    public override void Behaviour()
    {
        if (aiming)
        {
            agent.SetDestination(target.position);
        }
        else
        {
            switch (weapons.typeEnemy)
            {
                case TypeEnemy.Gun:
                    GunBhv();
                    break;
                case TypeEnemy.Stone:
                    StoneBhv();
                    break;
                case TypeEnemy.Knife:
                    KnifeBhv();
                    break;
                case TypeEnemy.Wood:
                    WoodBhv();
                    break;
                case TypeEnemy.Hand:
                    HandBhv();
                    break;
            }
        }

        agent.speed = (aiming) ? 2f : 3.5f;

        UpdateAnimator();
    }

    public override void CriticalReaction(float damage, Vector3 pos)
    {
        lastHitPos = pos;
        GameObject effect = Resources.Load("FX/Head Shot") as GameObject;
        GameObject tempEffect = Instantiate(effect, aimTarget.position, aimTarget.rotation);
        Destroy(tempEffect, 3f);
        aimTarget.transform.localScale = Vector3.zero;
        Die();
        CamPos.instance.ImpulseNoise();
    }

    public override void Dying()
    {
        StopAllCoroutines();
        StartCoroutine(Death());
    }

    IEnumerator Death()
    {
        if (!falled)
        {
            Vector3 direction = lastHitPos - transform.position;
            float angle = Vector3.Angle(direction, transform.forward);

            if (angle <= 90)
            {
                anim.CrossFade("Die Front", 0.1f);
            }
            else
            {
                anim.CrossFade("Die Back", 0.1f);
            }
        }

        Destroy(agent);
        Destroy(meleeReceiver.gameObject);
        Destroy(controller);

        yield return new WaitForSeconds(0.2f);

        puppetMaster.state = RootMotion.Dynamics.PuppetMaster.State.Dead;
        puppetMaster.pinWeight = puppetMaster.muscleWeight = 0;
        enabled = false;
    }

    public override void HeavyReaction()
    {
        StartCoroutine(Heavy());
    }

    IEnumerator Heavy()
    {
        if (!falled && !inGround)
        {
            Vector3 direction = lastHitPos - transform.position;
            float angle = Vector3.Angle(direction, transform.forward);

            if (angle <= 90)
            {
                anim.CrossFade("Die Front", 0.1f);
            }
            else
            {
                anim.CrossFade("Die Back", 0.1f);
            }
        }

        falled = true;
        StartCoroutine(FallTime());

        yield return new WaitForSeconds(0.2f);
        puppetMaster.state = RootMotion.Dynamics.PuppetMaster.State.Dead;

        float time = 0;

        while (time < 8)
        {
            time += Time.deltaTime;
            yield return null;
        }

        if (!dead)
            WakeUp();
    }

    IEnumerator FallTime()
    {
        fallTime = true;

        yield return new WaitForSeconds(2f);

        yield return new WaitForEndOfFrame();
        fallTime = false;
    }

    public void Hit()
    {
        switch (weapons.typeEnemy)
        {
            case TypeEnemy.Gun:
                GunFire();
                break;
            case TypeEnemy.Stone:
                LaunchStone();
                break;
            case TypeEnemy.Knife:
                MeleeHit();
                break;
            case TypeEnemy.Wood:
                MeleeHit();
                break;
            case TypeEnemy.Hand:
                MeleeHit();
                break;
        }
    }

    void WakeUp()
    {
        attackTime = 1;
        StopAllCoroutines();

        falled = true;
        fallTime = falled;

        puppetMaster.state = RootMotion.Dynamics.PuppetMaster.State.Alive;

        bool isProne = IsProne();

        //Set Target Rotation
        Vector3 spineDirection = puppetMaster.muscles[0].rigidbody.rotation * hipsUp;
        Vector3 normal = puppetMaster.targetRoot.up;
        Vector3.OrthoNormalize(ref normal, ref spineDirection);
        RotateTarget(Quaternion.LookRotation((isProne ? spineDirection : -spineDirection), puppetMaster.targetRoot.up));

        // Set the target's position
        puppetMaster.SampleTargetMappedState();
        Vector3 getUpOffset = isProne ? getUpOffsetProne : getUpOffsetSupine;
        MoveTarget(puppetMaster.muscles[0].rigidbody.position + puppetMaster.targetRoot.rotation * getUpOffset);
        getUpPosition = puppetMaster.targetRoot.position;

        //Getting up
        if (!isProne)
            anim.Play("Front Getting up", 0, 0);
        else
            anim.Play("Back Getting Up", 0, 0);

        falled = false;
        fallTime = falled;
    }

    #region Ragdoll Funtions

    private Vector3 hipsForward;
    private Vector3 hipsUp;
    private Vector3 getUpPosition;

    [Header("Ragdoll")]
    public Vector3 getUpOffsetProne;
    public Vector3 getUpOffsetSupine;

    public bool IsProne()
    {
        float dot = Vector3.Dot(puppetMaster.muscles[0].transform.rotation * hipsForward, puppetMaster.targetRoot.up);
        return dot < 0f;
    }

    protected void RotateTarget(Quaternion rotation)
    {
        puppetMaster.targetRoot.rotation = rotation;
    }

    protected void MoveTarget(Vector3 position)
    {
        puppetMaster.targetRoot.position = position;
    }

    #endregion

    float weight = 1;

    void UpdateAnimator()
    {
        // update the animator parameters
        anim.SetFloat("Forward", forward, 0.25f, Time.deltaTime);
    }
}
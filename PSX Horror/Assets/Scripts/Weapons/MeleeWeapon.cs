using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : WeaponBase
{
    [Header("Melee Values")]
    public Collider hitCollider;
    public float timeToCollider, currentTimeToCollider;

    public override void OnFire()
    {
        hitCollider.enabled = true;
        currentTimeToCollider = timeToCollider;
    }

    public override void OnReload()
    {
        throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    new void Start()
    {
        hitCollider.enabled = false;
        hitCollider.GetComponent<HitCollider>().damage = damage;

        base.Start();
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
        if (currentTimeToCollider > 0)
        {
            currentTimeToCollider -= Time.deltaTime;
        }
        else
        {
            hitCollider.enabled = false;
        }
    }
}

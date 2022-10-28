using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LifeBase : MonoBehaviour
{
    [Header("Life States")]
    public float currentLife;
    public float maxLife;

    [HideInInspector]
    public Vector3 lastHitPos;

    public bool dead;

    // Start is called before the first frame update
    public void Start()
    {
        //currentLife = maxLife;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Recovery(float life)
    {
        if (currentLife + life > maxLife)
            currentLife = maxLife;
        else
            currentLife += life;
    }

    public void TakeDamage(float damage, Vector3 pos)
    {
        if (!dead)
        {
            currentLife -= damage;

            lastHitPos = pos;
            OnDamage(damage);

            if (currentLife <= 0)
            {
                Die();
            }
        }
    }

    public void Die()
    {
        if(!dead)
            OnDie();
        dead = true;
    }

    public abstract void OnDamage(float damage);
    public abstract void OnDie();
}

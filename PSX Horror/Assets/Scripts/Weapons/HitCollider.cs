using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitCollider : MonoBehaviour
{
    public float damage;
    public bool enemy;

    private void OnCollisionEnter(Collision collision)
    {
        if (!enemy)
        {
            if (!collision.collider.CompareTag("Player"))
            {
                GameObject tempTarget = ExtensionMethods.FindParentWithTag(collision.gameObject, "Enemy");
                EnemyBase target = null;
                if (tempTarget)
                    target = tempTarget.GetComponentInChildren<EnemyBase>();
                enabled = false;

                if (target && enabled)
                {
                    target.TakeDamage(damage, transform.position);
                    FXController.instance.SpawnBloodEffect(collision.contacts[0].point, collision.contacts[0].normal);
                }
            }
        }
        else
        {
            if (enabled)
            {
                PlayerController target = collision.contacts[0].otherCollider.GetComponent<PlayerController>();
                print(collision.contacts[0].otherCollider.name);

                if (target)
                {
                    target.TakeDamage(damage, transform.position);
                    FXController.instance.SpawnBloodEffect(transform.position, transform.eulerAngles);
                }

                Destroy(this);
            }
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (!enemy)
        {
            GameObject tempTarget = ExtensionMethods.FindParentWithTag(col.gameObject, "Enemy");
            EnemyBase target = null;

            if (tempTarget)
                target = tempTarget.GetComponentInChildren<EnemyBase>();

            if (target)
            {
                target.TakeDamage(damage, transform.position);
                FXController.instance.SpawnBloodEffect(target.aimTarget.position, transform.eulerAngles);
            }
        }
        else
        {
            PlayerController target = col.GetComponent<PlayerController>();

            if (target)
            {
                target.TakeDamage(damage, transform.position);
                FXController.instance.SpawnBloodEffect(transform.position, transform.eulerAngles);
            }
        }
    }
}
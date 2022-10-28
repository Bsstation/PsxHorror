using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion;

namespace RootMotion.Dynamics
{
    public class RaycastTest : MonoBehaviour
    {
        public float force = 100;
        public float damage = 20;

        [Header("Crazy")]
        public ParticleSystem[] muzzleFlash;
        TrailRenderer trail;
        ParticleSystem hitEffect;

        AudioSource audioSource;
        [Header("Audios")]
        public AudioClip shotAudio;
        AudioClip dryAudio, dropLoader;

        protected Vector3 origin;
        public LayerMask layers;


        // Start is called before the first frame update
        void Start()
        {
            GameObject tempFx = Instantiate(Resources.Load("FX/HitEffect"), transform) as GameObject;
            hitEffect = tempFx.GetComponent<ParticleSystem>();

            GameObject tempTrial = Resources.Load("FX/Bullet Tracer") as GameObject;
            trail = tempTrial.GetComponent<TrailRenderer>();

            audioSource = gameObject.AddComponent<AudioSource>();
            dryAudio = Resources.Load<AudioClip>("Dry Fire") as AudioClip;
            dropLoader = Resources.Load<AudioClip>("Loader Drop") as AudioClip;
        }

        // Update is called once per frame
        void Update()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                foreach (ParticleSystem particle in muzzleFlash)
                {
                    if (particle)
                        particle.Emit(1);
                }

                audioSource.PlayOneShot(shotAudio);
                Vector3 direction = ray.direction;
                Shot(damage, direction);
            }
        }

        public void Shot(float damage, Vector3 direction)
        {
            origin = transform.position;

            RaycastHit hit;

            //Debug.DrawRay(origin, direction * 100, Color.green, 1f);

            Vector3 destiny = muzzleFlash[0].transform.position + muzzleFlash[0].transform.forward * 50;

            if (Physics.Raycast(origin, direction, out hit, 100, layers))
            {
                print(hit.transform.name);

                GameObject tempTarget = ExtensionMethods.FindParentWithTag(hit.transform.gameObject, "Enemy");
                EnemyBase target = null;
                if (tempTarget)
                    target = tempTarget.GetComponentInChildren<EnemyBase>();

                if (target)
                {
                    float value = Random.Range(0.1f, 100);

                    target.TakeDamage(damage, transform.position);
                    GameObject effect = Resources.Load("FX/Blood") as GameObject;
                    GameObject tempEffect = Instantiate(effect, target.aimTarget.position, Quaternion.LookRotation(hit.normal));
                    Destroy(tempEffect, 10f);

                    destiny = target.aimTarget.position;
                }
                else
                {
                    hitEffect.transform.position = hit.point;
                    hitEffect.transform.forward = hit.normal;
                    hitEffect.Emit(1);

                    destiny = hit.point;
                }

                if (hit.rigidbody)
                    hit.rigidbody.AddForce(hit.point * damage * 100);
            }

            if (trail)
            {
                var tempTrail = Instantiate(trail, muzzleFlash[0].transform.position, Quaternion.identity);
                tempTrail.AddPosition(muzzleFlash[0].transform.position);
                tempTrail.transform.position = destiny;
            }
        }

    }
}

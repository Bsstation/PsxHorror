using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FXController : MonoBehaviour
{
    public static FXController instance;
    
    [HideInInspector]
    public AudioClip dryAudio, dropLoader;

    TrailRenderer trail;
    ParticleSystem hitEffect;
    ParticleSystem bloodHitEffect;
    ParticleSystem[] muzzleFlash;

    [System.Serializable]
    public struct Layers
    {
        public LayerMask laserSight;
        public LayerMask weaponDamageLayer;
        public LayerMask playerCheckForEnemy;
        public LayerMask floor;
        public LayerMask enemyCheckForPlayer;
        public LayerMask camView;
    }
    public Layers layers;
    public GameObject postProcessing;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        postProcessing.SetActive(Settings.instance.hasPP);

        //camera
        Camera.main.cullingMask = layers.camView;
        Camera.main.GetComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;
        //Audios
        dryAudio = Resources.Load<AudioClip>("Dry Fire") as AudioClip;
        dropLoader = Resources.Load<AudioClip>("Loader Drop") as AudioClip;

        //Particles
        Vector3 pos = transform.position + Vector3.down * 50;

        GameObject tempFx = Instantiate(Resources.Load("FX/HitEffect"), transform) as GameObject;
        hitEffect = tempFx.GetComponent<ParticleSystem>();
        tempFx.transform.position = pos;

        tempFx = Instantiate(Resources.Load("FX/Blood"), transform) as GameObject;
        bloodHitEffect = tempFx.GetComponent<ParticleSystem>();
        tempFx.transform.position = pos;

        tempFx = Resources.Load("FX/Bullet Tracer") as GameObject;
        trail = tempFx.GetComponent<TrailRenderer>();
        tempFx.transform.position = pos;

        //muzzle
        muzzleFlash = new ParticleSystem[2];
        GameObject tempMuzzle = Instantiate(Resources.Load("FX/Muzzle"), transform) as GameObject;
        tempMuzzle.transform.position = pos;

        muzzleFlash[0] = tempMuzzle.GetComponent<ParticleSystem>();
        muzzleFlash[1] = tempMuzzle.transform.GetChild(0).GetComponent<ParticleSystem>();
        muzzleFlash[1].transform.parent = transform;

        InitSFX();
    }

    public void InitSFX()
    {
        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        Camera cam = Camera.main;

        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForEndOfFrame();

            Vector3 origin = cam.transform.position + Vector3.forward * 10;
            Vector3 destiny = origin + Vector3.forward * 20;

            SpawnTrail(origin, destiny);
            SpawnHitEffect(origin, origin);
            SpawnHitEffect(origin, origin);

            Transform temp = new GameObject().transform;
            temp.position = origin;

            FireMuzzle(temp);
            Destroy(temp.gameObject);
        }
    }

    public void UpdatePostProcessing(bool value)
    {
        postProcessing.SetActive(value);
    }

    public void SpawnTrail(Vector3 origin, Vector3 destiny)
    {
        if (trail)
        {
            var tempTrail = Instantiate(trail, origin, Quaternion.identity);
            tempTrail.AddPosition(origin);
            tempTrail.transform.position = destiny;
        }
    }

    public void SpawnHitEffect(Vector3 point, Vector3 normal)
    {
        hitEffect.transform.position = point;
        hitEffect.transform.forward = normal;
        hitEffect.Emit(1);
    }

    public void SpawnBloodEffect(Vector3 point, Vector3 normal)
    {
        bloodHitEffect.transform.position = point;
        bloodHitEffect.transform.forward = normal;
        bloodHitEffect.Emit(4);
        bloodHitEffect.GetComponent<AudioSource>().Play();
    }

    public void FireMuzzle(Transform transform)
    {
        foreach (ParticleSystem particle in muzzleFlash)
        {
            if (particle)
            {
                particle.transform.position = transform.position;
                particle.transform.rotation = transform.rotation;
                particle.Emit(1);
            }
        }
    }
}
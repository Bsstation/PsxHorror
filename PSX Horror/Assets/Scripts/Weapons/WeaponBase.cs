using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    Handgun, AR, Melee, Handgun40, Shotgun, Magnum, Smg
}

public enum StorePoint
{
    Back, Side
}

public abstract class WeaponBase : MonoBehaviour {

    protected Vector3 origin;

    public WeaponType weaponType;
    public StorePoint store;

    public int currentAmmo, capacityAmmo;
    public bool canReload, haveLoader;
    public float damage, fireRate;

    [HideInInspector]
    public float currentTimeToSHot;
    [HideInInspector]
    public bool canFire;

    [Range(0, 100)]
    public float criticalRate;
    public RuntimeAnimatorController runtimeAnimatorController;

    [System.Serializable]
    public struct Positions
    {
        public Vector3 equipPosition;
        public Vector3 equipRotation;
        [Space(5)]
        public Vector3 storePosition;
        public Vector3 storeRotation;
        [Space(5)]
        public Vector3 handAimPosition;
        public Vector3 handAimRotation;
        [Space(5)]
        public Vector3 elbowPosition;
    }

    [Header("Positions")]
    public Positions positions;

    public Transform leftHandPos;
    public bool TwoHandAimOff, TwoHandAimOn;

    [HideInInspector]
    public Transform bulletSpawnPoint;

    public string shotAnimation, reloadAnimation;

    AudioSource audioSource;
    [Header("Audios")]
    public AudioClip shotAudio;
    public AudioClip reloadAudio;
    public AudioClip pumpAudio;

    Animator anim;
    protected PlayerController player;

    [Header("Animations")]
    public bool reloading;
    public bool isEmpty;
    public bool pumping;

    int shellcount = 1;
    public GameObject shellPrefab;
    public Transform shellSpawner;

    public GameObject loader;

    // Start is called before the first frame update
    public void Start()
    {
        player = GameManager.instance.player;

        audioSource = GetComponent<AudioSource>();

        anim = GetComponent<Animator>();

        bulletSpawnPoint = transform.GetChild(0);
    }

    // Update is called once per frame
    public void Update()
    {
        if (currentTimeToSHot > 0)
        {
            currentTimeToSHot -= Time.deltaTime;
            canFire = false;
        }
        else if(currentAmmo > 0)
            canFire = true;

        shellcount = (haveLoader && weaponType != WeaponType.Magnum) ? 1 : (haveLoader && weaponType == WeaponType.Magnum) ?
            capacityAmmo : capacityAmmo - currentAmmo;

        canReload = currentAmmo < capacityAmmo && !pumping;

        Anim();
    }

    public void Anim()
    {
        if (!anim.runtimeAnimatorController)
            pumping = reloading = isEmpty = false;
        else
        {
            pumping = anim.GetCurrentAnimatorStateInfo(0).IsTag("Pump") || anim.GetAnimatorTransitionInfo(0).IsUserName("Pump");
            reloading = player.reloading;
            isEmpty = currentAmmo == 0;
        }

        if(ExtensionMethods.ContainsParam(anim, "IsEmpty"))
            anim.SetBool("IsEmpty", isEmpty);

        if (ExtensionMethods.ContainsParam(anim, "Reloading"))
            anim.SetBool("Reloading", reloading);
    }

    public void Fire()
    {
        if (weaponType != WeaponType.Melee)
        {
            currentAmmo--;
            FXController.instance.FireMuzzle(bulletSpawnPoint);
        }

        audioSource.PlayOneShot(shotAudio);
        anim.Play("Fire", 0, 0);
        currentTimeToSHot = fireRate;

        OnFire();
    }

    #region Events
    public void Eject()
    {
        if (shellPrefab && shellcount > 0 && shellSpawner) StartCoroutine(ShellSpawn());
    }

    public void EjectSingle()
    {
        GameObject temp = Instantiate(shellPrefab, shellSpawner.position, shellSpawner.rotation);
        temp.GetComponent<Rigidbody>().velocity = shellSpawner.TransformDirection((Vector3.right * 40 * Time.deltaTime) + (Vector3.forward * Random.Range(-20, 20) * Time.deltaTime));
        Destroy(temp, 4f);
    }

    public void PumpAudio()
    {
        if (pumpAudio) audioSource.PlayOneShot(pumpAudio);
    }

    IEnumerator ShellSpawn()
    {
        for(int i = 0; i < shellcount; i++)
        {
            EjectSingle();
            yield return new WaitForSeconds(0.03f);
        }
    }

    public void EjectLoader()
    {
        if (!loader) return;

        audioSource.PlayOneShot(FXController.instance.dropLoader);

        if (loader.activeInHierarchy)
            loader.SetActive(false);
        GameObject tempLoader = Instantiate(loader, loader.transform.position, loader.transform.rotation);
        tempLoader.layer = 17;

        tempLoader.GetComponent<Collider>().enabled = true;

        tempLoader.SetActive(true);
        Rigidbody rigid = tempLoader.AddComponent<Rigidbody>();
        rigid = shellPrefab.GetComponent<Rigidbody>();
        rigid.velocity = tempLoader.transform.TransformDirection(Vector3.down * 10 * Time.deltaTime);

        Destroy(tempLoader, 10f);
    }

    public void ActiveLoader()
    {
        if (!loader) return;

        if (!loader.activeInHierarchy)
            loader.SetActive(true);
    }

    public void ResetAnimations()
    {
        anim.Play("New State",0,0);
        ActiveLoader();

        if (GetComponentInChildren<HitCollider>())
            GetComponentInChildren<HitCollider>().enabled = false;
    }

    #endregion

    public void Shot(float damage, Vector3 direction)
    {
        origin = player.transform.position;
        origin.y = bulletSpawnPoint.position.y;

        RaycastHit hit;

        Vector3 destiny = bulletSpawnPoint.position + bulletSpawnPoint.forward * 50;

        if (Physics.Raycast(origin, direction, out hit, 100, FXController.instance.layers.weaponDamageLayer))
        {
            GameObject tempTarget = ExtensionMethods.FindParentWithTag(hit.transform.gameObject, "Enemy");
            EnemyBase target = null;
            if (tempTarget)
                target = tempTarget.GetComponentInChildren<EnemyBase>();

            if (target)
            {
                float value = Random.Range(0.1f, 100);
                if (weaponType == WeaponType.Shotgun)
                    value = hit.distance;

                if (!CheckCritical(value) || !target.canCritical)
                {
                    target.TakeDamage(damage, transform.position);
                    FXController.instance.SpawnBloodEffect(hit.point, hit.normal);
                }
                else
                    target.CriticalReaction(damage, transform.position);

                destiny = target.aimTarget.position;
            }
            else
            {
                FXController.instance.SpawnHitEffect(hit.point, hit.normal);
                destiny = hit.point;
            }
        }

        FXController.instance.SpawnTrail(bulletSpawnPoint.position, destiny);
    }

    public void PiercingShot(float damage, Vector3 direction)
    {
        origin = player.transform.position;
        origin.y = bulletSpawnPoint.position.y;

        RaycastHit[] hits;

        hits = Physics.RaycastAll(origin, direction, 100f, FXController.instance.layers.weaponDamageLayer);

        Vector3 destiny = bulletSpawnPoint.position + bulletSpawnPoint.forward * 50;

        float currentDamage = damage;
        float value = Random.Range(0.1f, 100);

        int outs = Mathf.Clamp(hits.Length, 0, 2);

        for (int i = 0; i < outs; i++)
        {
            RaycastHit hit = hits[i];

            GameObject tempTarget = ExtensionMethods.FindParentWithTag(hit.transform.gameObject, "Enemy");
            EnemyBase target = null;
            if (tempTarget)
                target = ExtensionMethods.FindParentWithTag(hit.transform.gameObject, "Enemy").GetComponentInChildren<EnemyBase>();

            if (target)
            {
                if (weaponType == WeaponType.Shotgun)
                    value = hit.distance;

                if (!CheckCritical(value) || !target.canCritical)
                {
                    target.TakeDamage(damage, transform.position);
                    FXController.instance.SpawnBloodEffect(hit.point, hit.normal);
                }
                else
                    target.CriticalReaction(damage, transform.position);
            }
            else
                FXController.instance.SpawnHitEffect(hit.point, hit.normal);

            if (i == outs - 1)
                destiny = hit.point;

            currentDamage = currentDamage / 2;
            value = 100;
        }

        FXController.instance.SpawnTrail(bulletSpawnPoint.position, destiny);
    }

    bool CheckCritical(float value)
    {
        if(weaponType == WeaponType.Shotgun)
        {
            float i = Random.Range(0.1f, 100);

            if (value < (criticalRate) && i < 20)
                return true;
        }
        else if (value <= criticalRate)
            return true;

        return false;
    }

    public int CheckAmmo()
    {
        InventoryUI inv = InventoryUI.instance;
        return inv.CheckAmmo(weaponType);
    }
    
    public void DryFire()
    {
        audioSource.PlayOneShot(FXController.instance.dryAudio);
    }

    public IEnumerator StartReload()
    {
        yield return new WaitForSeconds(0.07f);
        yield return new WaitForEndOfFrame();
        anim.Play("Reload", 0, 0);
    }

    public void Reload()
    {
        OnReload();
        audioSource.PlayOneShot(reloadAudio);
        ActiveLoader();
    }

    public abstract void OnFire();
    public abstract void OnReload();
}
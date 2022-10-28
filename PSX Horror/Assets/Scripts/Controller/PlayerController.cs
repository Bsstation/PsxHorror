using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlayerController : LifeBase
{
    #region Variables
    Animator anim;
    CharacterController controller;

    [Header("Movement")]
    public float moveSpeed = 2;
    public float rotateSpeed = 1.5f;

    Vector3 rotation;

    [HideInInspector]
    public bool sprint, reloading, uncontrol, undamage, bleeding, attacking;

    public bool shellReloading;
    [HideInInspector]
    public bool aim, aiming;

    bool ikOff;

    RuntimeAnimatorController unarmedAnimator;

    [HideInInspector]
    public WeaponBase currentWeapon;
    [Header("Weapons")]
    public List<Transform> pivots = new List<Transform>(2);
    public Transform weaponPivot;

    Light flashLight;
    public bool flashlightEnable;

    [Header("IK")]
    [Range(0, 1)]
    public float bodyWeightIntensity = 0.1f;
    [Range(0, 1)]
    public float bodyWeightMeleeIntensity = 1;
    [Range(0, 1)]
    public float headWeightIntensity = 0.4f;

    [Range(0, 1)]
    float bodyWeight;
    [Range(0, 1)]
    float rightHandWeight;
    [Range(0, 1)]
    float leftHandWeight;
    [Range(0, 1)]
    float rightElbowWeight;

    Transform mTransform;

    [HideInInspector]
    public Transform leftHandTarget;

    Transform rightHandTarget, rightElbowTarget, shoulder, lookDir, aimPivot;

    [HideInInspector]
    public ItemBase currentItem;
    [HideInInspector]
    public InteractibleBase interaction;

    [Header("Aim")]
    [Range(0f, 1f)]
    public float angleToAim = 0.1f;
    public float speedAim = 30;

    [HideInInspector]
    public EnemyBase enemyAimed;
    [HideInInspector]
    public EnemyBase[] enemiesInScene;

    [HideInInspector]
    public float v, h;

    InventoryUI inventory;

    public bool canShoot;
    #endregion

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        inventory = InventoryUI.instance;
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        if(ExtensionMethods.ContainsParam(anim, "Fire"))
            anim.ResetTrigger("Fire");

        if (GetComponentInChildren<Light>())
            flashLight = GetComponentInChildren<Light>();
        else
            print("esqueceu da lanterna!!!");

        mTransform = new GameObject("mTransform").transform;
        mTransform.parent = transform;
        mTransform.localPosition = new Vector3(0, 1.5f, 0);

        var temp = Instantiate(Resources.Load("Init/Player/AimPivot"), transform) as GameObject;
        aimPivot = temp.transform;

        var tempPos = Instantiate(Resources.Load("Init/Player/Map Icon"), transform) as GameObject;

        inventory.mapPanel.GetPlayerPos(tempPos);

        shoulder = anim.GetBoneTransform(HumanBodyBones.RightShoulder).transform;
        rightHandTarget = aimPivot.GetChild(0).GetChild(0);
        lookDir = aimPivot.GetChild(0).GetChild(1);
        rightElbowTarget = aimPivot.GetChild(0).GetChild(2);

        unarmedAnimator = Resources.Load("Init/Player/Unarmed") as RuntimeAnimatorController;

        LoadKeys();
    }

    public void LoadKeys()
    {
        ItemBox.instance.LoadItems();
        InventoryUI.instance.LoadItems();
        Load();
        MainMenu.DeleteKeys();
    }

    // Update is called once per frame
    void Update()
    {
        SetStates();
        SetWeightLook();

        Control();
        UpdateIkAim();
    }

    private void FixedUpdate()
    {
        mTransform.forward = transform.forward;

        if(initFlashlight)
            flashLight.enabled = flashlightEnable;

        Movement();
        Anim();
    }

    #region Control

    Vector2 newVelocity;
    Vector3 newCamForward;
    Vector3 newCamRight;
    Vector2 velocity;

    public void Control()
    {
        if (!currentWeapon && anim.runtimeAnimatorController != unarmedAnimator)
            anim.runtimeAnimatorController = unarmedAnimator;

        if (GameManager.instance.gameStatus == GameStatus.Game && !dead)
        {
            if (MessagesBehaviour.instance.examing)
            {
                aim = sprint = false;
                if (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Mouse0) ||
                    Input.GetKeyDown(InputManager.instance.kKeys.action) || InputManager.instance.GetJoyButtonDown("A"))
                    MessagesBehaviour.instance.ClearMessage();
            }
            else if(!uncontrol && !undamage)
            {
                velocity = (InputManager.instance.mode == InputMode.keyboard) ? InputManager.instance.Movement() :
                    InputManager.instance.JoystickLAxis();

                v = velocity.y;
                h = velocity.x;

                if (!anim.GetCurrentAnimatorStateInfo(2).IsName("Attack") && velocity.x == 0 && (Settings.instance.tank || aim))
                    h = (InputManager.instance.mode == InputMode.keyboard) ? Mathf.Clamp(Input.GetAxis("Mouse X"), -1.5f, 1.5f) :
                        Mathf.Clamp(InputManager.instance.JoystickRAxis().x, -1, 1);

                sprint = (Input.GetKey(InputManager.instance.kKeys.sprint) ||
                    InputManager.instance.GetJoyButton("B")) &&
                    !aim && !(Settings.instance.tank && v <= 0);

                ControlShortcuts();
                ControlWeapons();

                if ((Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(InputManager.instance.kKeys.action) ||
                    InputManager.instance.GetJoyButtonDown("A")) && !aim)
                {
                    if (currentItem)
                    {
                        float height = currentItem.transform.position.y - transform.position.y;

                        string animation = (height > 0.8f && height < 1.5f) ? "Pick_Mid" : (height <= 0.8f) ? "Pick_Down" : "Pick_Up";
                        anim.CrossFade(animation, 0.1f, 2);
                    }
                    else if (interaction)
                    {
                        interaction.Interact();
                        interaction = null;
                    }
                }

                if (Input.GetKeyDown(InputManager.instance.kKeys.flashlight) ||
                    InputManager.instance.GetJoyButtonDown("rstick"))
                {
                    flashlightEnable = !flashlightEnable;
                    InventoryUI.instance.PlayFlashlight();
                }
            }
            else
            {
                //if (currentWeapon && !reloading) currentWeapon.ResetAnimations();
                aim = false;
                h = v = 0;
                controller.Move(Vector3.zero);
            }

            if (reloading && shellReloading && currentWeapon.currentAmmo > 0)
            {
                if (Input.GetButton("Submit") || Input.GetKey(KeyCode.Mouse0) || (Input.GetKey(InputManager.instance.kKeys.action) ||
                    InputManager.instance.GetJoyTrigger("right")))
                {
                    anim.CrossFade("New State", 0.02f, 2);
                }
            }
        }
        else
        {
            //if (currentWeapon) currentWeapon.ResetAnimations();
            aim = false;
            h = v = 0;
            controller.Move(Vector3.zero);
        }
    }

    public void Movement()
    {
        if (GameManager.instance.gameStatus != GameStatus.Game || dead || uncontrol || undamage) return;

        float speed = ((bleeding || (currentLife / maxLife) <= 0.5f)) ? moveSpeed * 0.7f : moveSpeed;
        float r = 100;

        if (Mathf.Abs(v) != 0)
            r = 50;

        if (attacking)
            h = v = 0;

        Vector3 move = new Vector3(0, 0, v);

        if (Settings.instance.tank || aim)
        {
            if (sprint)
            {
                speed = moveSpeed * 3;
                v = 2;
            }

            rotation = new Vector3(0, h * rotateSpeed * r * Time.deltaTime, 0);
            transform.Rotate(rotation);

            float vSpeed = 0;
            if (!controller.isGrounded)
                vSpeed -= 9.8f * Time.deltaTime;

            move = transform.TransformDirection(move);

            float newSpeed = speed;
            if (bleeding || (currentLife / maxLife) <= 0.5f) newSpeed = speed * 0.8f;

            move = move.normalized * newSpeed * 0.4f * Time.deltaTime;

            move.y = vSpeed;
            controller.Move(move);

            if (!aim)
            {
                if (v < 0 && (Input.GetKeyDown(InputManager.instance.kKeys.sprint) || InputManager.instance.GetJoyButtonDown("B")))
                    anim.CrossFade("Turn", 0.02f);
            }
        }
        else
        {
            Vector3 camForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1).normalized);
            Vector3 camRight = Vector3.Scale(Camera.main.transform.right, new Vector3(1, 0, 1).normalized);

            if ((camForward != newCamForward || newCamRight != camRight) && velocity != newVelocity)
            {
                newCamForward = camForward;
                newCamRight = camRight;
            }

            move = v * newCamForward + h * newCamRight;

            if (move != Vector3.zero || sprint)
            {
                if (sprint)
                    speed = moveSpeed * 3;

                transform.forward = Vector3.Slerp(transform.forward, move, 5 * Time.deltaTime);
                v = (!sprint) ? 1 : 2;
                h = 0;

                float vSpeed = 0;
                if (!controller.isGrounded)
                    vSpeed -= 9.8f * Time.deltaTime;

                float newSpeed = speed;
                if (bleeding || (currentLife / maxLife) <= 0.5f) newSpeed = speed * 0.8f;

                Vector3 movement = transform.forward.normalized * newSpeed * 0.5f * Time.deltaTime;
                movement.y = vSpeed;

                controller.Move(movement);
            }
            else
                v = 0;

            newVelocity = (InputManager.instance.mode == InputMode.keyboard) ? InputManager.instance.Movement() :
                InputManager.instance.JoystickMove();
        }
    }

    float timeToShot = 0;

    void ControlWeapons()
    {
        aim = currentWeapon && (Input.GetKey(InputManager.instance.kKeys.aim) || Input.GetKey(KeyCode.Mouse1) || GameManager.instance.debugAim ||
            InputManager.instance.GetJoyTrigger("left") || currentWeapon.pumping || attacking);

        timeToShot = (aiming && aim) ? timeToShot + Time.deltaTime : 0;

        if (aiming && aim && timeToShot > 0.3f)
        {
            float y = 1.5f;
            if (currentWeapon.bulletSpawnPoint)
                y = currentWeapon.bulletSpawnPoint.position.y;

            mTransform.position = new Vector3(mTransform.position.x, y, mTransform.position.z);
            SeachTarget();

            if (Input.GetKeyDown(InputManager.instance.kKeys.focus) || Input.GetKeyDown(KeyCode.Mouse2) ||
            InputManager.instance.GetJoyButtonDown("rshoulder"))
                InitAim();

            if (currentWeapon.currentAmmo > 0)
            {
                if (currentWeapon.canFire && !reloading)
                {
                    if (Input.GetButton("Submit") || Input.GetKey(KeyCode.Mouse0) || Input.GetKey(InputManager.instance.kKeys.action) ||
                        InputManager.instance.GetJoyTrigger("right"))
                        Fire();
                }
            }
            else if (currentWeapon.currentAmmo == 0)
            {
                if (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(InputManager.instance.kKeys.action) ||
                    InputManager.instance.GetJoyTriggerDown("right") &&
                    !reloading && !currentWeapon.pumping)
                {
                    if (currentWeapon.CheckAmmo() > 0)
                    {
                        if (currentWeapon.canReload)
                            StartReload();
                    }
                    else
                        currentWeapon.DryFire();
                }
            }
        }
        else
            enemyAimed = null;

        if (currentWeapon && currentWeapon.canReload && !currentWeapon.pumping && !reloading && currentWeapon.CheckAmmo() > 0)
        {
            if (Input.GetKeyDown(InputManager.instance.kKeys.reload) ||
                InputManager.instance.GetJoyButtonDown("X"))
                StartReload();
        }
    }

    void ControlShortcuts()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || InputManager.instance.GetJoyButtonDown("up"))
        {
            ShortcutSlot shortcutSlot = inventory.shortcuts[0];

            if (shortcutSlot.currentItem && !(shortcutSlot.currentItem.GetComponent<WeaponBase>() && 
                shortcutSlot.currentItem.GetComponent<WeaponBase>() == currentWeapon))
            {
                shortcutSlot.currentItem.Use();
                inventory.ShowHotkeys(1);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || InputManager.instance.GetJoyButtonDown("right"))
        {
            ShortcutSlot shortcutSlot = inventory.shortcuts[1];

            if (shortcutSlot.currentItem && !(shortcutSlot.currentItem.GetComponent<WeaponBase>() &&
                shortcutSlot.currentItem.GetComponent<WeaponBase>() == currentWeapon))
            {
                shortcutSlot.currentItem.Use();
                inventory.ShowHotkeys(2);
            }
        }
        else if(Input.GetKeyDown(KeyCode.Alpha3) || InputManager.instance.GetJoyButtonDown("down"))
        {
            ShortcutSlot shortcutSlot = inventory.shortcuts[2];

            if (shortcutSlot.currentItem && !(shortcutSlot.currentItem.GetComponent<WeaponBase>() &&
                shortcutSlot.currentItem.GetComponent<WeaponBase>() == currentWeapon))
            {
                shortcutSlot.currentItem.Use();
                inventory.ShowHotkeys(3);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) || InputManager.instance.GetJoyButtonDown("left"))
        {
            ShortcutSlot shortcutSlot = inventory.shortcuts[3];

            if (shortcutSlot.currentItem && !(shortcutSlot.currentItem.GetComponent<WeaponBase>() &&
                shortcutSlot.currentItem.GetComponent<WeaponBase>() == currentWeapon))
            {
                shortcutSlot.currentItem.Use();
                inventory.ShowHotkeys(4);
            }
        }

        if(InputManager.instance.mode == InputMode.keyboard && Input.GetAxisRaw("Scroll") != 0)
        {
            if (currentWeapon)
            {
                float scroll = Input.GetAxisRaw("Scroll");
                int currentShortcut = 0;
                int init = 0;

                for (int i = 0; i < inventory.shortcuts.Length; i++)
                {
                    if (inventory.shortcuts[i].currentItem &&
                        inventory.shortcuts[i].currentItem.GetComponent<WeaponBase>() == currentWeapon)
                    {
                        init = i;
                        break;
                    }
                }

                if (scroll > 0)
                    currentShortcut = CheckShortcut(true, init);
                else if(scroll < 0)
                    currentShortcut = CheckShortcut(false, init);

                if (currentShortcut != init && inventory.shortcuts[currentShortcut].currentItem &&
                    inventory.shortcuts[currentShortcut].currentItem.GetComponent<WeaponBase>())
                {
                    inventory.shortcuts[currentShortcut].currentItem.Use();
                    inventory.ShowHotkeys(currentShortcut + 1);
                }
            }
            else
            {
                for(int i = 0; i < inventory.shortcuts.Length; i++)
                {
                    if (inventory.shortcuts[i].currentItem &&
                        inventory.shortcuts[i].currentItem.GetComponent<WeaponBase>())
                    {
                        inventory.shortcuts[i].currentItem.Use();
                        inventory.ShowHotkeys(i + 1);
                        break;
                    }
                }
            }
        }
    }

    public int CheckShortcut(bool increased, int value)
    {
        bool loop = false;

        if (increased)
        {
            for (int i = value; i < inventory.shortcuts.Length; i++)
            {
                if (inventory.shortcuts[i].currentItem && inventory.shortcuts[i].currentItem.GetComponent<WeaponBase>() &&
                    inventory.shortcuts[i].currentItem.GetComponent<WeaponBase>() != currentWeapon)
                    return i;

                if (i == inventory.shortcuts.Length - 1 && !loop)
                {
                    i = -1;
                    loop = true;
                }

                if (loop && i == value)
                    break;
            }
        }
        else
        {
            for (int i = value; i >= 0; i--)
            {
                if (inventory.shortcuts[i].currentItem && inventory.shortcuts[i].currentItem.GetComponent<WeaponBase>() &&
                    inventory.shortcuts[i].currentItem.GetComponent<WeaponBase>() != currentWeapon)
                    return i;

                if (i == 0 && !loop)
                {
                    i = inventory.shortcuts.Length;
                    loop = true;
                }

                if (loop && i == value)
                    break;
            }
        }
        return value;
    }

    public void Anim()
    {
        if (ExtensionMethods.ContainsParam(anim, "Aim"))
            anim.SetBool("Aim", aim);

        anim.SetBool("Injuried", bleeding || (currentLife / maxLife) <= 0.5f);

        anim.SetFloat("Vertical", v, 0.2f, Time.deltaTime);
        anim.SetFloat("Horizontal", h, 0.2f, Time.deltaTime);

        if (ExtensionMethods.ContainsParam(anim, "Shell"))
            anim.SetBool("Shell", shellReloading);

        if (v == 0 && Mathf.Abs(anim.GetFloat("Vertical")) < 0.05f)
            anim.SetFloat("Vertical", 0);

        if (h == 0 && Mathf.Abs(anim.GetFloat("Horizontal")) < 0.05f)
            anim.SetFloat("Horizontal", 0);
    }

    void SetStates()
    {
        shellReloading = (currentWeapon && !currentWeapon.haveLoader && currentWeapon.canReload && currentWeapon.CheckAmmo() > 0);

        reloading = anim.GetCurrentAnimatorStateInfo(2).IsTag("Reload") || anim.GetAnimatorTransitionInfo(2).IsName("Reload");

        attacking = anim.GetAnimatorTransitionInfo(2).IsUserName("Attack") || anim.GetCurrentAnimatorStateInfo(2).IsTag("Attack");

        uncontrol = reloading || anim.GetCurrentAnimatorStateInfo(0).IsTag("Uncontrol") || anim.GetAnimatorTransitionInfo(0).IsName("Uncontrol")
            || anim.GetCurrentAnimatorStateInfo(2).IsTag("Uncontrol") || anim.GetAnimatorTransitionInfo(2).IsName("Uncontrol") ||
            anim.GetCurrentAnimatorStateInfo(2).IsTag("Action") || anim.GetAnimatorTransitionInfo(2).IsName("Action");

        undamage = !attacking && !reloading && !uncontrol && (!anim.GetCurrentAnimatorStateInfo(2).IsName("New State") || anim.IsInTransition(2));

        ikOff = anim.GetCurrentAnimatorStateInfo(2).IsTag("Action") || anim.GetAnimatorTransitionInfo(2).IsName("Action") ||
            reloading || undamage;

        aiming = (!anim.IsInTransition(0) && anim.GetCurrentAnimatorStateInfo(0).IsName("Aim") ||
            anim.IsInTransition(0) && anim.GetAnimatorTransitionInfo(0).IsUserName("Aim"));
    }

    private void OnTriggerStay(Collider other)
    {
        if (GameManager.instance.gameStatus != GameStatus.Game && GameManager.instance.gameStatus != GameStatus.Inventory) return;

        if (other.GetComponent<InteractibleBase>())
        {
            if (other.GetComponent<InteractibleBase>().canInteract)
            {
                Vector3 Ref = new Vector3(other.GetComponent<InteractibleBase>().angleRef.position.x,
                    transform.position.y,
                    other.GetComponent<InteractibleBase>().angleRef.position.z);
                float angle = Mathf.Abs(Vector3.Angle((Ref) - transform.position, transform.forward));

                if (angle <= 0.15f * 360)
                {
                    interaction = other.GetComponent<InteractibleBase>();
                }
                else if (interaction == other.GetComponent<InteractibleBase>())
                {
                    interaction = null;
                }
            }
            else
            {
                interaction = null;
            }
        }

        if (other.GetComponent<ItemBase>())
        {
            Vector3 Ref = new Vector3(other.transform.position.x,
                    transform.position.y,
                    other.transform.position.z);

            float angle = Mathf.Abs(Vector3.Angle((Ref) - transform.position, transform.forward));

            if (angle <= 0.14f * 360)
            {
                currentItem = other.GetComponent<ItemBase>();
            }
            else if (currentItem == other.GetComponent<ItemBase>())
            {
                currentItem = null;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<InteractibleBase>() && other.GetComponent<InteractibleBase>() == interaction)
        {
            interaction = null;
        }

        if (other.GetComponent<ItemBase>() && other.GetComponent<ItemBase>() == currentItem)
        {
            currentItem = null;
        }
    }

    #endregion

    #region Enemies

    public void InitAim()
    {
        SearchEnemiesInScene();
        if (ClosestEnemy())
            StartCoroutine(Target(ClosestEnemy().transform));
    }

    public void SearchEnemiesInScene()
    {
        enemiesInScene = FindObjectsOfType(typeof(EnemyBase)) as EnemyBase[];
    }

    EnemyBase ClosestEnemy()
    {
        List<float> distances = new List<float>();

        distances.Clear();

        foreach (EnemyBase enemy in enemiesInScene)
        {
            if (!enemy.dead)
            {
                float dist = Vector3.Distance(transform.position + transform.up * 1.5f,
                    enemy.aimTarget.position);
                distances.Add(dist);
            }
        }

        distances.Sort();

        int x = 0;
        for (int i = 0; i < enemiesInScene.Length; i++)
        {
            if (!enemiesInScene[i].dead)
            {
                Vector3 origin = transform.position + transform.up * 1.5f;
                Vector3 direction = enemiesInScene[i].aimTarget.position;

                if (distances[x] == Vector3.Distance(origin, direction))
                {
                    RaycastHit hit;

                    if (Physics.Linecast(origin, direction, out hit, FXController.instance.layers.playerCheckForEnemy))
                    {
                        if (x < enemiesInScene.Length - 1)
                        {
                            i = -1;

                            if (x >= distances.Count)
                                return null;
                            else
                                x++;
                        }
                    }
                    else
                        return enemiesInScene[i];
                }
            }
        }
        return null;
    }

    IEnumerator Target(Transform target)
    {
        if (target != null)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            dir.y = 0;
            Quaternion rotTo = Quaternion.LookRotation(dir);

            while (true && aim)
            {
                Quaternion NewRotation = Quaternion.RotateTowards(transform.rotation, rotTo, speedAim);

                transform.rotation = NewRotation;

                if (Vector3.Angle(transform.forward, dir) < 1)
                    break;

                yield return null;
            }
        }
    }

    void SeachTarget()
    {
        List<EnemyBase> enemiesInView = new List<EnemyBase>();

        enemiesInView.Clear();
        enemyAimed = null;

        for (int i = 0; i < enemiesInScene.Length; i++)
        {
            if (enemiesInScene[i] && !enemiesInScene[i].dead)
            { 
                RaycastHit hit;
                Vector3 origin = transform.position + transform.up * 1.5f;
                Vector3 direction = enemiesInScene[i].aimTarget.position;

                float angle = Mathf.Abs(Vector3.Angle((enemiesInScene[i].transform.position) - transform.position, transform.forward));

                if (angle <= angleToAim * 360)
                {
                    if (Physics.Linecast(origin, direction, out hit, FXController.instance.layers.playerCheckForEnemy))
                        Debug.DrawLine(origin, direction, Color.red);
                    else 
                    {
                        Debug.DrawLine(origin, direction, Color.green);
                        enemiesInView.Add(enemiesInScene[i]);

                        if (enemiesInView.Count == 1)
                            enemyAimed = enemiesInScene[i];
                        else if (enemiesInView.Count > 1)
                            enemyAimed = ClosestEnemy();
                        mTransform.LookAt(enemyAimed.aimTarget);
                    }
                }
                else
                    Debug.DrawLine(origin, direction, Color.red);
            }
        }
    }

    #endregion

    #region Weapons

    public void EquipWeapon(WeaponBase weapon)
    {
        if (ExtensionMethods.ContainsParam(anim, "Fire"))
            anim.ResetTrigger("Fire");

        foreach (Transform child in weaponPivot)
        {
            if (child)
            {
                child.gameObject.SetActive(false);
                child.parent = null;
            }
        }

        if (weapon != currentWeapon)
        {
            currentWeapon = weapon;
            weapon.gameObject.SetActive(true);
            if (weapon.GetComponent<Collider>())
                weapon.GetComponent<Collider>().enabled = false;
            weapon.transform.parent = weaponPivot;
            weapon.transform.localPosition = weapon.positions.equipPosition;
            weapon.transform.localEulerAngles = weapon.positions.equipRotation;

            rightHandTarget.localPosition = weapon.positions.handAimPosition;
            rightHandTarget.localRotation = Quaternion.Euler(weapon.positions.handAimRotation);
            rightElbowTarget.localPosition = weapon.positions.elbowPosition;

            leftHandTarget = null;
            leftHandWeight = 0;
            anim.runtimeAnimatorController = weapon.runtimeAnimatorController;

            if (weapon.leftHandPos)
            {
                leftHandTarget = weapon.leftHandPos;
                if(weapon.TwoHandAimOff)
                    leftHandWeight = 1;
            }
        }
        else
        {
            currentWeapon = null;
            leftHandTarget = null;
        }
    }

    public void ChangeWeapon(WeaponBase weapon)
    {
        EquipWeapon(weapon);

        anim.Play("Take Gun", 2);
    }

    void WeaponInBody(WeaponBase wp, Transform pivot)
    {
        if (pivot)
        {
            foreach (Transform child in pivot)
            {
                if (child)
                {
                    child.gameObject.SetActive(false);
                    child.parent = null;
                }
            }

            wp.GetComponent<Collider>().enabled = false;
            wp.gameObject.SetActive(true);
            wp.transform.parent = pivot;
            wp.transform.localPosition = wp.positions.storePosition;
            wp.transform.localEulerAngles = wp.positions.storeRotation;
        }
        else
        {
            wp.gameObject.SetActive(false);
        }
    }

    public void WeaponsInBody()
    {
        bool side = false;
        bool back = false;

        SlotItemBehaviour[] slots = InventoryUI.instance.slots;
        for (int i = 0; i < InventoryUI.instance.slotsAvailable; i++)
        {
            if (slots[i].currentItem && slots[i].currentItem.type == ItemType.Weapon)
            {
                if (slots[i].currentItem.transform.parent != weaponPivot)
                {
                    if (slots[i].currentItem.GetComponent<WeaponBase>().store == StorePoint.Side)
                    {
                        WeaponInBody(slots[i].currentItem.GetComponent<WeaponBase>(), pivots[0]);
                        side = true;
                    }
                    else if (slots[i].currentItem.GetComponent<WeaponBase>().store == StorePoint.Back)
                    {
                        WeaponInBody(slots[i].currentItem.GetComponent<WeaponBase>(), pivots[1]);
                        back = true;
                    }
                }
            }
        }

        if (!side)
        {
            foreach (Transform child in pivots[0])
            {
                if (child)
                {
                    child.gameObject.SetActive(false);
                    child.parent = null;
                }
            }
        }
        if (!back)
        {
            foreach (Transform child in pivots[1])
            {
                if (child)
                {
                    child.gameObject.SetActive(false);
                    child.parent = null;
                }
            }
        }
    }

    public void Fire()
    {
        if (currentWeapon.weaponType != WeaponType.Melee)
        {
            aimPivot.GetComponent<Animator>().Play(currentWeapon.shotAnimation, 0, 0);
            currentWeapon.Fire();

            float x = Random.Range(-0.5f, 0.5f);
            transform.Rotate(new Vector3(0, x * 100 * Time.deltaTime, 0));
        }
        else
        {
            if (anim.GetCurrentAnimatorStateInfo(2).IsName("New State") && !anim.IsInTransition(2))
            {
                currentWeapon.currentTimeToSHot = currentWeapon.fireRate;
                anim.SetTrigger("Fire");
            }
        }
    }

    public void Attack()
    {
        if(currentWeapon.weaponType == WeaponType.Melee && !undamage)
            currentWeapon.Fire();
    }

    public void StartReload()
    {
        if (aimPivot.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Empty"))
        {
            anim.CrossFade(currentWeapon.reloadAnimation, 0.25f, 2);
            StartCoroutine(currentWeapon.StartReload());
        }
    }

    public void Pump()
    {
        currentWeapon.PumpAudio();
    }
    #endregion

    #region Ik
    private void OnAnimatorIK(int layerIndex)
    {
        if (!dead)
        {
            if (!reloading && !uncontrol && !undamage)
            {
                float weight = (currentWeapon && currentWeapon.weaponType == WeaponType.Melee) ? bodyWeightMeleeIntensity : bodyWeightIntensity;

                anim.SetLookAtPosition(lookDir.position);
                anim.SetLookAtWeight(bodyWeight, weight, headWeightIntensity, 0.2f, 0.5f);
            }

            UpdateIK(AvatarIKGoal.RightHand, rightHandTarget, rightHandWeight);
            UpdateIK(AvatarIKHint.RightElbow, rightElbowTarget, rightElbowWeight);

            if (leftHandTarget != null)
                UpdateIK(AvatarIKGoal.LeftHand, leftHandTarget, leftHandWeight);
        }
    }

    void UpdateIK(AvatarIKHint goal, Transform t, float w)
    {
        anim.SetIKHintPosition(goal, t.position);
        anim.SetIKHintPositionWeight(goal, w);
    }

    void UpdateIK(AvatarIKGoal goal, Transform t, float w)
    {
        anim.SetIKPositionWeight(goal, w);
        anim.SetIKRotationWeight(goal, w);
        anim.SetIKPosition(goal, t.position);
        anim.SetIKRotation(goal, t.rotation);
    }

    void UpdateIkAim()
    {
        Vector3 lookDir = mTransform.forward * 5f;

        aimPivot.position = shoulder.position;

        Vector3 targetDir = lookDir;

        if (targetDir == Vector3.zero)
            targetDir = aimPivot.forward;

        Quaternion tr = Quaternion.LookRotation(targetDir);
        aimPivot.rotation = Quaternion.Lerp(aimPivot.rotation, tr, Time.deltaTime * 5);
    }

    void SetWeightLook()
    {
        if (!ikOff)
        {
            //valores do olhar
            bodyWeight = (aiming) ? Mathf.Lerp(bodyWeight, 0.7f, 2 * Time.deltaTime) : Mathf.Lerp(bodyWeight, 0, 2 * Time.deltaTime);

            if (currentWeapon != null)
            {
                //valores da mao esquerda
                if (!currentWeapon.TwoHandAimOff)
                {
                    if (aiming)
                        leftHandWeight = Mathf.Lerp(leftHandWeight, 1f, 2f * Time.deltaTime);
                    else
                        leftHandWeight = Mathf.Lerp(leftHandWeight, 0.0f, 15f * Time.deltaTime);
                }
                else
                    leftHandWeight = Mathf.Lerp(leftHandWeight, 1f, 10f * Time.deltaTime);


                //valores da mao direita e cotovelo
                if (currentWeapon.TwoHandAimOn)
                {
                    if (aiming)
                    {
                        rightHandWeight = Mathf.Lerp(rightHandWeight, 1f, 2f * Time.deltaTime);
                        rightElbowWeight = Mathf.Lerp(rightElbowWeight, 1f, 2f * Time.deltaTime);
                    }
                    else
                    {
                        rightHandWeight = Mathf.Lerp(rightHandWeight, 0.0f, 10f * Time.deltaTime);
                        rightElbowWeight = Mathf.Lerp(rightElbowWeight, 0.0f, 10f * Time.deltaTime);
                    }
                }
                else
                {
                    rightHandWeight = Mathf.Lerp(rightHandWeight, 0.0f, 10f * Time.deltaTime);
                    rightElbowWeight = Mathf.Lerp(rightElbowWeight, 0.0f, 10f * Time.deltaTime);
                }
            }
        }
        else
        {
            bodyWeight = 0;
            leftHandWeight = 0;
            rightHandWeight = 0;
            rightElbowWeight = 0;
        }
    }
    #endregion

    #region Physics

    float pushPower = 4.0f;
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        CharacterController chara = hit.controller;

        if (body == null || chara == null || body.isKinematic)
        {
            return;
        }

        if (hit.moveDirection.y < -0.3)
        {
            return;
        }

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        body.velocity = pushDir * pushPower;
    }

    public void SetPosition(Transform destiny)
    {
        GetComponent<CharacterController>().enabled = false;
        transform.position = destiny.position;
        transform.eulerAngles = destiny.eulerAngles;
        GetComponent<CharacterController>().enabled = true;
    }

    #endregion

    public void UnlockInteraction(string keyName)
    {
        interaction.Unlock();
        GameManager.instance.ChangeStateToGame();
        MessagesBehaviour.instance.SendMessageTxt(MessagesBehaviour.instance.youUsedThe.msgs[Settings.instance.currentLanguage] + " ");
        MessagesBehaviour.instance.AddRich(keyName, "<color=lime>", "</color>");
        MessagesBehaviour.instance.AddRich(".");
    }

    #region Save & Load

    public void Save()
    {
        PlayerPrefs.SetFloat("Player Life", currentLife);

        if(flashlightEnable)
            PlayerPrefs.SetInt("Flash On", 1);
        else
            PlayerPrefs.SetInt("Flash On", 0);

        if (currentWeapon)
        {
            for (int i = 0; i < InventoryUI.instance.slotsAvailable; i++)
            {
                if (InventoryUI.instance.slots[i] && InventoryUI.instance.slots[i].currentItem && InventoryUI.instance.slots[i].currentItem.GetComponent<WeaponBase>())
                {
                    if (InventoryUI.instance.slots[i].currentItem.GetComponent<WeaponBase>() == currentWeapon)
                    {
                        PlayerPrefs.SetInt("Current Weapon", i);
                        break;
                    }
                }
            }
        }
        else
        {
            PlayerPrefs.DeleteKey("Current Weapon");
        }
    }

    bool initFlashlight = false;

    IEnumerator InitFlashlight()
    {
        Camera cam = Camera.main;
        Vector3 initPos = flashLight.transform.localPosition;
        flashLight.transform.localPosition = cam.transform.position + Vector3.forward * 10;

        LensFlareBehaviour lensFlare = GetComponent<LensFlareBehaviour>();
        lensFlare.enabled = false;
        Vector3 flareInitPos = lensFlare.flash.transform.localPosition;

        for (int i = 0; i < 3; i++)
        {
            flashLight.enabled = true;
            lensFlare.flash.SetActive(true);
            yield return new WaitForEndOfFrame();
            flashLight.enabled = false;
            lensFlare.flash.SetActive(false);
            yield return new WaitForEndOfFrame();
        }

        lensFlare.enabled = true;
        flashLight.transform.localPosition = initPos;
        lensFlare.flash.transform.localPosition = flareInitPos;
        initFlashlight = true;
    }

    public void Load()
    {
        currentLife = PlayerPrefs.GetFloat("Player Life");
        if (currentLife == 0) currentLife = maxLife;

        int value = (PlayerPrefs.HasKey("Flash On")) ? PlayerPrefs.GetInt("Flash On") : 0;

        flashlightEnable = (value == 1) ? true : false;
        StartCoroutine(InitFlashlight());

        if (PlayerPrefs.HasKey("Current Weapon") && InventoryUI.instance.slots[PlayerPrefs.GetInt("Current Weapon")].currentItem.GetComponent<WeaponBase>())
            EquipWeapon(InventoryUI.instance.slots[PlayerPrefs.GetInt("Current Weapon")].currentItem.GetComponent<WeaponBase>());

        if (!currentWeapon && anim.runtimeAnimatorController != unarmedAnimator)
            anim.runtimeAnimatorController = unarmedAnimator;

        WeaponsInBody();
    }

    #endregion

    public override void OnDamage(float damage)
    {
        aim = false;
        anim.ResetTrigger("Fire");
        if (currentWeapon) currentWeapon.ResetAnimations();

        Vector3 direction = lastHitPos - transform.position;
        float angle = Vector3.Angle(direction, transform.forward);

        FXController.instance.SpawnBloodEffect(anim.GetBoneTransform(HumanBodyBones.LeftShoulder).position,
            anim.GetBoneTransform(HumanBodyBones.LeftShoulder).position);

        anim.CrossFade((angle <= 90) ? "Damage Front" : "Damage Back", 0.1f, 2);

        if(CamPos.instance)
            CamPos.instance.ImpulseNoise();
    }

    #region Events

    public void Reload()
    {
        aim = false;
        currentWeapon.Reload();
    }

    public void TakeItem()
    {
        if (currentItem)
        {
            currentItem.Add();
            currentItem = null;
        }
    }

    #endregion

    public override void OnDie()
    {
        if (currentWeapon) currentWeapon.ResetAnimations();
        aim = false;
        anim.CrossFade("Die", 0.1f, 2);
        Invoke("Dead", 6);
    }

    void Dead()
    {
        GameManager.instance.StateToDie();
    }
}
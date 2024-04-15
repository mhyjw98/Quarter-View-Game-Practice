using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades;
    public int hasGrenades;
    public GameObject grenadeObj;
    public Camera followCamera;
    public GameManager manager;

    public AudioSource playerMove;
    public AudioSource playerDodge;
    public AudioSource playerJump;
    public AudioSource playerSwap;
    public AudioSource playerReload;
    public AudioSource playerReloadOut;
    public AudioSource playerGrenade;
    public AudioSource playerItem;
    public AudioSource playerDamage;
    public AudioSource playerDie;
    public AudioSource playerShop;

    public int ammo;
    public int coin;
    public int health;
    public int score;
    
    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxGrenades;

    float hAxis;
    float vAxis;

    bool wDown;
    bool jDown;
    bool iDown;
    bool rDown;
    bool fDown;
    bool gDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;

    bool isMove;
    bool isJump;
    bool isDodge;
    bool isSwap;
    bool isReload;
    bool isFireReady = true;
    bool isBorder;
    bool isDamaged;
    bool isDead;
    bool isPlayingFootsteps;
    public bool isShop;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Animator anim;
    Rigidbody rigid;
    MeshRenderer[] meshs;

    GameObject nearObject;
    public Weapon equipWeapon;
    int equipWeaponIndsx = -1;
    float fireDelay;
    float footStepTimer = 0;
    float footStepDelay;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        //PlayerPrefs.SetInt("MaxScore", 112500);
    }


    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Grenade();
        Attack();
        Reload();
        Dodge();
        Swap();
        Interation();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        fDown = Input.GetButton("Fire1");
        gDown = Input.GetButtonDown("Fire2");
        rDown = Input.GetButtonDown("Reload");
        iDown = Input.GetButtonDown("Interation");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
    }

    void Move()
    {
        isMove = true;
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
            moveVec = dodgeVec;

        if (isSwap || isReload || !isFireReady || isDead)
            moveVec = Vector3.zero;

        if(!isBorder)
             transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

        if (moveVec != Vector3.zero && !isJump && !isDodge && !isPlayingFootsteps)
        {
            isPlayingFootsteps = true;
            playerMove.Play();
        }

        if (isPlayingFootsteps && !isJump && !isDodge)
        {
            if (wDown)
                footStepDelay = 0.8f;
            else
                footStepDelay = 0.3f;
            footStepTimer += Time.deltaTime;
            if (footStepTimer > footStepDelay)
            {
                footStepTimer = 0f;
                playerMove.Play();
            }

        }
        else
        {
            playerMove.Stop();
        }

        if (moveVec == Vector3.zero && isPlayingFootsteps)
        {
            playerMove.Stop();
            isMove = false;
        }
            

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVec);

        if (fDown && !isMove && !isDead)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }
        }  
    }

    void Jump()
    {
        if(jDown && !isJump && moveVec == Vector3.zero && !isDodge && !isSwap && !isDead)
        {
            rigid.AddForce(Vector3.up * 15, ForceMode.Impulse);
            playerJump.Play();
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Grenade()
    {
        if(hasGrenades == 0)
            return;

        if(gDown && !isReload && !isSwap && !isDead)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 15;
                
                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);
                playerGrenade.Play();

                hasGrenades--;
                grenades[hasGrenades].SetActive(false);
            }
        }
    }

    void Attack()
    {
        if(equipWeapon == null)
                return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if(fDown && isFireReady && !isDodge && !isSwap && !isShop && !isDead)
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }

    void Reload()
    {       
        if (equipWeapon == null)
            return;

        if (equipWeapon.type == Weapon.Type.Melee)
            return;

        if (ammo == 0 || equipWeapon.curAmmo == equipWeapon.maxAmmo)
            return;

        if(rDown && !isJump && !isDodge && !isSwap && isFireReady && !isDead)
        {
            anim.SetTrigger("doReload");
            isReload = true;
            Invoke("ReloadOut", 2f);

            StartCoroutine(ReloadSound());
        }
    }
    void ReloadOut()
    {
        playerReloadOut.Play();
        int reAmmo = equipWeapon.maxAmmo - equipWeapon.curAmmo;

        if(ammo < reAmmo )
            reAmmo = ammo;

        equipWeapon.curAmmo +=  reAmmo;
        ammo -= reAmmo;
        isReload = false;
    }

    IEnumerator ReloadSound()
    {
        playerReload.Play();
        yield return new WaitForSeconds(2);
        playerReload.Stop();
    }
    void Dodge()
    {
        if (jDown && !isJump && moveVec != Vector3.zero && !isDodge && !isSwap && !isDead)
        {
            dodgeVec = moveVec;
            speed *= 2;
            anim.SetTrigger("doDodge");
            playerDodge.Play();
            isDodge = true;

            Invoke("DodgeOut",0.5f);
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    void Swap()
    {
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndsx == 0))
            return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndsx == 1))
            return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndsx == 2))
            return;

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if ((sDown1 ||  sDown2 || sDown3) && !isJump && !isDodge && !isDead)
        {
            if(equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);

            equipWeaponIndsx = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);
            playerSwap.Play();

            anim.SetTrigger("doSwap");

            isSwap = true;

            Invoke("SwapOut", 0.4f);
        }
    }

    void SwapOut()
    {
        isSwap = false;
    }
    void Interation()
    {
        if(iDown && nearObject != null && !isJump && !isDodge && !isDead)
        {
            if(nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                playerItem.Play();
                Destroy(nearObject);
            }
            else if(nearObject.tag == "Shop")
            {
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Enter(this);
                isShop = true;
                playerShop.Play();
            }
        }
    }
    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
    }

    private void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }         
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Physics.IgnoreCollision(other, GetComponent<CapsuleCollider>());
            Item item = other.GetComponent<Item>();
            switch(item.type)
            {
                case Item.Type.Ammo:                  
                    ammo += item.value;
                    if(ammo > maxAmmo) 
                        ammo = maxAmmo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                        coin = maxCoin;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                        health = maxHealth;
                    break;
                case Item.Type.Grenade:
                    if (hasGrenades < maxGrenades)
                    {
                        grenades[hasGrenades].SetActive(true);
                        hasGrenades += item.value;
                    }                   
                    break;
            }
            playerItem.Play();
            Destroy(other.gameObject);
        }
        else if (other.tag == "EnemyBullet")
        {
            if (!isDamaged)
            {
                Physics.IgnoreCollision(other, GetComponent<CapsuleCollider>());
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;
                playerDamage.Play();

                bool isBossAtk = other.name == "Boss Melee Area";
                StartCoroutine(Ondamge(isBossAtk));
            }
        }
    }

    IEnumerator Ondamge(bool isBossAtk)
    {
        isDamaged = true;
        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.yellow;
        }
        if (isBossAtk)
            rigid.AddForce(transform.forward * -25, ForceMode.Impulse);

        if (health <= 0 && !isDead)
        {
            OnDie();
        }

        yield return new WaitForSeconds(1f);

        isDamaged = false;
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }

        if (isBossAtk)
            rigid.velocity = Vector3.zero;    
    }

    void OnDie()
    {
        anim.SetTrigger("doDie");
        isDead = true;
        playerDie.Play();
        manager.GameOver();
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Weapon" || other.tag == "Shop")
            nearObject = other.gameObject;
           
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = null;
        else if (other.tag == "Shop")
        {        
            if (isShop)
            {
                Physics.IgnoreCollision(other, GetComponent<CapsuleCollider>());
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Exit();
                isShop = false;
                nearObject = null;
                shop.isSoundPlay = false;
            }
        }
    }
}

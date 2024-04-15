using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C, D };
    public Type enemyType;
    public int maxHealth;
    public int curHealth;
    public int score;
    public GameManager manager;
    public Transform target;
    public BoxCollider meleeArea;
    public GameObject bullet;
    public GameObject[] coins;
    public bool isChase;
    public bool isAttack;
    public bool isDead;

    public AudioSource enemyAttackASound;
    public AudioSource enemyAttackBSound;
    public AudioSource enemyAttackCSound;
    public AudioSource enemyAttackDSound;
    public AudioSource enemyDamageSound;
    public AudioSource enemyDieSound;

    public Rigidbody rigid;
    public BoxCollider boxCollider;
    public MeshRenderer[] meshs;
    public NavMeshAgent nav;
    public Animator anim;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        if(enemyType != Type.D)
            Invoke("ChaseStart", 2);
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }
    private void Update()
    {
        if (nav.enabled && enemyType != Type.D)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }        
    }


    void FreezeVelocity()
    {
        if (isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }     
    }

    void Targeting()
    {
        if(!isDead && enemyType != Type.D)
        {
            float targetRadius = 0;
            float targetRange = 0;

            switch (enemyType)
            {
                case Type.A:
                    targetRadius = 1.5f;
                    targetRange = 3f;
                    break;
                case Type.B:
                    targetRadius = 1f;
                    targetRange = 12f;
                    break;
                case Type.C:
                    targetRadius = 0.5f;
                    targetRange = 25f;
                    break;
            }

            RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));

            if (rayHits.Length > 0 && !isAttack)
            {
                StartCoroutine(Attack());
            }
        }    
    }

    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true );

        switch (enemyType)
        {
            case Type.A:
                yield return new WaitForSeconds(1f);
                enemyAttackASound.Play();
                meleeArea.enabled = true;

                yield return new WaitForSeconds(0.8f);
                meleeArea.enabled = false;

                yield return new WaitForSeconds(0.5f);
                break;
            case Type.B:
                yield return new WaitForSeconds(0.1f);
                enemyAttackBSound.Play();
                rigid.AddForce(transform.forward*20,ForceMode.Impulse);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;

                yield return new WaitForSeconds(2f);
                break;
            case Type.C:
                yield return new WaitForSeconds(0.5f);
                enemyAttackCSound.Play();
                GameObject instantBullet = Instantiate(bullet, transform.position + new Vector3 (0, 2.5f, 0), transform.rotation);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward*20;

                yield return new WaitForSeconds(2f);
                break;
        }

        

        isChase = true;
        isAttack = false;
        anim.SetBool("isAttack", false);
    }

    private void FixedUpdate()
    {
        Targeting();
        FreezeVelocity();
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Melee") 
        {
            Weapon weapon = other.GetComponent<Weapon>();
            if (curHealth > 0)
            {
                curHealth -= weapon.damage;
                if(curHealth < 0) 
                    curHealth = 0;
            }           
            Vector3 reactVec = transform.position - other.transform.position;

            StartCoroutine(OnDamage(reactVec, false));
        }
        else if(other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (curHealth > 0)
            {               
                curHealth -= bullet.damage;
                if (curHealth < 0)
                    curHealth = 0;
            }
                
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject);

            StartCoroutine(OnDamage(reactVec, false));
        }
    }

    public void HitByGrenade(Vector3 explosionPos)
    {
        if (curHealth > 0)
        {
            curHealth -= 200;
            if (curHealth < 0)
                curHealth = 0;
        }
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec, true));
    }

    IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
    {
        foreach(MeshRenderer mesh in meshs)
            mesh.material.color = Color.red;
        
        if(curHealth > 0)
        {
            enemyDamageSound.Play();
            yield return new WaitForSeconds(0.1f);
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.white;
        }
        else
        {
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.gray;

            if(enemyType != Type.C)
                meleeArea.enabled = false;
            if (!isDead)
            {
                enemyDieSound.Play();
                Player player = target.GetComponent<Player>();
                player.score += score;
                int coinValue = 1 + (manager.stage / 5);
                if (enemyType == Type.D)
                    coinValue *= 3;
                for(int i = 0; i < coinValue; i++)
                {
                    int ranCoin = Random.Range(0, 3);
                    Instantiate(coins[ranCoin], transform.position, Quaternion.identity);
                }             
            }               
            gameObject.layer = 14;      
            isChase = false;
            nav.enabled = false;
            anim.SetTrigger("doDie");
                      
            

            if (!isDead)
            {
                switch (enemyType)
                {
                    case Type.A:
                        manager.enemyCntA--;
                        break;
                    case Type.B:
                        manager.enemyCntB--;
                        break;
                    case Type.C:
                        manager.enemyCntC--;
                        break;
                    case Type.D:
                        manager.enemyCntD--;
                        break;
                }
            }
            
            if(isGrenade)
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up*3;

                rigid.freezeRotation = false;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
                rigid.AddTorque(reactVec * 15, ForceMode.Impulse);
            }
            else
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
            }

            isDead = true;
            Destroy(gameObject,4);
        }
    }
}

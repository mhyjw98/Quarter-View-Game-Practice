using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy
{
    public GameObject missile;
    public Transform missilePortA;
    public Transform missilePortB;
    public bool isLook;

    Vector3 lookVec;
    Vector3 tauntVec;
    

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        nav.isStopped = true;
        StartCoroutine(Wait());
        StartCoroutine(Think());
    }

    void Update()
    {
        if(isDead)
        {
            StopAllCoroutines();
            return;
        }
           
        if (isLook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            lookVec = new Vector3(h, 0, v) * 5f;
            transform.LookAt(target.position + lookVec);
        }
        else
            nav.SetDestination(tauntVec);
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(5f);
    }
    IEnumerator Think()
    {
        yield return new WaitForSeconds(0.1f);

        int ranAction = Random.Range(0, 5);
        switch(ranAction)
        {
            case 0:
            case 1:
                // �̻��� �߻� ����
                StartCoroutine(MissileShot());
                break;
            case 2:
            case 3:
                // �� ������ ����
                StartCoroutine(RockShot());
                break;
            case 4:
                // ���� ���� ����
                StartCoroutine(Taunt());
                break;
        }
    }

    IEnumerator MissileShot()
    {
        // �ִϸ��̼� �۵�
        anim.SetTrigger("doShot");
        yield return new WaitForSeconds(0.2f);
        // ���� ���
        enemyAttackCSound.Play();
        // �̻���A ����
        GameObject instantMissileA = Instantiate(missile, missilePortA.position, missilePortA.rotation);
        BossMissile bossMissileA = instantMissileA.GetComponent<BossMissile>();
        // �̻��Ͽ� Ÿ�� ����
        bossMissileA.target = target;

        yield return new WaitForSeconds(0.3f);
        // ���� ���
        enemyAttackCSound.Play();
        // �̻���B ����
        GameObject instantMissileB = Instantiate(missile, missilePortB.position, missilePortB.rotation);
        BossMissile bossMissileB = instantMissileB.GetComponent<BossMissile>();
        // �̻��Ͽ� Ÿ�� ����
        bossMissileB.target = target;

        // 2.5���� �̻��� ����
        
        yield return new WaitForSeconds(2.5f);
        if (bossMissileA != null)
            StartCoroutine(bossMissileA.MissileExplosion());
        if (bossMissileB != null)
            StartCoroutine(bossMissileB.MissileExplosion());

        StartCoroutine(Think());
    }

    IEnumerator RockShot()
    {
        isLook = false;
        anim.SetTrigger("doBigShot");
        enemyAttackDSound.Play();
        Instantiate(bullet, transform.position, transform.rotation);

        yield return new WaitForSeconds(3f);
        isLook = true;

        StartCoroutine(Think());
    }

    IEnumerator Taunt()
    {
        tauntVec = target.position + lookVec;

        enemyAttackBSound.Play();
        isLook = false;
        nav.isStopped = false;
        boxCollider.enabled = false;
        anim.SetTrigger("doTaunt");

        yield return new WaitForSeconds(1.5f);
        enemyAttackASound.Play();
        meleeArea.enabled = true;

        yield return new WaitForSeconds(1f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(0.5f);
        isLook = true;
        nav.isStopped = true;
        boxCollider.enabled = true;

        StartCoroutine(Think());
    }
}

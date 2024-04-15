using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossMissile : Bullet
{
    public Transform target;
    public GameObject meshObj;
    public GameObject missileEffect;
    public GameObject effectObj;
    public new BoxCollider collider;
    NavMeshAgent nav;

    bool isExplosion;

    public AudioSource explosionSound;

    void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        nav.SetDestination(target.position);
    }

    void OnCollisionEnter(Collision collision)
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Wall"))
        {
            StartCoroutine(MissileExplosion());
        }
    }

    public IEnumerator MissileExplosion()
    {
        if (!isExplosion)
        {
            isExplosion = true;
            nav.isStopped = true;
            nav.velocity = Vector3.zero;
            effectObj.SetActive(true);
            explosionSound.Play();
            collider.enabled = false;
            meshObj.SetActive(false);
            missileEffect.SetActive(false);

            yield return new WaitForSeconds(1f);
            if (gameObject != null)
                Destroy(gameObject);
        }
    }
}

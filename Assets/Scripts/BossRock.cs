using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRock : Bullet
{
    Rigidbody rigid;
    float angularPower = 2;
    float scaleValue = 0.1f;
    bool isShoot;

    public AudioSource shootSound;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        StartCoroutine(GainpowerTimer());
        StartCoroutine(GainPower());
    }

    IEnumerator GainpowerTimer()
    {
        yield return new WaitForSeconds(2.5f);
        shootSound.Play();
        isShoot = true;
    }

    IEnumerator GainPower()
    {
        while(!isShoot)
        {
            angularPower += 0.02f;
            scaleValue += 0.0013f;
            transform.localScale = Vector3.one * scaleValue;
            rigid.AddTorque(transform.right * angularPower, ForceMode.Acceleration);
            yield return null;
        }
    }
}

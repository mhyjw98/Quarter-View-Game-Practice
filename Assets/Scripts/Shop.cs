using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public RectTransform[] uiGrounds;
    public Animator anim;
    public Player player;

    public AudioSource buySound;
    public AudioSource LackSound;
    public AudioSource ExitSound;
    public AudioSource TabSound;

    public GameObject[] itemObj;
    public int[] itemPrice;
    public int[] itemUpgradePrice;
    public Transform[] itemPos;
    public Text talkSellText;
    public Text talkUpgradeText;
    public string[] talkData;
    public Text[] itemPriceText;
    public bool isSoundPlay;

    Player enterPlayer;

    public void Enter(Player player)
    {
        enterPlayer = player;
        uiGrounds[0].anchoredPosition = Vector3.zero;
    }

    public void Exit()
    {
        anim.SetTrigger("doHello");
        uiGrounds[0].anchoredPosition = Vector3.down * 1000;
        uiGrounds[1].anchoredPosition = Vector3.down * 1000;

        if(!isSoundPlay)
            ExitSound.Play();

        isSoundPlay = true;
    }

    public void Buy(int index)
    {
        if (index > itemObj.Length - 1)
            return;

        int price = itemPrice[index];
        if(price > enterPlayer.coin)
        {
            StopCoroutine(SellTalk());
            StartCoroutine(SellTalk());
            LackSound.Play();
            return;
        }
        else
        {
            enterPlayer.coin -= price;
            Vector3 ranVec = Vector3.right * Random.Range(-3, 3) + Vector3.forward * Random.Range(-3, 3);
            Instantiate(itemObj[index], itemPos[index].position + ranVec, itemPos[index].rotation);
            buySound.Play();
        }    
    }

    public void Upgrade(int index)
    {
        if (index > itemObj.Length - 1)
            return;

        if (!player.hasWeapons[index])
        {
            StopCoroutine(UpdateTalk(0));
            StartCoroutine(UpdateTalk(0));
            return;
        }
            
        int price = itemUpgradePrice[index];
        if(price > enterPlayer.coin)
        {
            StopCoroutine(UpdateTalk(1));
            StartCoroutine(UpdateTalk(1));
            return;
        }
        else
        {
            enterPlayer.coin -= price;

            if (index == 0)
            {
                player.weapons[0].GetComponent<Weapon>().damage += 10;
                itemUpgradePrice[0] += 1000;
                itemPriceText[0].text = string.Format("{0:n0}", itemUpgradePrice[0]);
            }                
            else if(index == 1)
            {
                GameObject bulletObject = player.weapons[1].GetComponent<Weapon>().bullet;
                Bullet bulletComponent = bulletObject.GetComponent<Bullet>();
                bulletComponent.damage += 5;
                player.weapons[1].GetComponent<Weapon>().curAmmo += 1;
                player.weapons[1].GetComponent<Weapon>().maxAmmo += 1;
                itemUpgradePrice[1] += 1000;
                itemPriceText[1].text = string.Format("{0:n0}", itemUpgradePrice[1]);
            }
                
            else if(index == 2)
            {
                GameObject bulletObject = player.weapons[2].GetComponent<Weapon>().bullet;
                Bullet bulletComponent = bulletObject.GetComponent<Bullet>();
                bulletComponent.damage += 3;
                player.weapons[2].GetComponent<Weapon>().curAmmo += 5;
                player.weapons[2].GetComponent<Weapon>().maxAmmo += 5;
                itemUpgradePrice[2] += 1000;
                itemPriceText[2].text = string.Format("{0:n0}", itemUpgradePrice[2]);
            }
        }
    }

    public void SellTabChange()
    {
        TabSound.Play();
        uiGrounds[0].anchoredPosition = Vector3.zero;
        uiGrounds[1].anchoredPosition = Vector3.down * 1000;
    }

    public void UpgradeTabChange()
    {
        TabSound.Play();
        uiGrounds[1].anchoredPosition = Vector3.zero;
        uiGrounds[0].anchoredPosition = Vector3.down * 1000;
    }

    IEnumerator SellTalk()
    {
        talkSellText.text = talkData[1];
        yield return new WaitForSeconds(2f);
        talkSellText.text = talkData[0];
    }

    IEnumerator UpdateTalk(int value)
    {
        if (value == 0)
            talkUpgradeText.text = talkData[3];
        if (value == 1)
            talkUpgradeText.text = talkData[1];

        yield return new WaitForSeconds(2f);

        talkUpgradeText.text = talkData[2];
    }
}

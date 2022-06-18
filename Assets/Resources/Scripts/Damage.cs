using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Photon.Pun;

public class Damage : MonoBehaviour
{
    [SerializeField] TMP_Text damageTMP;
    Transform tr;
    PhotonView PV;

    private void Awake()
    {
        this.PV = this.GetComponent<PhotonView>();
    }
    public void SetupTransform(Transform tra)
    {
        this.tr = tra;
    }
    public void Damaged(int damage)
    {
        if (PV)
            PV.RPC("damaged", RpcTarget.All,damage);
    }
    [PunRPC]
    void damaged(int damage)
    {
        if (damage <= 0)
            return;//0댐이하는 취급안함

        GetComponent<Order>().SetOriginOrder(1000);//맨앞으로

        damageTMP.text = $"-{damage}";//-damage로

        Sequence sequence = DOTween.Sequence()
            .Append(transform.DOScale(Vector3.one * 1.8f, 0.5f).SetEase(Ease.InOutBack))
            //0.5초간 스케일을 1.8로
            .AppendInterval(1.2f)//1.2초 대기
            .Append(transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutBack))
            //0.5초간 스케일을 0으로
            .OnComplete(
                () =>
                {
                    PhotonNetwork.Destroy(gameObject);//자멸
                }
            );
    }
    void Start()
    {
        
    }

    void Update()
    {
        if (tr != null)
            transform.position = tr.position;
    }
}

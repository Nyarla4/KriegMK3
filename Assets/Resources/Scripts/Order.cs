using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Order : MonoBehaviour
{
    [SerializeField] Renderer[] backRenderers;
    [SerializeField] Renderer[] middleRenderers;
    [SerializeField] string sortingLayerName;
    int originOrder;

    PhotonView PV;
    private void Awake()
    {
        this.PV = this.GetComponent<PhotonView>();
    }

    public void SetOriginOrder(int originOrder)//최초 설정
    {
        this.originOrder = originOrder;
        PV.RPC("SetOrder", RpcTarget.AllBuffered, originOrder);
    }

    public void SetMostFrontOrder(bool isMostFront)//선택시 맨앞으로
    {
        PV.RPC("SetOrder", RpcTarget.AllBuffered, isMostFront ? 100 : originOrder);//true라면 100, 아니라면 originOrder
    }

    [PunRPC]
    void SetOrder(int order)
    {
        int murOrder = order * 10;//카드 사이의 거리 10
        foreach (var renderer in backRenderers)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = murOrder;
        }

        foreach (var renderer in middleRenderers)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = murOrder + 1;//한칸 더 앞에
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

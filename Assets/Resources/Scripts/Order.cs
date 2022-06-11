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

    public void SetOriginOrder(int originOrder)//���� ����
    {
        this.originOrder = originOrder;
        PV.RPC("SetOrder", RpcTarget.AllBuffered, originOrder);
    }

    public void SetMostFrontOrder(bool isMostFront)//���ý� �Ǿ�����
    {
        PV.RPC("SetOrder", RpcTarget.AllBuffered, isMostFront ? 100 : originOrder);//true��� 100, �ƴ϶�� originOrder
    }

    [PunRPC]
    void SetOrder(int order)
    {
        int murOrder = order * 10;//ī�� ������ �Ÿ� 10
        foreach (var renderer in backRenderers)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = murOrder;
        }

        foreach (var renderer in middleRenderers)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = murOrder + 1;//��ĭ �� �տ�
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

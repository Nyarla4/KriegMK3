using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class roomPlayer : MonoBehaviour
{
    PhotonView PV;
    SpriteRenderer SR;
    float colorR;
    float colorG;
    float colorB;
    GameObject Name = null;

    bool isReady = false;
    bool isTurn = false;

    GameObject PlayPanel = null;

    Vector3 mousePos;

    CardData HQ;

    void Start()
    {
        PV = this.GetComponent<PhotonView>();
        SR = this.GetComponent<SpriteRenderer>();
        colorR = colorG = colorB = 1.0f;
        Name = Instantiate(Resources.Load("Prefabs/lobby/Name") as GameObject, GameObject.Find("RoomPanel").transform);

        PlayPanel = GameObject.Find("Canvas").transform.GetChild(5).gameObject;

        PV.RPC("layerSet", RpcTarget.AllBuffered);
    }

    // Update is called once per frame
    void Update()
    {
        if (PV.IsMine)
        {
            Vector3 mouseOrigin = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
            mousePos = Camera.main.ScreenToWorldPoint(mouseOrigin);
            if (Input.GetKeyDown(KeyCode.R))
            {
                colorR = 1.0f;
                colorG = 0.0f;
                colorB = 0.0f;
            }
            else if (Input.GetKeyDown(KeyCode.G))
            {
                colorR = 0.0f;
                colorG = 1.0f;
                colorB = 0.0f;
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                colorR = 0.0f;
                colorG = 0.0f;
                colorB = 1.0f;
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                colorR = 1.0f;
                colorG = 1.0f;
                colorB = 1.0f;
            }
            PV.RPC("colorSet", RpcTarget.AllBuffered, colorR, colorG, colorB);
        }
    }
    [PunRPC]
    void colorSet(float r, float g, float b)
    {
        if (SR)
            SR.color = new Color(r, g, b);
        if (Name)
        {
            Name.GetComponent<Text>().text = PV.Controller.NickName;
            Name.transform.position = Camera.main.WorldToScreenPoint(new Vector3(this.transform.position.x, this.transform.position.y + 1.5f, 0));
        }
    }

    public void setPlayer(CardData HQ)
    {
        if (PV)
            PV.RPC("spriteSet", RpcTarget.AllBuffered, HQ.Sprite);
        this.HQ = HQ;
        if (this.GetComponent<Entity>())
        {
            this.GetComponent<Entity>().setHQ(HQ, true);
        }
        this.transform.GetChild(0).GetComponent<MeshRenderer>().sortingLayerName = "Card";
        this.transform.GetChild(0).GetComponent<MeshRenderer>().sortingOrder = 150;
    }

    [PunRPC]
    void spriteSet(string sprite)
    {
        this.GetComponent<SpriteRenderer>().sprite = Resources.Load(sprite, typeof(Sprite)) as Sprite;
    }

    [PunRPC]
    void layerSet()
    {
        this.GetComponent<SpriteRenderer>().sortingLayerName = "Player";
    }

    #region 대기방

    private void OnMouseDown()
    {
        if (PlayPanel.activeSelf)
            return;
        colorR -= 0.5f;
        if (colorR < 0)
            colorR = 0;
        colorG -= 0.5f;
        if (colorG < 0)
            colorG = 0;
        colorB -= 0.5f;
        if (colorB < 0)
            colorB = 0;
    }

    private void OnMouseUp()
    {
        if (PlayPanel.activeSelf)
            return;
        if (colorR != 0f)
            colorR += 0.5f;
        if (colorG != 0f)
            colorG += 0.5f;
        if (colorB != 0f)
            colorB += 0.5f;
    }

    private void OnMouseDrag()
    {
        if (PlayPanel.activeSelf)
            return;
        if (PV.IsMine)
            this.transform.position = mousePos;
    }

    public void setReady(bool value)
    {
        if (PV)
            PV.RPC("readySet", RpcTarget.All, value);
    }
    public bool getReady()
    {
        return isReady;
    }
    [PunRPC]
    void readySet(bool value)
    {
        isReady = value;
    }
    #endregion

    

    #region 턴 처리
    public void setTurn(bool value)
    {
        if (PV)
            PV.RPC("turnSet", RpcTarget.All, value);
    }
    public bool getTurn()
    {
        return isTurn;
    }
    [PunRPC]
    void turnSet(bool value)
    {
        isTurn = value;
        TurnManager.Inst.setTurn(value, PV.IsMine);
    }
    #endregion

}

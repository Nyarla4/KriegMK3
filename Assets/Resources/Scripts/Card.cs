using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Photon.Pun;

public class Card : MonoBehaviour
{
    [SerializeField] SpriteRenderer card;
    [SerializeField] SpriteRenderer character;
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text colorCostTMP;
    [SerializeField] TMP_Text neutralCostTMP;
    [SerializeField] GameObject Tag;
    [SerializeField] TMP_Text attackTMP;
    [SerializeField] TMP_Text healthTMP;
    [SerializeField] GameObject Effect;
    [SerializeField] Sprite cardFront;
    [SerializeField] Sprite cardBack;

    PhotonView PV;

    public CardData CardData;//카드 정보
    bool isFront;//앞면 여부
    public PRS originPos;//초기 위치
    bool isDead;//폐기상태 여부
    bool isSaved;//저축상태 여부

    #region 초기설정
    public void setup(CardData cardData)
    {
        if (PV.IsMine)
            PV.RPC("Setup", RpcTarget.All,
                    cardData.No, cardData.Name, cardData.Attack, cardData.Health, cardData.Sprite,
                    cardData.Faction, cardData.Color, cardData.CardFront, cardData.NeutralCost, cardData.ColorCost, cardData.Kind,
                    cardData.Keyword, cardData.Effect, cardData.Tag);
    }
    [PunRPC]
    void Setup(
        string no, string name, int attack, int health, string sprite,
        string faction, string color, string cardFront, int neutralCost, int colorCost, string kind,
        string[] keyword, string[] effect, string[] tag)
    {
        this.CardData.No = no;
        this.CardData.Name = name;
        this.CardData.Attack = attack;
        this.CardData.Health = health;
        this.CardData.Sprite = sprite;

        this.CardData.Faction = faction;
        this.CardData.Color = color;
        this.CardData.CardFront = cardFront;
        this.CardData.NeutralCost = neutralCost;
        this.CardData.ColorCost = colorCost;
        this.CardData.Kind = kind;

        this.CardData.Keyword = keyword;
        this.CardData.Effect = effect;
        this.CardData.Tag = tag;

        this.isFront = true;
        this.isSaved = false;
        this.isDead = false;

        if (this.isFront)
        {
            character.sprite = Resources.Load(this.CardData.Sprite, typeof(Sprite)) as Sprite;
            card.sprite = Resources.Load(this.CardData.CardFront, typeof(Sprite)) as Sprite;
            nameTMP.text = this.CardData.Name;
            attackTMP.text = this.CardData.Attack.ToString();
            healthTMP.text = this.CardData.Health.ToString();
            if (this.CardData.ColorCost > 0)
                colorCostTMP.text = this.CardData.ColorCost.ToString();
            else
                colorCostTMP.text = "";
            if (this.CardData.NeutralCost > 0)
                neutralCostTMP.text = this.CardData.NeutralCost.ToString();
            else
                neutralCostTMP.text = "";

            Tag.GetComponent<TMP_Text>().text = "";
            foreach (var item in this.CardData.Tag)
            {
                Tag.GetComponent<TMP_Text>().text += string.Format(" {0}", item);
            }
            Effect.GetComponent<TMP_Text>().text = string.Format("{0}", this.CardData.Effect[0]);
            for (int i = 1; i < this.CardData.Effect.Length; i++)
            {
                Effect.GetComponent<TMP_Text>().text += string.Format("\n{0}", this.CardData.Effect[i]);
            }
        }
        else
        {
            card.sprite = cardBack;
            character.sprite = null;
            nameTMP.text = "";
            attackTMP.text = "";
            healthTMP.text = "";
            colorCostTMP.text = "";
            neutralCostTMP.text = "";
            Tag.GetComponent<TMP_Text>().text = "";
            Effect.GetComponent<TMP_Text>().text = "";
        }
    }
    #endregion

    private void Awake()
    {
        this.PV = this.GetComponent<PhotonView>();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine
            && isFront)
        {
            isFront = false;
            card.sprite = cardBack;
            character.sprite = null;
            nameTMP.text = "";
            attackTMP.text = "";
            healthTMP.text = "";
            colorCostTMP.text = "";
            neutralCostTMP.text = "";
            Tag.GetComponent<TMP_Text>().text = "";
            Effect.GetComponent<TMP_Text>().text = "";
        }
    }

    //부드럽게 이동할지(doTween사용여부), 이동시간
    #region prs이동 함수
    public void moveTransform(PRS prs, bool useDotween, float dotweenTime = 0)
    {
        if (PV)
            PV.RPC("MoveTransform", RpcTarget.All, prs.pos, prs.rot, prs.scale, useDotween, dotweenTime);
    }

    [PunRPC]
    void MoveTransform(Vector3 pos, Quaternion rot, Vector3 scale, bool useDotween, float dotweenTime = 0)
    {
        if (useDotween)
        {
            transform.DOMove(pos, dotweenTime);
            transform.DORotateQuaternion(rot, dotweenTime);
            transform.DOScale(scale, dotweenTime);
        }
        else
        {
            transform.position = pos;
            transform.rotation = rot;
            transform.localScale = scale;
        }
    }
    #endregion


    #region 마우스
    private void OnMouseOver()
    {
        if (PV.IsMine)
            CardManager.Inst.mouseOver(this);
    }
    private void OnMouseExit()
    {
        if (PV.IsMine)
            CardManager.Inst.mouseExit(this);
    }
    private void OnMouseDown()
    {
        if (PV.IsMine)
            CardManager.Inst.mouseDown(this);
    }

    private void OnMouseDrag()
    {
        if (PV.IsMine)
            CardManager.Inst.mouseDrag(this);
    }
    private void OnMouseUp()
    {
        if (PV.IsMine)
        {
            CardManager.Inst.mouseUp(this);
        }
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Photon.Pun;

public class Entity : MonoBehaviour
{
    [SerializeField] SpriteRenderer entity;
    [SerializeField] SpriteRenderer character;
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text colorCostTMP;
    [SerializeField] TMP_Text neutralCostTMP;
    [SerializeField] GameObject Tag;
    [SerializeField] TMP_Text attackTMP;
    [SerializeField] TMP_Text healthTMP;
    [SerializeField] GameObject Effect;
    [SerializeField] GameObject sleepParticle;

    PhotonView PV;//�����

    CardData CardData;//������

    [SerializeField] bool isHQOrEmpty;//�� ��ƼƼ Ȥ�� HQ Ȯ��

    Vector3 originPos;//���Ŀ�

    int turns = 0;//��� �� ��

    bool isLock = false;//�� ����

    bool isSave = false;//���� ĭ ����
    bool isDead = false;//��� ĭ ����

    bool attackable = false;//���ݰ��� ����

    bool fear = false;//�������� ����

    bool maintain = false;//������ ����
    int maintainColor = 0;//���� ������
    int maintainNeutral = 0;//���� ������

    int attack;
    int health;
    #region �ʱ⼳��
    public void setup(CardData cardData)
    {
        if (PV.IsMine)
            PV.RPC("Setup", RpcTarget.All,
                    cardData.No, cardData.Name, cardData.Attack, cardData.Health, cardData.Sprite,
                    cardData.Faction, cardData.Color, cardData.CardFront, cardData.NeutralCost, cardData.ColorCost, cardData.Kind,
                    cardData.Keyword, cardData.Effect, cardData.Tag);
        attack = cardData.Attack;
        health = cardData.Health;
    }
    [PunRPC]
    void Setup(
        string no, string name, int attack, int health, string sprite,
        string faction, string color, string cardFront, int neutralCost, int colorCost, string kind,
        string[] keyword, string[] effect, string[] tag)
    {
        this.attack = attack;
        this.health = health;

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

        character.sprite = Resources.Load(this.CardData.Sprite, typeof(Sprite)) as Sprite;
        entity.sprite = Resources.Load(this.CardData.CardFront, typeof(Sprite)) as Sprite;
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

        string isMaintain = "";
        foreach (var item in this.CardData.Keyword)
        {
            if (item.Contains("������"))
            {
                maintain = true;
                isMaintain = item;
            }   
        }

        if(maintain)
        {
            string spr = isMaintain.Replace("������ ", "");
            switch (spr)
            {
                case "R":
                case "B":
                case "W":
                case "G":
                case "V":
                    maintainColor = 1;
                    break;
            }
        }
    }
    public void setHQ(CardData cardData, bool value)
    {
        isHQOrEmpty = value;
        if (PV)
            PV.RPC("SetHQ", RpcTarget.All,
                        cardData.No, cardData.Name, cardData.Attack, cardData.Health, cardData.Sprite,
                        cardData.Faction, cardData.Color, cardData.CardFront, cardData.NeutralCost, cardData.ColorCost, cardData.Kind,
                        cardData.Keyword, cardData.Effect, cardData.Tag);
    }
    [PunRPC]
    void SetHQ(string no, string name, int attack, int health, string sprite,
        string faction, string color, string cardFront, int neutralCost, int colorCost, string kind,
        string[] keyword, string[] effect, string[] tag)
    {
        this.attack = attack;
        this.health = health;

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
    }


    public void setHQorEmpty(bool value)
    {
        if (PV)
            PV.RPC("HQorEmptySet", RpcTarget.All, value);
    }
    [PunRPC]
    void HQorEmptySet(bool value)
    {
        isHQOrEmpty = value;
    }
    public bool getHQorEmpty()
    {
        return isHQOrEmpty;
    }


    public Vector3 getOriginPos()
    {
        return originPos;
    }
    public void setOriginPos(Vector3 value)
    {
        originPos = value;
    }

    public void setSaveorDead(bool isSave, bool value)
    {
        if (PV.IsMine)
        {
            if (isSave)
                PV.RPC("saveSet", RpcTarget.All, value);
            else
                PV.RPC("deadSet", RpcTarget.All, value);
        }
    }
    [PunRPC]
    void saveSet(bool value)
    {
        this.isSave = value;
    }
    [PunRPC]
    void deadSet(bool value)
    {
        this.isDead = value;
    }
    #endregion

    #region �� �� Ȯ��
    public void setTurn()
    {
        PV.RPC("turnSet", RpcTarget.All);
    }
    [PunRPC]
    void turnSet()
    {
        this.turns++;
    }
    #endregion

    //�ε巴�� �̵�����(doTween��뿩��), �̵��ð�
    #region prs�̵� �Լ�
    public void moveTransform(Vector3 pos, Quaternion rot, bool useDotween, float dotweenTime = 0)
    {
        if (PV)
            PV.RPC("MoveTransform", RpcTarget.All, pos, rot, useDotween, dotweenTime);
    }

    [PunRPC]
    void MoveTransform(Vector3 pos, Quaternion rot, bool useDotween, float dotweenTime = 0)
    {
        if (useDotween)
        {
            transform.DOMove(pos, dotweenTime);
            transform.DORotateQuaternion(rot, dotweenTime);
        }
        else
        {
            transform.position = pos;
            transform.rotation = rot;
        }
    }
    #endregion

    private void Awake()
    {
        this.PV = this.GetComponent<PhotonView>();
        CardData = new CardData();
    }
    void Start()
    {
        if (this.gameObject.name == "EmptyEntity")
        {
            this.setHQorEmpty(true);
        }
    }

    void Update()
    {
        if (this.tag == "Player" && !this.isHQOrEmpty)
        {
            this.setHQorEmpty(true);
        }
        if (!this.isHQOrEmpty && this.health <= 0)
        {
            EntityManager.Inst.deadEntities(0, this);
        }
        if (isSave || isDead)
        {
            sleepParticle.SetActive(false);
            return;
        }
        if (PV)
            PV.RPC("setHealth", RpcTarget.All, health);
        if (!isHQOrEmpty)
        {
            if (turns != 0)
            {
                sleepParticle.SetActive(false); 
            }
            if (isLock)
            {
                moveTransform(originPos, this.transform.rotation * Quaternion.Euler(new Vector3(0, 0, 90)), true, 0.1f);
            }
        }
    }
    [PunRPC]
    void setHealth(int health)
    {
        CardData.Health = health;
        if (healthTMP != null)
        {
            healthTMP.text = CardData.Health.ToString();
            if (healthTMP.text == "0")
                healthTMP.text = "";
        }
    }

    public int getAttack()
    {
        return attack;
    }
    public string getColor()
    {
        return CardData.Color;
    }

    #region ��, ���ݿ��� �� ���� ����
    public void setLock(bool value)
    {
        isLock = value;
    }
    public bool getLock()
    {
        return isLock;
    }
    public void setAttackable(bool value)
    {
        attackable = value;
    }
    public bool getAttackable()
    {
        return attackable;
    }
    public void setFear(bool value)
    {
        fear = value;
    }
    public bool getFear()
    {
        return fear;
    }
    public bool getMaintain()
    {
        return maintain;
    }
    #endregion
    //������� ����
    public bool Damaged(int damage)
    {
        if (PV)
            PV.RPC("getDamage", RpcTarget.All, damage);
        return isDead;
    }
    [PunRPC]
    void getDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            isDead = true;
        }
    }
    #region ���콺����
    private void OnMouseDown()
    {
        if (PV.IsMine)
            EntityManager.Inst.entityMouseDown(this);
    }
    private void OnMouseDrag()
    {
        if (PV.IsMine)
            EntityManager.Inst.entityMouseDrag();
    }
    private void OnMouseUp()
    {
        if (PV.IsMine)
            EntityManager.Inst.entityMouseUp(this);
    }
    #endregion
    public bool isMine()
    {
        return PV.IsMine;
    }
    public string getName()
    {
        return CardData.Name;
    }
}

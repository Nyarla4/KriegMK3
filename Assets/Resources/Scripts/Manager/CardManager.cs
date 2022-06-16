using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;//������ ��ġ�Ƿ� ����� ������ ����� ������ ����
using Photon.Pun;
using DG.Tweening;
using UnityEngine.UI;

public class CardManager : MonoBehaviour//, IPunObservable
{
    public static CardManager Inst { get; private set; }
    void Awake() => Inst = this;//�̱��� ����(�Ŵ����� �ϳ��� ����)

    [SerializeField] GameObject cardPrefab;//ī���� ������
    List<CardData> deckBuffer;
    [SerializeField] List<Card> myCards;//���� ��
    CardData HQ;//HQ������

    GameObject player = null;
    GameObject enemy = null;

    Card selected;
    bool isDrag = false;

    Vector3 cardLeft = Vector3.zero;
    Vector3 cardRight = Vector3.zero;

    bool onCardArea = false;//�� ����
    bool onSaveArea = false;//���� ����

    [SerializeField] GameObject playPanel;
    void Start()
    {
        
    }

    void Update()
    {
        if (playPanel.activeSelf)
        {
            onCardArea = chkCardArea();
            onSaveArea = chkSaveArea();
        }
    }

    //���� �� ������ ��
    public void GameStart(bool isFirst)
    {
        SetPlayer();
        SetupDeckBuffer();
        HQ = deckBuffer.Find(item => item.Kind == "HQ");
        deckBuffer.Remove(HQ);
        player.GetComponent<roomPlayer>().setPlayer(HQ);
        if (isFirst)
        {
            for (int i = 0; i < 4; i++)
            {
                AddCard(isFirst);
            }
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                AddCard(!isFirst);
            }
        }

        EntityManager.Inst.setBossEntity();
        
        if (isFirst)
            TurnManager.Inst.setPhase(PHASE.MAIN);
        else
            TurnManager.Inst.setPhase(PHASE.WAITING);
    }

    //deckSO�� ������ deckBuffer�� �ְ� ���´�
    public void SetupDeckBuffer()
    {
        deckBuffer = new List<CardData>(100);
        foreach (var item in DeckManager.Inst.deckSO.cards)
            deckBuffer.Add(item);
        for (int i = 0; i < deckBuffer.Count; i++)
        {
            int rand = Random.Range(i, deckBuffer.Count);
            CardData temp = deckBuffer[i];
            deckBuffer[i] = deckBuffer[rand];
            deckBuffer[rand] = temp;
        }
    }
    
    //deckBuffer�� ������ ������
    public CardData PopCard()
    {
        CardData temp = null;
        if (deckBuffer.Count > 0)
        {
            temp = deckBuffer[0];
            deckBuffer.RemoveAt(0);
        }
        return temp;
    }

    //�п� ī�带 ���Ѵ�
    public void AddCard(bool isMine)
    {
        //var cardObj = isMine ?
        //    Instantiate(cardPrefab, player.transform.position + (player.transform.right * 2.0f), player.transform.rotation) :
        //    Instantiate(cardPrefab, enemy.transform.position + (enemy.transform.right * 2.0f), enemy.transform.rotation);
        var cardObj =
            PhotonNetwork.Instantiate("Prefabs/Card", player.transform.position + (player.transform.right * 12.0f), player.transform.rotation);
        var card = cardObj.GetComponent<Card>();
        //card.PVset();
        card.setup(PopCard());
        myCards.Add(card);
        SetOriginOrder(isMine);
        CardAlignment();
    }

    //���� ī���� ȭ�� �켱���� ���Ѵ�
    void SetOriginOrder(bool isMine)
    {
        for (int i = 0; i < myCards.Count;i++)// (isMine ? myCards.Count : otherCards.Count); i++)
        {
            var target = myCards[i];//isMine ? myCards[i] : otherCards[i];
            target?.GetComponent<Order>().SetOriginOrder(i);
            //target?.GetComponent<Order>().SetOrder(i);
        }
    }

    //���� ī�带 �����Ѵ�
    void CardAlignment()
    {
        float height = (player.transform.rotation == Utils.QI) ? 0.5f : -0.5f;
        List <PRS> originPRS = new List<PRS>();
        originPRS = RoundAlignment(
            cardLeft, cardRight,
            myCards.Count, height, Vector3.one * 0.5f,
            player.transform.rotation * Quaternion.Euler(0, 0, 15),
            player.transform.rotation * Quaternion.Euler(0, 0, -15)
            );
        var targets = myCards;
        for (int i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            target.originPos = originPRS[i];
            target.moveTransform(target.originPos, true, 1.0f);
        }
    }

    //���������Ͽ� ����
    List<PRS> RoundAlignment(Vector3 left, Vector3 right, int objCount, float height, Vector3 scale, Quaternion leftR, Quaternion rightR)
    {
        //���� �ش� ������Ʈ�� ��ġ�� 0�� 1���̿��� ���Ѵ�.
        float[] objLerps = new float[objCount];
        List<PRS> result = new List<PRS>();

        switch (objCount)
        {
            //1 ~ 3 : �ϵ��ڵ� ����(�߰� ���� �ʿ�)
            case 1: objLerps = new float[] { 0.5f }; break;
            case 2: objLerps = new float[] { 0.27f, 0.79f }; break;
            case 3: objLerps = new float[] { 0.1f, 0.5f, 0.9f }; break;
            default:
                //�ɰ��� ��ġ�� ����Ͽ� �ִ´�
                float interval = 1f / (objCount - 1);
                for (int i = 0; i < objCount; i++)
                    objLerps[i] = interval * i;
                break;
        }

        for (int i = 0; i < objCount; i++)
        {
            //���ʰ� ������ ���̿��� ��ġ�� �����Ͽ� ���� �� ȸ�� �ʱ�ȭ
            var targetPos = Vector3.Lerp(left, right, objLerps[i]);
            var targetRot = leftR * Quaternion.Euler(0, 0, -15);
            //�������� ȸ��
            //0.5,0�� �������� �ϴ� height�� �������� ��
            //0.5�� �߽��� ���� : objLerps�� ���� 0~1�̹Ƿ�
            if (objCount >= 4)//4�� �̻󿡼�
            {
                //x�� ��ġ�� objLerps�̹Ƿ� y���� ����ؾ���
                //y���� ��(����������-(x-����x)����)
                float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));

                curve = height >= 0 ? curve : -curve;//height�� ���� ��� Ȥ�� ����
                targetPos.y += curve;//���� y�� ����

                //�������� �����Ѵ�
                targetRot = Quaternion.Slerp(leftR, rightR, objLerps[i]);
            }
            result.Add(new PRS(targetPos, targetRot, scale));
        }
        return result;
    }

    //player,enemy����
    void SetPlayer()
    {
        GameObject[] temp = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in temp)
        {
            if (item.GetComponent<PhotonView>().IsMine)
                player = item;
            else
                enemy = item;
        }
        enemy.transform.position = (player.transform.rotation == Utils.QI) ? new Vector3(0, 4, 0) : new Vector3(0, -4, 0);
        enemy.transform.rotation = player.transform.rotation * Quaternion.Euler(0, 0, 180);

        cardLeft =
        player.transform.position + (player.transform.right * -2.0f) + (player.transform.up * -1.5f);
        cardRight =
        player.transform.position + (player.transform.right * 2.0f) + (player.transform.up * -1.5f);
    }

    public GameObject getPlayer()
    {
        return player;
    }

    //�� ����(��� ������)
    public List<Card> GetCards()
    {
        return this.myCards;
    }

    #region ���콺 ����
    public void mouseOver(Card card)
    {
        selected = card;
        switch (TurnManager.Inst.getPhase())
        {
            case PHASE.MAIN:
                TurnManager.Inst.setSelectedCost(selected);//�ڽ�Ʈ �ϸŴ����� �ѱ�� �Լ�
                break;
            case PHASE.PAYING:
                break;
            case PHASE.TARGETING:
                break;
            case PHASE.WAITING:
                break;
            case PHASE.SAVING:
                break;
            case PHASE.NUM:
                break;
            default:
                break;
        }
        if (!isDrag)
            EnlargeCard(true, card);
    }

    public void mouseExit(Card card)
    {
        selected = null;
        EnlargeCard(false, card);
    }

    public void mouseDrag(Card card)
    {
        Quaternion upDown = (player.transform.rotation.eulerAngles.z == 0) ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 0, 180);
        float downUp = player.transform.rotation.eulerAngles.z == 0 ? 1f : -1f;
        if (player.GetComponent<roomPlayer>().getTurn())
        {
            if (!onCardArea)
            {    //EnlargeCard(true, card);
                card.moveTransform(new PRS(Utils.MousePos * downUp, upDown, card.originPos.scale), false);
                EntityManager.Inst.InsertEmptyEntity(Utils.MousePos.x);
            }
        }
    }

    public void mouseDown(Card card)
    {
        isDrag = true;
    }

    public void mouseUp(Card card)
    {
        isDrag = false;
        if (player.GetComponent<roomPlayer>().getTurn())
        {
            if (onCardArea)//�� �������� �����д� : �ȳ���
                card.moveTransform(card.originPos, false);
            else
            {
                switch (TurnManager.Inst.getPhase())
                {
                    case PHASE.MAIN:
                        if (onSaveArea)//���࿵���̶�� ������
                            TurnManager.Inst.setPhase(PHASE.SAVING);
                        else//�ƴ϶�� �����
                        {
                            //print(canSpawn(card));
                            if (canSpawn(card))//�ڽ�Ʈ Ȯ�� �Լ�
                            {
                                TurnManager.Inst.setPhase(PHASE.PAYING);//������ �ѱ�� �Լ�
                                spawn(card);//���� �Լ�
                            }
                        }
                        break;
                    case PHASE.SAVING:
                        break;
                    case PHASE.PAYING:
                        myCards.Remove(card);
                        card.transform.DOKill();
                        PhotonNetwork.Destroy(card.gameObject);
                        selected = null;
                        CardAlignment();
                        if (TurnManager.Inst.getCost(true) > 0)//�� �ڽ�Ʈ�� �����ִٸ�
                        {
                            if (TurnManager.Inst.getSelected() == card.CardData.Color)//���� ���̶�� �� �ڽ�Ʈ ����
                            {
                                TurnManager.Inst.useCost(true);
                            }
                            else//�ٸ� ���̶�� ���� �ڽ�Ʈ ����
                            {
                                TurnManager.Inst.useCost(false);
                            }
                        }
                        else//������ �����ִٸ�
                            TurnManager.Inst.useCost(false);//���� �ڽ�Ʈ ����
                        break;
                    case PHASE.TARGETING:
                        break;
                }
            }
            EntityManager.Inst.RemoveMyEmptyEntity();
        }
    }
    #endregion

    //ī�� Ȯ��
    void EnlargeCard(bool isEnlarge, Card card)
    {
        if (isEnlarge)
        {
            float upDown = (player.transform.rotation.eulerAngles.z == 0) ? 1f : -1f;
            Vector3 enlargePos = new Vector3(card.originPos.pos.x, card.originPos.pos.y + (2f * upDown), 0f);//���� ��ġ���� ��¦ ����
            card.moveTransform(new PRS(enlargePos, player.transform.rotation, Vector3.one * 0.75f), false);
        }
        else
        {
            card.moveTransform(card.originPos, false);
        }
        card.GetComponent<Order>().SetMostFrontOrder(isEnlarge);
    }

    //�� ���� Ȯ��
    bool chkCardArea()
    {
        float downUp = player.transform.rotation.eulerAngles.z == 0 ? 1f : -1f;
        Vector3 mouse = Utils.MousePos * downUp;
        float leftLimit = (downUp * cardLeft.x) - ((Mathf.Sqrt(16.4f) * 3179f) / 6970f);
        float rightLimit = (downUp * cardRight.x) + ((Mathf.Sqrt(16.4f) * 3179f) / 6970f);
        float upLimit = cardLeft.y + 1.7f;
        float downLimit = cardLeft.y - 1.7f;
        if (mouse.x <= rightLimit && mouse.x >= leftLimit
            && mouse.y <= upLimit && mouse.y >= downLimit)
        {
            return true;
        }
        return false;
    }
    //���� ���� Ȯ��
    bool chkSaveArea()
    {
        return false;
    }

    void spawn(Card card)
    {
        switch (card.CardData.Kind)
        {
            case "Unit"://���� ����
                float downUp = player.transform.rotation.eulerAngles.z == 0 ? 1f : -1f;
                if (EntityManager.Inst.SpawnEntity(card.CardData, Utils.MousePos * downUp))
                {
                    myCards.Remove(card);
                    card.transform.DOKill();
                    PhotonNetwork.Destroy(card.gameObject);
                    selected = null;
                    CardAlignment();
                }
                break;
            case "Strategy"://���� ����

                break;
            default:
                break;
        }
    }

    bool canSpawn(Card card)
    {
        //�ڽ�Ʈ ������ �Ұ����� ��Ȳ�϶� false�� ��ȯ�Ѵ�.
        int color = card.CardData.ColorCost;
        int neutral = card.CardData.NeutralCost;
        int ColorHave = 0;
        int NeutralHave = 0;
        foreach (var item in myCards)
        {
            if (item.CardData.Color == card.CardData.Color)
                ColorHave++;
            else
                NeutralHave++;
        }
        foreach (var item in EntityManager.Inst.getSaves())
        {
            if (item.getColor() == card.CardData.Color)
                ColorHave++;
            else
                NeutralHave++;
        }
        if (color + neutral > ColorHave + NeutralHave)
            return false;
        else
        {
            if (color > ColorHave)
                return false;
            else
                return true;
        }
    }
}

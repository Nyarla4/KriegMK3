using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;//랜덤이 겹치므로 어느쪽 랜덤을 사용할 것인지 정의
using Photon.Pun;
using DG.Tweening;
using UnityEngine.UI;

public class CardManager : MonoBehaviour//, IPunObservable
{
    public static CardManager Inst { get; private set; }
    void Awake() => Inst = this;//싱글톤 형식(매니저는 하나만 존재)

    [SerializeField] GameObject cardPrefab;//카드의 프리팹
    List<CardData> deckBuffer;
    [SerializeField] List<Card> myCards;//본인 패
    CardData HQ;//HQ데이터

    GameObject player = null;
    GameObject enemy = null;

    Card selected;
    bool isDrag = false;

    Vector3 cardLeft = Vector3.zero;
    Vector3 cardRight = Vector3.zero;

    bool onCardArea = false;//패 영역
    bool onSaveArea = false;//저축 영역

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

    //시작 시 실행할 것
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

    //deckSO의 내용을 deckBuffer에 넣고 섞는다
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
    
    //deckBuffer의 내용을 꺼낸다
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

    //패에 카드를 더한다
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

    //패의 카드의 화면 우선도를 정한다
    void SetOriginOrder(bool isMine)
    {
        for (int i = 0; i < myCards.Count;i++)// (isMine ? myCards.Count : otherCards.Count); i++)
        {
            var target = myCards[i];//isMine ? myCards[i] : otherCards[i];
            target?.GetComponent<Order>().SetOriginOrder(i);
            //target?.GetComponent<Order>().SetOrder(i);
        }
    }

    //패의 카드를 정렬한다
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

    //구형보간하여 정렬
    List<PRS> RoundAlignment(Vector3 left, Vector3 right, int objCount, float height, Vector3 scale, Quaternion leftR, Quaternion rightR)
    {
        //현재 해당 오브젝트의 위치를 0과 1사이에서 정한다.
        float[] objLerps = new float[objCount];
        List<PRS> result = new List<PRS>();

        switch (objCount)
        {
            //1 ~ 3 : 하드코딩 조정(추가 조정 필요)
            case 1: objLerps = new float[] { 0.5f }; break;
            case 2: objLerps = new float[] { 0.27f, 0.79f }; break;
            case 3: objLerps = new float[] { 0.1f, 0.5f, 0.9f }; break;
            default:
                //쪼개서 위치를 계산하여 넣는다
                float interval = 1f / (objCount - 1);
                for (int i = 0; i < objCount; i++)
                    objLerps[i] = interval * i;
                break;
        }

        for (int i = 0; i < objCount; i++)
        {
            //왼쪽과 오른쪽 사이에서 위치를 보간하여 조정 및 회전 초기화
            var targetPos = Vector3.Lerp(left, right, objLerps[i]);
            var targetRot = leftR * Quaternion.Euler(0, 0, -15);
            //원형으로 회전
            //0.5,0을 기준으로 하는 height가 반지름인 원
            //0.5가 중심인 이유 : objLerps의 값이 0~1이므로
            if (objCount >= 4)//4장 이상에서
            {
                //x의 위치는 objLerps이므로 y값을 계산해야함
                //y값은 √(반지름제곱-(x-원점x)제곱)
                float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));

                curve = height >= 0 ? curve : -curve;//height에 따라 양수 혹은 음수
                targetPos.y += curve;//구한 y값 적용

                //구형으로 보간한다
                targetRot = Quaternion.Slerp(leftR, rightR, objLerps[i]);
            }
            result.Add(new PRS(targetPos, targetRot, scale));
        }
        return result;
    }

    //player,enemy지정
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

    //패 정보(장수 때문에)
    public List<Card> GetCards()
    {
        return this.myCards;
    }

    #region 마우스 조작
    public void mouseOver(Card card)
    {
        selected = card;
        switch (TurnManager.Inst.getPhase())
        {
            case PHASE.MAIN:
                TurnManager.Inst.setSelectedCost(selected);//코스트 턴매니저에 넘기는 함수
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
            if (onCardArea)//패 영역에서 내려둔다 : 안낸다
                card.moveTransform(card.originPos, false);
            else
            {
                switch (TurnManager.Inst.getPhase())
                {
                    case PHASE.MAIN:
                        if (onSaveArea)//저축영역이라면 저축을
                            TurnManager.Inst.setPhase(PHASE.SAVING);
                        else//아니라면 사용을
                        {
                            //print(canSpawn(card));
                            if (canSpawn(card))//코스트 확인 함수
                            {
                                TurnManager.Inst.setPhase(PHASE.PAYING);//페이즈 넘기는 함수
                                spawn(card);//실행 함수
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
                        if (TurnManager.Inst.getCost(true) > 0)//색 코스트가 남아있다면
                        {
                            if (TurnManager.Inst.getSelected() == card.CardData.Color)//같은 색이라면 색 코스트 감소
                            {
                                TurnManager.Inst.useCost(true);
                            }
                            else//다른 색이라면 무색 코스트 감소
                            {
                                TurnManager.Inst.useCost(false);
                            }
                        }
                        else//무색만 남아있다면
                            TurnManager.Inst.useCost(false);//무색 코스트 감소
                        break;
                    case PHASE.TARGETING:
                        break;
                }
            }
            EntityManager.Inst.RemoveMyEmptyEntity();
        }
    }
    #endregion

    //카드 확대
    void EnlargeCard(bool isEnlarge, Card card)
    {
        if (isEnlarge)
        {
            float upDown = (player.transform.rotation.eulerAngles.z == 0) ? 1f : -1f;
            Vector3 enlargePos = new Vector3(card.originPos.pos.x, card.originPos.pos.y + (2f * upDown), 0f);//기존 위치보다 살짝 위로
            card.moveTransform(new PRS(enlargePos, player.transform.rotation, Vector3.one * 0.75f), false);
        }
        else
        {
            card.moveTransform(card.originPos, false);
        }
        card.GetComponent<Order>().SetMostFrontOrder(isEnlarge);
    }

    //패 영역 확인
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
    //저축 영역 확인
    bool chkSaveArea()
    {
        return false;
    }

    void spawn(Card card)
    {
        switch (card.CardData.Kind)
        {
            case "Unit"://유닛 스폰
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
            case "Strategy"://전략 실행

                break;
            default:
                break;
        }
    }

    bool canSpawn(Card card)
    {
        //코스트 지불이 불가능한 상황일때 false를 반환한다.
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

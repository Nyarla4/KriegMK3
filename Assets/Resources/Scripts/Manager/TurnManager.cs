using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public enum PHASE
{ // 페이즈 열거체
    NONE = -1, // 상태 정보 없음.
    MAIN = 0, // 통상 상태.(카드를 낸 위치에 따라 PAYING으로)
    DRAW, // 드로우.(턴 바뀐 직후-드로우 후 언락으로)
    UNLOCK, // 언락.(락된 카드들 언락, 이후 메인으로)
    PAYING, // 코스트 지불 중.(지불 후 메인 혹은 타겟팅으로)
    TARGETING, // 타겟 지정 중.(지정 후 효과 처리 후 메인으로)
    WAITING, //상대 턴
    NUM // 상태가 몇 종류 있는지 나타낸다
};

public class TurnManager : MonoBehaviour
{
    public static TurnManager Inst { get; private set; }
    //싱글톤 형식(매니저는 하나만 존재)
    void Awake()
    {
        Inst = this;
        myTurn = false;
    }

    [SerializeField] GameObject endButton;
    TextMeshProUGUI endButtonText;
    [SerializeField] GameObject playPanel;
    bool myTurn;

    public PHASE phase = PHASE.NONE;
    public PHASE next_phase = PHASE.NONE;

    int turn = 0;

    string selectColor = "";
    int selectedColor = 0;
    int selectedNeutral = 0;
    void Start()
    {
        endButtonText = endButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playPanel.activeSelf)
        {
            //페이즈 변경
            if (next_phase == PHASE.NONE)
            {
                switch (this.phase)
                {
                    case PHASE.MAIN:
                    case PHASE.WAITING:
                        //setPhase로 이동함
                        break;
                    case PHASE.DRAW:
                    case PHASE.UNLOCK:
                        //페이즈 변경 시 행동 후 페이즈 이동함
                        break;
                    case PHASE.PAYING:
                        if (selectedColor == 0
                        && selectedNeutral == 0)
                            this.next_phase = PHASE.MAIN;
                        break;
                    case PHASE.TARGETING:
                        break;
                }
            }
            //페이즈가 바뀌었을 때
            while (this.next_phase != PHASE.NONE)
            {
                turn++;
                this.phase = this.next_phase;
                this.next_phase = PHASE.NONE;
                switch (this.phase)
                {
                    case PHASE.MAIN:
                        endButtonText.text = "내 턴";
                        break;
                    case PHASE.DRAW://내 턴이 되었다
                        if (turn <= 2)
                        { }
                        else
                        {
                            int count = CardManager.Inst.GetCards().Count;
                            if (count < 1)
                                CardManager.Inst.AddCard(true);
                            if (count < 2)
                                CardManager.Inst.AddCard(true);
                            if (count < 3)
                                CardManager.Inst.AddCard(true);
                            CardManager.Inst.AddCard(true);
                        }
                        endButtonText.text = "내 턴";
                        this.next_phase = PHASE.UNLOCK;
                        break;
                    case PHASE.UNLOCK:
                        EntityManager.Inst.turnInit();
                        this.next_phase = PHASE.MAIN;
                        break;
                    case PHASE.PAYING:
                        endButtonText.text = "패 지불 완료";
                        break;
                    case PHASE.TARGETING:
                        break;
                    case PHASE.WAITING://상대턴이 되었다
                        endButtonText.text = "남 턴";
                        break;
                }
            }
            //각 페이즈 항시 실행
            switch (this.phase)
            {
                case PHASE.MAIN:
                    break;
                case PHASE.PAYING:

                    break;
                case PHASE.TARGETING:
                    break;
                case PHASE.WAITING:
                    break;
                default:
                    break;
            }
        }
    }

    public void setTurn(bool value, bool isMine)//, int turnCount)
    {
        if (isMine)
        {
            if (myTurn != value)//턴이 바뀌었을 때
            {
                if (value)
                {
                    this.next_phase = PHASE.DRAW;
                    print("내 턴이 되었다.************");
                }
                else
                {
                    this.next_phase = PHASE.WAITING;
                    print("************상대 턴이 되었다.");
                }
                if (myTurn)
                    print("내 턴이었다.************");
                else
                    print("************상대 턴이었다.");
            }
        }
        myTurn = value;
    }

    public PHASE getPhase()
    {
        return this.phase;
    }

    public void setPhase(PHASE next)
    {
        this.next_phase = next;
    }

    public void setSelectedCost(Card card)
    {
        selectedColor = card.CardData.ColorCost;
        selectedNeutral = card.CardData.NeutralCost;
        selectColor = card.CardData.Color;
    }

    public void useCost(bool color)
    {
        if (color)
            selectedColor--;
        else
            selectedNeutral--;
    }
    public int getCost(bool color)
    {
        if (color)
            return selectedColor;
        else
            return selectedNeutral;
    }
    public string getSelected()
    {
        return selectColor;
    }
}

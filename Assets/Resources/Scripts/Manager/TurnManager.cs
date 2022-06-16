using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public enum PHASE
{ // ������ ����ü
    NONE = -1, // ���� ���� ����.
    MAIN = 0, // ��� ����.(ī�带 �� ��ġ�� ���� PAYING����)
    DRAW, // ��ο�.(�� �ٲ� ����-��ο� �� �������)
    UNLOCK, // ���.(���� ī��� ���, ���� ��������)
    PAYING, // �ڽ�Ʈ ���� ��.(���� �� ���� Ȥ�� Ÿ��������)
    TARGETING, // Ÿ�� ���� ��.(���� �� ȿ�� ó�� �� ��������)
    WAITING, //��� ��
    NUM // ���°� �� ���� �ִ��� ��Ÿ����
};

public class TurnManager : MonoBehaviour
{
    public static TurnManager Inst { get; private set; }
    //�̱��� ����(�Ŵ����� �ϳ��� ����)
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
            //������ ����
            if (next_phase == PHASE.NONE)
            {
                switch (this.phase)
                {
                    case PHASE.MAIN:
                    case PHASE.WAITING:
                        //setPhase�� �̵���
                        break;
                    case PHASE.DRAW:
                    case PHASE.UNLOCK:
                        //������ ���� �� �ൿ �� ������ �̵���
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
            //����� �ٲ���� ��
            while (this.next_phase != PHASE.NONE)
            {
                turn++;
                this.phase = this.next_phase;
                this.next_phase = PHASE.NONE;
                switch (this.phase)
                {
                    case PHASE.MAIN:
                        endButtonText.text = "�� ��";
                        break;
                    case PHASE.DRAW://�� ���� �Ǿ���
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
                        endButtonText.text = "�� ��";
                        this.next_phase = PHASE.UNLOCK;
                        break;
                    case PHASE.UNLOCK:
                        EntityManager.Inst.turnInit();
                        this.next_phase = PHASE.MAIN;
                        break;
                    case PHASE.PAYING:
                        endButtonText.text = "�� ���� �Ϸ�";
                        break;
                    case PHASE.TARGETING:
                        break;
                    case PHASE.WAITING://������� �Ǿ���
                        endButtonText.text = "�� ��";
                        break;
                }
            }
            //�� ������ �׽� ����
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
            if (myTurn != value)//���� �ٲ���� ��
            {
                if (value)
                {
                    this.next_phase = PHASE.DRAW;
                    print("�� ���� �Ǿ���.************");
                }
                else
                {
                    this.next_phase = PHASE.WAITING;
                    print("************��� ���� �Ǿ���.");
                }
                if (myTurn)
                    print("�� ���̾���.************");
                else
                    print("************��� ���̾���.");
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

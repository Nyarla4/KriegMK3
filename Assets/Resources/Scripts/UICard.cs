using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UICard : MonoBehaviour
{
    [SerializeField] Image card;
    [SerializeField] Image character;
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text colorCostTMP;
    [SerializeField] TMP_Text neutralCostTMP;
    [SerializeField] GameObject Tag;
    [SerializeField] TMP_Text attackTMP;
    [SerializeField] TMP_Text healthTMP;
    [SerializeField] GameObject Effect;

    public CardData CardData;//ī�� ����

    GameObject seed;

    public void Setup(CardData cardData)
    {
        this.CardData = cardData;
        if (CardData.Faction == "Visitor")
        {
            nameTMP.faceColor =
            attackTMP.faceColor =
            healthTMP.faceColor =
            colorCostTMP.faceColor =
            neutralCostTMP.faceColor =
            Tag.GetComponent<TextMeshProUGUI>().faceColor =
            Effect.GetComponent<TextMeshProUGUI>().faceColor =
            Color.black;
        }
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
    void Start()
    {
        seed = this.transform.parent.transform.parent.transform.parent.transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void toDeck()
    {
        GameObject temp;
        switch (seed.name)
        {
            case "CardList"://���� �߰�
                CardData data = this.GetComponent<UICard>().CardData;
                int errCode = deckErrCheck(data);

                if (errCode == -1)
                {
                    DeckManager.Inst.deck.Add(data);
                    Transform InstPlace = seed.transform.parent.GetChild(1).transform.GetChild(0).transform.GetChild(0);
                    if (DeckManager.Inst.deck.Count % 3 == 1)
                        Instantiate(Resources.Load("Prefabs/group") as GameObject, InstPlace);
                    temp = Instantiate(Resources.Load("Prefabs/UICard") as GameObject, InstPlace.transform.GetChild(InstPlace.childCount - 1));
                    temp.GetComponent<UICard>().Setup(this.CardData);
                    float posX = 0;
                    switch (DeckManager.Inst.deck.Count % 3)
                    {
                        case 1: posX = -110.2f; break;
                        case 2: posX = 0f; break;
                        case 0: posX = 110.2f; break;
                    }
                    temp.transform.localPosition = new Vector3(posX, 0, 0);
                }
                else
                {
                    string msg = "";
                    switch (errCode)
                    {
                        case 0:
                            msg = "��ū��\n���� �߰��� ��\n�����ϴ�."; break;
                        case 1:
                            msg = "����\n2����������\n�����մϴ�."; break;
                        case 2:
                            msg = "HQ��\n�� �常 ���� ��\n�ֽ��ϴ�."; break;
                        case 3:
                            msg = "������\nHQ�� ���� ����\n�����մϴ�"; break;
                        case 4:
                            msg = "������ �ֱ� ����\nHQ�� �־�� �մϴ�."; break;
                        case 5:
                            msg = "��� ī���\n�ִ� 4���Դϴ�."; break;
                        case 6:
                            msg = "���� ī���\n�ִ� 1���Դϴ�."; break;
                        case 7:
                            msg = "�غ� ī���\n�ִ� 2���Դϴ�."; break;
                        default:
                            break;
                    }
                    NetworkManager.Inst.cantAddWindow(msg);
                }
                break;
            case "DeckList"://������ ����
                float calc = (this.transform.parent.localPosition.y - 67.9f) / (-145.8f);
                int i = Mathf.RoundToInt(calc);
                i--;//i�� �ش� ī�尡 �ִ� ���� �ε���(����Ʈ.���ϵ�(i)�� �� ��)
                int last = DeckManager.Inst.deck.Count / 3 + (DeckManager.Inst.deck.Count % 3 == 0 ? 0 : 1) - 1;
                //last�� ������ ���� �ε���
                int j = -1;//j�� �ش� ī�尡 �ִ� �ٿ��� ���°�ΰ�
                Transform content = this.transform.parent.transform.parent.transform;
                switch (this.transform.localPosition.x)
                {
                    case -110.2f:
                        j = 0;
                        break;
                    case 0f:
                        j = 1;
                        break;
                    case 110.2f:
                        j = 2;
                        break;
                }
                if (3 * i + j == DeckManager.Inst.deck.Count - 1)//������ ī���� ���
                {
                    DeckManager.Inst.deck.RemoveAt(i * 3 + j);
                    if (j == 0)
                        Destroy(this.transform.parent.gameObject);
                    else
                        Destroy(this.gameObject);
                }
                else if (i == last)//�������� �ƴѵ� ������ ���� ���
                {
                    switch (j)
                    {
                        case 0:
                            this.transform.parent.transform.GetChild(1).localPosition = new Vector3(-110.2f, 0f, 0f);
                            if (DeckManager.Inst.deck.Count % 3 == 0)
                                this.transform.parent.transform.GetChild(2).localPosition = new Vector3(0f, 0f, 0f);
                            break;
                        case 1:
                            this.transform.parent.transform.GetChild(2).localPosition = new Vector3(0f, 0f, 0f);
                            break;
                    }
                    DeckManager.Inst.deck.RemoveAt(i * 3 + j);
                    Destroy(this.gameObject);

                }
                else//������
                {
                    print("i : " + i + ", last : " + last);
                    if (j < 1)
                        this.transform.parent.transform.GetChild(1).localPosition = new Vector3(-110.2f, 0f, 0f);
                    if (j < 2)
                        this.transform.parent.transform.GetChild(2).localPosition = new Vector3(0f, 0f, 0f);
                    for (int k = i + 1; k < last+1; k++)
                    {
                        temp = Instantiate(content.GetChild(k).transform.GetChild(0).gameObject, content.GetChild(k - 1));
                        temp.transform.localPosition = new Vector3(110.2f, 0f, 0f);
                        if (k == last)
                        {
                            if (content.GetChild(k).childCount == 1)
                                Destroy(content.GetChild(k).gameObject);
                            else
                            {
                                Destroy(content.GetChild(k).transform.GetChild(0).gameObject);
                                content.GetChild(k).transform.GetChild(1).localPosition = new Vector3(-110.2f, 0f, 0f);
                                if (content.GetChild(k).childCount > 2)
                                    content.GetChild(k).transform.GetChild(2).localPosition = new Vector3(0f, 0f, 0f);
                            }
                        }
                        else
                        {
                            Destroy(content.GetChild(k).transform.GetChild(0).gameObject);
                            content.GetChild(k).transform.GetChild(1).localPosition = new Vector3(-110.2f, 0f, 0f);
                            content.GetChild(k).transform.GetChild(2).localPosition = new Vector3(0f, 0f, 0f);
                        }
                    }
                    DeckManager.Inst.deck.RemoveAt(i * 3 + j);
                    Destroy(this.gameObject);
                }
                {
                //else//��������, ������ �ٵ� �ƴ� ���
                //    {
                //        if (j < 1)
                //            this.transform.parent.transform.GetChild(1).localPosition = new Vector3(-110.2f, 0f, 0f);
                //        if (j < 2)
                //            this.transform.parent.transform.GetChild(2).localPosition = new Vector3(0f, 0f, 0f);
                //        for (int k = i + 1; ;)
                //        {
                //            GameObject temp_ = Instantiate(this.transform.parent.transform.parent.transform.GetChild(k).transform.GetChild(0).gameObject, this.transform.parent);
                //            temp_.transform.localPosition = new Vector3(110.2f, 0f, 0f);
                //            if (k == last//�������� �������̰�, �� ���� �ڽ��� �ϳ��� ������ ��
                //                && this.transform.parent.transform.parent.transform.GetChild(k).childCount == 1)
                //            {
                //                Destroy(this.transform.parent.transform.parent.transform.GetChild(k).gameObject);
                //                break;
                //            }
                //            else if (k == last)//�������� �������϶�
                //            {
                //                //print(i + 1+"��°�� �ڽļ�");
                //                //print("�ı���"+this.transform.parent.transform.parent.transform.GetChild(i + 1).childCount);
                //                Destroy(this.transform.parent.transform.parent.transform.GetChild(k).transform.GetChild(0).gameObject);
                //                this.transform.parent.transform.parent.transform.GetChild(k).transform.GetChild(1).localPosition = new Vector3(-110.2f, 0f, 0f);
                //                if (this.transform.parent.transform.parent.transform.GetChild(k).childCount > 2)
                //                    this.transform.parent.transform.parent.transform.GetChild(k).transform.GetChild(2).localPosition = new Vector3(0f, 0f, 0f);
                //                break;
                //            }
                //            else//�������� �������� ��
                //            {
                //                Destroy(this.transform.parent.transform.parent.transform.GetChild(k).transform.GetChild(0).gameObject);
                //                this.transform.parent.transform.parent.transform.GetChild(k).transform.GetChild(1).localPosition = new Vector3(-110.2f, 0f, 0f);
                //                this.transform.parent.transform.parent.transform.GetChild(k).transform.GetChild(2).localPosition = new Vector3(0f, 0f, 0f);
                //                k = k + 1;
                //            }
                //        }
                //        switch (DeckManager.Inst.deck.Count % 3)
                //        {
                //            case 1://������ ���� �ϳ��� �����ִ� ���
                //                break;
                //            case 2://������ �ٿ� ���� �����ִ� ���
                //                break;
                //            case 0://���������� �� ���ִ� ���
                //                break;
                //        }

                //        DeckManager.Inst.deck.RemoveAt(i * 3 + j);
                //        Destroy(this.gameObject);
                //    }
                }
                break;
        }
    }

    int deckErrCheck(CardData data)
    {
        string hqColor = "";//HQ��
        int defaultThis = 0;//���� �ش�ī�� ���
        int rareType = 0;//Ÿ��(0:���, 1:����, 2:�غ�, 3:����)
        foreach (var item in data.Keyword)
        {
            if (item == "��ū")
            {
                return 0;
            }
            if (item == "����")
                rareType = 3;
            if (item == "����")
                rareType = 1;
            if (item == "�غ�")
                rareType = 2;
        }
        if (data.Color != "C")
        {
            List<string> defaultColor = new List<string>();
            foreach (var item in DeckManager.Inst.deck)
            {
                if (!defaultColor.Contains(item.Color)
                    && item.Color != "C")
                    defaultColor.Add(item.Color);
            }
            if (defaultColor.Count >= 2//���� ���� 2�� �̻��̰�
                && !defaultColor.Contains(data.Color))//���ο� ���� ������ ���ٸ�(3���̻��̶��)
                return 1;
        }
        foreach (var item in DeckManager.Inst.deck)
        {
            if (item.Kind == "HQ")
            {
                if (data.Kind == "HQ")
                    return 2;
                if (rareType == 3)
                {
                    hqColor = item.Color;
                    if (data.Color != hqColor)
                        return 3;
                }
            }
            if (item.No == data.No)
                defaultThis++;
        }
        if (rareType == 3
            && hqColor == "")
            return 4;
        if (rareType == 0)
        {
            if (defaultThis >= 4)
                return 5;
        }
        else
        {
            if (defaultThis >= rareType)
            {
                switch (rareType)
                {
                    case 1: return 6;
                    case 2: return 7;
                    case 3:
                        break;
                }
            }
        }
        return -1;
    }
}

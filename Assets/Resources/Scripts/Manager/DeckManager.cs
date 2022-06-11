using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] public cardSO deckSO;
    [SerializeField] public cardSO cardSO;
    [SerializeField] GameObject DeckPanel;

    CardJsonData cardData;
    DeckJsonData deckData;
    public List<CardData> deck;



    void Start()
    {
        if (json.FileExist(Application.dataPath, "card"))
            cardData = json.LoadJsonFile<CardJsonData>(Application.dataPath, "card");
        if (json.FileExist(Application.dataPath, "deck"))
            deckData = json.LoadJsonFile<DeckJsonData>(Application.dataPath, "deck");
        cardSO.cards = new List<CardData>();
        foreach (var item in cardData.cardList)
            cardSO.cards.Add(item);
        deckUpdate();
    }

    void Update()
    {
        
    }

    public void deckUpdate()
    {
        deck = new List<CardData>();
        foreach (var item in deckData.deck)
            foreach (var items in cardData.cardList)
                if (item == items.No)
                    deck.Add(items);
        print("��밡�ɿ���" + deckCheck(deck));
        deckSO.cards = new List<CardData>();
        foreach (var item in deck)        
            deckSO.cards.Add(item);        
    }

    public bool deckCheck(List<CardData> deck)
    {
        int hqCheck = 0;
        string hqColor = null;
        string[] colors = new string[0] { };
        foreach (var item in deck)
        {
            if (item.Kind == "HQ")
            {
                hqCheck++;
                if (hqCheck > 1)//HQ�� 1���� �Ѵ´ٸ�
                {
                    return false;//���Ұ�
                }
                hqColor = item.Color;
            }
            if (Array.IndexOf(colors, item.Color) == -1)//�ش� ī���� ���� ���迭�� ���ٸ�
            {
                if (item.Color == "C")
                    continue;
                Array.Resize(ref colors, colors.Length + 1);//���� 1 �߰�
                colors[colors.Length - 1] = item.Color;//�ش� ���� �迭�� �߰�
            }
        }
        if (hqCheck < 1//HQ�� 1������ ���ų�
            || colors.Length > 2)//���� 2�������� ���ٸ�
            return false;//���Ұ�
        if (deck.Count > 60+1 //����� 60���� �Ѱų�
            || deck.Count < 40+1)//40���� �ȵȴٸ�
            return false;//���Ұ�(HQ����)
        foreach (var item in deck)
        {
            if (Array.IndexOf(item.Keyword, "����") != -1)//������ �ִٸ�
            {
                if (item.Color != hqColor)//hq�� ���� �ƴ϶��
                    return false;//���Ұ�
            }
        }
        return true;//��� ����
    }

    public void saveDeck()
    {
        if (deckCheck(deck))
            jsonSave();
        else
            NetworkManager.Inst.warningWindow();
    }

    public void clearDeck()
    {
        deck = new List<CardData>();
        NetworkManager.Inst.reset(1);
    }

    public void jsonSave()
    {
        deckData.deck = new string[deck.Count];
        for (int i = 0; i < deck.Count; i++)
        {
            deckData.deck[i] = deck[i].No;
        }
        string temp = json.ObjectToJson(deckData);
        json.CreateJsonFile(Application.dataPath, "deck", temp);
    }
}

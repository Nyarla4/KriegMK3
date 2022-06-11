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
        print("사용가능여부" + deckCheck(deck));
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
                if (hqCheck > 1)//HQ가 1개를 넘는다면
                {
                    return false;//사용불가
                }
                hqColor = item.Color;
            }
            if (Array.IndexOf(colors, item.Color) == -1)//해당 카드의 색이 색배열에 없다면
            {
                if (item.Color == "C")
                    continue;
                Array.Resize(ref colors, colors.Length + 1);//길이 1 추가
                colors[colors.Length - 1] = item.Color;//해당 색을 배열에 추가
            }
        }
        if (hqCheck < 1//HQ가 1개보다 적거나
            || colors.Length > 2)//색이 2가지보다 많다면
            return false;//사용불가
        if (deck.Count > 60+1 //장수가 60장을 넘거나
            || deck.Count < 40+1)//40장이 안된다면
            return false;//사용불가(HQ제외)
        foreach (var item in deck)
        {
            if (Array.IndexOf(item.Keyword, "제식") != -1)//제식이 있다면
            {
                if (item.Color != hqColor)//hq의 색이 아니라면
                    return false;//사용불가
            }
        }
        return true;//사용 가능
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

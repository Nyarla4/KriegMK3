using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class card
{
    public string No;
    public string Name;
    public int Attack;
    public int Health;
    public string Sprite;
    public string Faction;
    public string Color;
    public string CardFront;
    public int NeutralCost;
    public int ColorCost;
    public string Kind;
    public string[] Keyword;
    public string[] Effect;
    public string[] Tag;
}
[CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Object/CardSO")]     //���� �� ������ �迭�� ���� �� �ְ� �ȴ�
public class cardSO : ScriptableObject//monobehavier->scriptableObject
{
    public List<CardData> cards;
}

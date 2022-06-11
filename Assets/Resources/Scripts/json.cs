using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;//FileStream에 필요
using System.Text;//Encoding, var에 필요

[System.Serializable]
public class CardData
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
[System.Serializable]
public class CardJsonData
{
    public List<CardData> cardList = new List<CardData>();
    public Dictionary<string, CardData> MakeData()
    {
        Dictionary<string, CardData> temp = new Dictionary<string, CardData>();
        foreach (CardData item in cardList)
            temp.Add(item.No, item);
        return temp;
    }
    public CardJsonData()
    {
        CardData nullCard = new CardData();
        cardList.Add(nullCard);
    }
}
[System.Serializable]
public class DeckJsonData
{
    public string[] deck;
}
public class json : MonoBehaviour
{
    void Start()
    {
        
    }
    void Update()
    {
        
    }

    public static string ObjectToJson(object obj)//ObjectToJson() 함수는 JsonUtility 클래스의 ToJson() 함수를 이용해서 오브젝트를 문자열로 된 JSON 데이터로 변환하여 반환하는 처리
    {
        return JsonUtility.ToJson(obj);
    }
    public static T JsonToObject<T>(string jsonData)//JsonToObject() 함수는 FromJson() 함수를 이용해서 문자열로 된 JSON 데이터를 받아서 원하는 타입의 객체로 반환하는 처리
    {
        return JsonUtility.FromJson<T>(jsonData);
    }
    public static void CreateJsonFile(string createPath, string fileName, string jsonData)
    {
        FileStream fileStream = new FileStream(string.Format("{0}/Resources/Json/{1}.json", createPath, fileName), FileMode.Create);
        byte[] data = Encoding.UTF8.GetBytes(jsonData);
        fileStream.Write(data, 0, data.Length);
        fileStream.Close();
    }
    public static T LoadJsonFile<T>(string loadPath, string fileName)
    {
        FileStream fileStream = new FileStream(string.Format("{0}/Resources/Json/{1}.json", loadPath, fileName), FileMode.Open);

        byte[] data = new byte[fileStream.Length];
        fileStream.Read(data, 0, data.Length);
        fileStream.Close();
        string jsonData = Encoding.UTF8.GetString(data);
        return JsonUtility.FromJson<T>(jsonData);
    }
    //게임 종료시 temp삭제 요
    public static void DeleteJsonFile(string loadPath, string fileName)//path:Application.dataPath
    {
        System.IO.File.Delete(string.Format("{0}/Resources/Json/{1}.json", loadPath, fileName));
    }

    public static bool FileExist(string loadPath, string fileName)
    {
        FileInfo info = new FileInfo(string.Format("{0}/Resources/Json/{1}.json", loadPath, fileName));
        return info.Exists;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Inst { get; private set; }

    [ContextMenu("����")]
    void info()
    {
        if (PhotonNetwork.InRoom)
        {
            print("���� �� : " + PhotonNetwork.CurrentRoom.Name);
            print("���� �ο� : " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers);

            string playerStr = "�÷��̾� ��� : ";
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                playerStr += PhotonNetwork.PlayerList[i].NickName;
                if (i != PhotonNetwork.PlayerList.Length - 1)
                    playerStr += ", ";
            }            
            print(playerStr);
        } 
        else
        {
            print("���� �ο� �� : " + PhotonNetwork.CountOfPlayers);
            print("�� ���� : " + PhotonNetwork.CountOfRooms);
            print("��� �� �� �ο� �� : " + PhotonNetwork.CountOfPlayersInRooms);
            print("���� �κ� �� ���� : " + PhotonNetwork.InLobby);
            print("���� ���� : " + PhotonNetwork.IsConnected);
        }
    }

    [Header("DisconnectPanel")]
    [SerializeField] GameObject DisconnectPanel;
    [SerializeField] InputField nicknameInput;

    [Header("LobbyPanel")]
    [SerializeField] GameObject LobbyPanel;
    [SerializeField] InputField roomInput;
    [SerializeField] Text welcomeText;
    [SerializeField] Text LobbyInfoText;
    [SerializeField] Button[] CellBtn;
    [SerializeField] Button prevBtn;
    [SerializeField] Button nextBtn;
    [SerializeField] InputField maxInput;

    [Header("RoomPanel")]
    [SerializeField] GameObject RoomPanel;
    [SerializeField] Text ListText;
    [SerializeField] Text RoomInfoText;
    [SerializeField] Text[] ChatText;
    [SerializeField] InputField ChatInput;

    [Header("ETC")]
    [SerializeField] Text StatusText;
    [SerializeField] PhotonView PV;

    [Header("DeckPanel")]
    [SerializeField] GameObject DeckPanel;

    [Header("Manager")]
    [SerializeField] GameObject cardManager;

    [Header("PlayPanel")]
    [SerializeField] GameObject PlayPanel;
    [SerializeField] GameObject PlayBG;

    List<RoomInfo> myList = new List<RoomInfo>();
    int currPage = 1, maxPage, multiple;

    GameObject PT = null;

    #region ��������
    void Awake()
    {
        Inst = this;
        //Screen.SetResolution(960, 540, false);
        DisconnectPanel.SetActive(true);
        LobbyPanel.SetActive(false);
        RoomPanel.SetActive(false);
        DeckPanel.SetActive(false);
        PlayPanel.SetActive(false);
        PlayBG.SetActive(false);
    }
    void Update()
    {
        //����
        StatusText.text = "����";
        StatusText.rectTransform.sizeDelta = new Vector2(240, 100);
        StatusText.rectTransform.localPosition = new Vector3(-360, 220, 0);
        if (PlayPanel.activeSelf)
        {
            StatusText.text = "�÷��� ��";
        }
        else if (DeckPanel.activeSelf)
        {
            StatusText.rectTransform.sizeDelta = new Vector2(480, 100);
            StatusText.rectTransform.localPosition = new Vector3(-240, 220, 0);
            StatusText.text += " �� ������";
            StatusText.text += " / ���� �� ��� : ";
            StatusText.text += DeckManager.Inst.deck.Count;
        }
        else
        {
            switch (PhotonNetwork.NetworkClientState)
            {
                case ClientState.PeerCreated:
                case ClientState.Disconnected:
                    StatusText.text += " ��ġ : ����";
                    break;
                case ClientState.JoinedLobby:
                    StatusText.text += " ��ġ : �κ�";
                    break;
                case ClientState.Joined:
                    StatusText.text += " ��ġ : ��";
                    break;
                //case ClientState.JoiningLobby:
                //    break;
                //case ClientState.DisconnectingFromMasterServer:
                //    break;
                //case ClientState.ConnectingToGameServer:
                //    break;
                //case ClientState.ConnectedToGameServer:
                //    break;
                //case ClientState.Joining:
                //    break;
                //case ClientState.Leaving:
                //    break;
                //case ClientState.DisconnectingFromGameServer:
                //    break;
                //case ClientState.ConnectingToMasterServer:
                //    break;
                //case ClientState.Disconnecting:
                //    break;
                case ClientState.ConnectedToMasterServer:
                    StatusText.text += " �� ���� ����";
                    break;
                //case ClientState.ConnectingToNameServer:
                //    break;
                //case ClientState.ConnectedToNameServer:
                //    break;
                //case ClientState.DisconnectingFromNameServer:
                //    break;
                //case ClientState.ConnectWithFallbackProtocol:
                //    break;
                default:
                    StatusText.text += " ���� ��";
                    break;
            }
        }
        //StatusText.text = PhotonNetwork.NetworkClientState.ToString();
        if (LobbyPanel.activeSelf)
            LobbyInfoText.text = "�κ� " + (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + " / ���� " + PhotonNetwork.CountOfPlayers;
        
        if (RoomPanel.activeSelf
            && GameObject.Find("ReadyOrStart").transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == "�غ���")
        {
            GameObject[] temp = GameObject.FindGameObjectsWithTag("Player");
            foreach (var item in temp)
            {
                if (!item.GetPhotonView().IsMine
                    && item.GetComponent<roomPlayer>().getReady())
                {
                    RoomPanel.SetActive(false);
                    PlayPanel.SetActive(true);
                    PlayBG.SetActive(true);
                    GameObject.Find("Main Camera").transform.rotation =
                        PlayBG.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
                    foreach (var playerTemp in temp)
                    {
                        if (playerTemp.GetComponent<PhotonView>().IsMine)
                        {
                            playerTemp.transform.position = new Vector3(0, 4, 0);
                            playerTemp.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
                        }
                    }
                    PT.GetComponent<roomPlayer>().setTurn(false);
                    CardManager.Inst.SetupDeckBuffer();
                    CardManager.Inst.GameStart(false);
                    break;
                }
            }
        }
    }
    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
        DisconnectPanel.SetActive(false);
    }
    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();//�����ͼ��� ���ӽ� �ٷ� �κ�� ����
    public override void OnJoinedLobby()
    {
        LobbyPanel.SetActive(true);
        RoomPanel.SetActive(false);
        PhotonNetwork.LocalPlayer.NickName = nicknameInput.text;
        welcomeText.text = PhotonNetwork.LocalPlayer.NickName + "�ÿ� ����ʽÿ�";
        myList.Clear();//�κ� ���ư��� ����Ʈ �ʱ�ȭ
    }
    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
        DisconnectPanel.SetActive(true);
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (LobbyPanel)
            LobbyPanel.SetActive(false);
        if (RoomPanel)
            RoomPanel.SetActive(false);
    }
    #endregion

    #region ��
    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(//�� ����
        roomInput.text == "" ? "Room" + Random.Range(0, 100) : roomInput.text,//��������� Room+0~99�� �����߰�
        new RoomOptions { MaxPlayers = maxInput.text == "" ? byte.Parse(2.ToString()) : byte.Parse(maxInput.text) });//�ִ� �μ�
    }
    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();//���� �� ����
    public void LeaveRoom()
    {
        Destroy(PT.gameObject);
        PT = null;
        GameObject[] nameTag = GameObject.FindGameObjectsWithTag("nameTag");
        foreach (var item in nameTag)
            Destroy(item.gameObject);
        PhotonNetwork.LeaveRoom();//�� ������
    }
    public override void OnJoinedRoom()
    {
        PT=PhotonNetwork.Instantiate("Prefabs/roomPlayer", Vector3.zero, Quaternion.identity);
        LobbyPanel.SetActive(false);
        RoomPanel.SetActive(true);
        RoomPanel.transform.GetChild(7).gameObject.SetActive(false);
        RoomRenewal();//�� ���� �ʱ�ȭ
        ChatInput.text = "";//ä��â �ʱ�ȭ
        foreach (var item in ChatText)//ä��â �ʱ�ȭ
            item.text = "";
    }

    //�� ���� �� ���� ���� ���� ��
    public override void OnCreateRoomFailed(short returnCode, string message) { roomInput.text = ""; CreateRoom(); }
    public override void OnJoinRandomFailed(short returnCode, string message) { roomInput.text = ""; CreateRoom(); }

    //�÷��̾� ����/����
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RoomRenewal();
        PV.RPC("ChatRPC", RpcTarget.All, "<color=yellow>" + newPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!PlayPanel.activeSelf)
        {
            RoomRenewal();
            PV.RPC("ChatRPC", RpcTarget.All, "<color=yellow>" + otherPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");
        }
    }
    private void RoomRenewal()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
            GameObject.Find("ReadyOrStart").transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "����";
        else
            GameObject.Find("ReadyOrStart").transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "�غ�";
        ListText.text = "";//roomPanel�� ���� �÷��̾� ����Ʈ
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            ListText.text += 
                PhotonNetwork.PlayerList[i].NickName 
                + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : ", ");
        RoomInfoText.text = PhotonNetwork.CurrentRoom.Name + " / " 
            + PhotonNetwork.CurrentRoom.PlayerCount + "/" 
            + PhotonNetwork.CurrentRoom.MaxPlayers + "��";
        GameObject[] nameTag = GameObject.FindGameObjectsWithTag("nameTag");
        foreach (var item in nameTag)
        {
            int test = 0;
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (item.GetComponent<Text>().text == PhotonNetwork.PlayerList[i].NickName)
                    test++;
            }
            if (test == 0)
                Destroy(item.gameObject);
        }
        PT.GetComponent<roomPlayer>().setReady(false);
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            GameObject.Find("ReadyOrStart").GetComponent<Button>().interactable = true;
            GameObject.Find("ReadyOrStart").GetComponent<Image>().color = GameObject.Find("ReadyOrStart").GetComponent<Button>().colors.normalColor;
        }
        else
            GameObject.Find("ReadyOrStart").GetComponent<Button>().interactable = false;
    }

    public void ReadyOrStart()
    {
        GameObject buttonObject = GameObject.Find("ReadyOrStart");
        TextMeshProUGUI buttonText = buttonObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        switch (buttonText.text)
        {
            case "�غ�":
                if (DeckManager.Inst.deckCheck(DeckManager.Inst.deck))
                {
                    buttonText.text = "�غ���";
                    PT.GetComponent<roomPlayer>().setReady(true);
                    buttonObject.GetComponent<Image>().color = buttonObject.GetComponent<Button>().colors.pressedColor;
                }
                else
                {
                    RoomPanel.transform.GetChild(7).gameObject.SetActive(true);
                }
                break;
            case "�غ���":
                GameObject.Find("ReadyOrStart").transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "�غ�";
                PT.GetComponent<roomPlayer>().setReady(false);
                buttonObject.GetComponent<Image>().color = buttonObject.GetComponent<Button>().colors.normalColor;
                break;
            case "����":
                if (!DeckManager.Inst.deckCheck(DeckManager.Inst.deck))
                {
                    RoomPanel.transform.GetChild(7).gameObject.SetActive(true);
                }
                else
                {
                    GameObject[] tempPlayer = GameObject.FindGameObjectsWithTag("Player");
                    bool readyTest = true;
                    foreach (var item in tempPlayer)
                    {
                        if (!item.GetPhotonView().IsMine)
                            readyTest = item.GetComponent<roomPlayer>().getReady() && readyTest;
                    }
                    if (readyTest)
                    {
                        PT.GetComponent<roomPlayer>().setReady(true);
                        RoomPanel.SetActive(false);
                        PlayPanel.SetActive(true);
                        PlayBG.SetActive(true);
                        foreach (var item in tempPlayer)
                        {
                            if (item.GetComponent<PhotonView>().IsMine)
                                item.transform.position = new Vector3(0, -4, 0);
                        }
                        PT.GetComponent<roomPlayer>().setTurn(true);
                        CardManager.Inst.SetupDeckBuffer();
                        CardManager.Inst.GameStart(true);
                    }
                    else
                        print("���� �Ұ�");
                }
                break;
        }
    }

    public void roomWindowClose()
    {
        RoomPanel.transform.GetChild(7).gameObject.SetActive(false);
    }

    public bool getTurn()
    {
        GameObject[] collection = GameObject.FindGameObjectsWithTag("Player");
        GameObject player = null;
        foreach (var item in collection)
        {
            if (item.GetComponent<PhotonView>().IsMine)
                player = item;
        }

        return player.GetComponent<roomPlayer>().getTurn();
    }
    public void TurnEnd()
    {
        if (PT.GetComponent<roomPlayer>().getTurn())//Ŭ���̾�Ʈ�� �Ͽ�
        {
            switch (TurnManager.Inst.getPhase())
            {
                case PHASE.MAIN://���� ������ ���϶�(�������̰ų� �������� �ƴҶ�)
                    GameObject[] collection = GameObject.FindGameObjectsWithTag("Player");
                    foreach (var item in collection)
                        item.GetComponent<roomPlayer>().setTurn(!item.GetComponent<PhotonView>().IsMine);
                    PT.GetComponent<roomPlayer>().setTurn(false);
                    break;
                case PHASE.PAYING://���� ���϶� : ���� ������ ������, ���࿡�� ���� �Ҷ�
                    int colorCost =
                    TurnManager.Inst.getCost(true);
                    int neutralCost =
                        TurnManager.Inst.getCost(false);
                    int saveColor = 0;
                    int saveNeutral = 0;
                    foreach (var item in EntityManager.Inst.getSaves())
                    {
                        if (item.getColor() == TurnManager.Inst.getSelected())
                            saveColor++;
                        else
                            saveNeutral++;
                    }
                    //1.���࿡�� �ڽ�Ʈ ���� �������� Ȯ��
                    if (colorCost + neutralCost > saveColor + saveNeutral)
                        return;
                    else if (colorCost > saveColor)
                        return;
                    //2.������ ��� ���࿡�� �ڽ�Ʈ ����
                    EntityManager.Inst.useSave(colorCost > 0);//���� �ڽ�Ʈ�� ���Ҵٸ� ������, �ƴ϶�� ������
                    if (colorCost + neutralCost > 0)//�ڽ�Ʈ�� ���Ҵٸ� �ݺ�
                        TurnEnd();
                    
                    //1)mySaves���� ������ ��ƼƼ�� �����Ѵ�
                    //2)�ش� ��ƼƼ�� ���������� �����Ѵ�
                    //3)�ڽ�Ʈ���� ������ ��ƼƼ�� ����ŭ ��´�
                    break;
                case PHASE.TARGETING:
                    break;
                case PHASE.WAITING:
                    break;
                case PHASE.NUM:
                    break;
                default:
                    break;
            }
        }
        
    }

    #endregion

    #region ä��
    public void Send()
    {
        string msg = PhotonNetwork.NickName + " : " + ChatInput.text;
        PV.RPC("ChatRPC", RpcTarget.All, msg);
        ChatInput.text = "";
    }
    [PunRPC]
    void ChatRPC(string msg)
    {
        bool isInput = false;
        foreach (var item in ChatText)
            if (item.text == "")
            {
                isInput = true;
                item.text = msg;
                break;
            }
        if(!isInput)
        {
            for (int i = 0; i < ChatText.Length; i++)
                ChatText[i - 1].text = ChatText[i].text;
            ChatText[ChatText.Length - 1].text = msg;
        }
    }
    #endregion

    #region �渮��Ʈ ����
    //<:-2, >:-1
    public void MyListClick(int num)
    {
        switch (num)
        {
            case -2: --currPage; break;
            case -1: ++currPage; break;
            default:
                PhotonNetwork.JoinRoom(myList[multiple + num].Name);
                break;
        }
        MyListRenewal();
    }

    void MyListRenewal()
    {
        maxPage = (myList.Count % CellBtn.Length == 0) ? //�� ������ ���� �ִ� ������ �� ó��
            myList.Count / CellBtn.Length : 
            myList.Count / CellBtn.Length + 1;
        prevBtn.interactable = (currPage <= 1) ? false : true;//1��������� ��Ȱ��ȭ
        nextBtn.interactable = (currPage >= maxPage) ? false : true;//��������������� ��Ȱ��ȭ

        multiple = (currPage - 1) * CellBtn.Length;//�� �������� ù��° ��
        for (int i = 0; i < CellBtn.Length; i++)
        {
            CellBtn[i].interactable = (multiple + i < myList.Count) ? true : false;//myList ���� ����� Ȱ��ȭ
            CellBtn[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
            CellBtn[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + " / " + myList[multiple].MaxPlayers : "0 / 0";
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)//���⿡�� �޾ƾ��ϹǷ� myList�� ���´� List<RoomInfo>
    {
        foreach (var item in roomList)
        {
            if (!item.RemovedFromList)//item�� ���� �����ϴ� ���̶��
            {
                if (!myList.Contains(item)) myList.Add(item);//item�� myList�� ���ٸ� �߰�
                else myList[myList.IndexOf(item)] = item;//�ִٸ� ����
            }
            else if (myList.IndexOf(item) != -1)//item�� index�� �������� ���� ���
                myList.RemoveAt(myList.IndexOf(item));//���ش�
        }
        MyListRenewal();//����
    }
    #endregion

    #region ��
    public void DeckSetting()
    {
        DisconnectPanel.SetActive(false);
        DeckPanel.SetActive(true);
        DeckPanel.transform.GetChild(5).gameObject.SetActive(false);
        cardSet();
        deckSet();
    }
    public void LeaveDeck()
    {
        reset(0);
        reset(1);
        DeckManager.Inst.deckUpdate();
        DisconnectPanel.SetActive(true);
        DeckPanel.transform.GetChild(5).gameObject.SetActive(false);
        DeckPanel.SetActive(false);
    }

    void cardSet()
    {
        Transform contentPlace = DeckPanel.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0);
        int line = DeckManager.Inst.cardSO.cards.Count / 3 + (DeckManager.Inst.cardSO.cards.Count % 3 == 0 ? 0 : 1);
        //for (int i = 0; i < line; i++)
        //{
        //    Instantiate(Resources.Load("Prefabs/group") as GameObject, contentPlace);
        //}
        for (int i = 0; i < line;)
        {
            Instantiate(Resources.Load("Prefabs/group") as GameObject, contentPlace);
            for (int j = 0; j < 3; j++)
            {
                if (i * 3 + j == DeckManager.Inst.cardSO.cards.Count)
                    return;
                GameObject temp =
                Instantiate(Resources.Load("Prefabs/UICard") as GameObject, contentPlace.GetChild(i));
                temp.GetComponent<UICard>().Setup(DeckManager.Inst.cardSO.cards[i * 3 + j]);
                float posX = 0;
                switch (j)
                {
                    case 0: posX = -110.2f; break;
                    case 1: posX = 0f; break;
                    case 2: posX = 110.2f; break;
                }
                temp.transform.localPosition = new Vector3(posX, 0, 0);
                if (j == 2)
                    i++;
            }
        }
    }

    public void deckSet()
    {
        Transform contentPlace = DeckPanel.transform.GetChild(1).transform.GetChild(0).transform.GetChild(0);
        int line = DeckManager.Inst.deck.Count / 3 + (DeckManager.Inst.deck.Count % 3 == 0 ? 0 : 1);
        for (int i = 0; i < line;)
        {
            Instantiate(Resources.Load("Prefabs/group") as GameObject, contentPlace);
            for (int j = 0; j < 3; j++)
            {
                if (i * 3 + j == DeckManager.Inst.deck.Count)
                    return;
                GameObject temp =
                Instantiate(Resources.Load("Prefabs/UICard") as GameObject, contentPlace.GetChild(i));
                temp.GetComponent<UICard>().Setup(DeckManager.Inst.deck[i * 3 + j]);
                float posX = 0;
                switch (j)
                {
                    case 0: posX = -110.2f; break;
                    case 1: posX = 0f; break;
                    case 2: posX = 110.2f; break;
                }
                temp.transform.localPosition = new Vector3(posX, 0, 0);
                if (j == 2)
                    i++;
            }
        }
    }

    public void reset(int j)//0:ī�帮��Ʈ, 1:������Ʈ
    {
        Transform contentPlace = DeckPanel.transform.GetChild(j).transform.GetChild(0).transform.GetChild(0);
        if (contentPlace.childCount > 0)
        {
            for (int i = 0; i < contentPlace.childCount; i++)
            {
                Destroy(contentPlace.GetChild(i).gameObject);
            }
        }
        else
            return;
    }

    public void warningWindow()
    {
        DeckPanel.transform.GetChild(5).gameObject.SetActive(true);
        DeckPanel.transform.GetChild(5).transform.GetChild(0).gameObject.SetActive(true);
        DeckPanel.transform.GetChild(5).transform.GetChild(1).gameObject.SetActive(false);
    }
    public void warningYes()
    {
        DeckManager.Inst.jsonSave();
        DeckPanel.transform.GetChild(5).gameObject.SetActive(false);
    }
    public void warningNo()
    {
        DeckPanel.transform.GetChild(5).gameObject.SetActive(false);
    }

    public void cantAddWindow(string msg)
    {
        DeckPanel.transform.GetChild(5).gameObject.SetActive(true);
        DeckPanel.transform.GetChild(5).transform.GetChild(1).gameObject.SetActive(true);
        DeckPanel.transform.GetChild(5).transform.GetChild(0).gameObject.SetActive(false);
        DeckPanel.transform.GetChild(5).transform.GetChild(1).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = msg;
    }
    #endregion
}

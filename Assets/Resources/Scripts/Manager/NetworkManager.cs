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

    [ContextMenu("정보")]
    void info()
    {
        if (PhotonNetwork.InRoom)
        {
            print("현재 방 : " + PhotonNetwork.CurrentRoom.Name);
            print("현재 인원 : " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers);

            string playerStr = "플레이어 목록 : ";
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
            print("접속 인원 수 : " + PhotonNetwork.CountOfPlayers);
            print("방 개수 : " + PhotonNetwork.CountOfRooms);
            print("모든 방 내 인원 수 : " + PhotonNetwork.CountOfPlayersInRooms);
            print("현재 로비 내 여부 : " + PhotonNetwork.InLobby);
            print("연결 여부 : " + PhotonNetwork.IsConnected);
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

    #region 서버연결
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
        //상태
        StatusText.text = "현재";
        StatusText.rectTransform.sizeDelta = new Vector2(240, 100);
        StatusText.rectTransform.localPosition = new Vector3(-360, 220, 0);
        if (PlayPanel.activeSelf)
        {
            StatusText.text = "플레이 중";
        }
        else if (DeckPanel.activeSelf)
        {
            StatusText.rectTransform.sizeDelta = new Vector2(480, 100);
            StatusText.rectTransform.localPosition = new Vector3(-240, 220, 0);
            StatusText.text += " 덱 설정중";
            StatusText.text += " / 현재 덱 장수 : ";
            StatusText.text += DeckManager.Inst.deck.Count;
        }
        else
        {
            switch (PhotonNetwork.NetworkClientState)
            {
                case ClientState.PeerCreated:
                case ClientState.Disconnected:
                    StatusText.text += " 위치 : 서버";
                    break;
                case ClientState.JoinedLobby:
                    StatusText.text += " 위치 : 로비";
                    break;
                case ClientState.Joined:
                    StatusText.text += " 위치 : 방";
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
                    StatusText.text += " 방 접속 실패";
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
                    StatusText.text += " 접속 중";
                    break;
            }
        }
        //StatusText.text = PhotonNetwork.NetworkClientState.ToString();
        if (LobbyPanel.activeSelf)
            LobbyInfoText.text = "로비 " + (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + " / 접속 " + PhotonNetwork.CountOfPlayers;
        
        if (RoomPanel.activeSelf
            && GameObject.Find("ReadyOrStart").transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == "준비중")
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
    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();//마스터서버 접속시 바로 로비로 접속
    public override void OnJoinedLobby()
    {
        LobbyPanel.SetActive(true);
        RoomPanel.SetActive(false);
        PhotonNetwork.LocalPlayer.NickName = nicknameInput.text;
        welcomeText.text = PhotonNetwork.LocalPlayer.NickName + "시여 어서오십시오";
        myList.Clear();//로비 돌아갈때 리스트 초기화
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

    #region 방
    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(//방 생성
        roomInput.text == "" ? "Room" + Random.Range(0, 100) : roomInput.text,//비어있으면 Room+0~99중 숫자추가
        new RoomOptions { MaxPlayers = maxInput.text == "" ? byte.Parse(2.ToString()) : byte.Parse(maxInput.text) });//최대 인수
    }
    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();//랜덤 방 입장
    public void LeaveRoom()
    {
        Destroy(PT.gameObject);
        PT = null;
        GameObject[] nameTag = GameObject.FindGameObjectsWithTag("nameTag");
        foreach (var item in nameTag)
            Destroy(item.gameObject);
        PhotonNetwork.LeaveRoom();//방 나가기
    }
    public override void OnJoinedRoom()
    {
        PT=PhotonNetwork.Instantiate("Prefabs/roomPlayer", Vector3.zero, Quaternion.identity);
        LobbyPanel.SetActive(false);
        RoomPanel.SetActive(true);
        RoomPanel.transform.GetChild(7).gameObject.SetActive(false);
        RoomRenewal();//방 정보 초기화
        ChatInput.text = "";//채팅창 초기화
        foreach (var item in ChatText)//채팅창 초기화
            item.text = "";
    }

    //방 제작 및 랜덤 입장 실패 시
    public override void OnCreateRoomFailed(short returnCode, string message) { roomInput.text = ""; CreateRoom(); }
    public override void OnJoinRandomFailed(short returnCode, string message) { roomInput.text = ""; CreateRoom(); }

    //플레이어 참가/퇴장
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RoomRenewal();
        PV.RPC("ChatRPC", RpcTarget.All, "<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!PlayPanel.activeSelf)
        {
            RoomRenewal();
            PV.RPC("ChatRPC", RpcTarget.All, "<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
        }
    }
    private void RoomRenewal()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
            GameObject.Find("ReadyOrStart").transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "시작";
        else
            GameObject.Find("ReadyOrStart").transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "준비";
        ListText.text = "";//roomPanel에 있을 플레이어 리스트
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            ListText.text += 
                PhotonNetwork.PlayerList[i].NickName 
                + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : ", ");
        RoomInfoText.text = PhotonNetwork.CurrentRoom.Name + " / " 
            + PhotonNetwork.CurrentRoom.PlayerCount + "/" 
            + PhotonNetwork.CurrentRoom.MaxPlayers + "명";
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
            case "준비":
                if (DeckManager.Inst.deckCheck(DeckManager.Inst.deck))
                {
                    buttonText.text = "준비중";
                    PT.GetComponent<roomPlayer>().setReady(true);
                    buttonObject.GetComponent<Image>().color = buttonObject.GetComponent<Button>().colors.pressedColor;
                }
                else
                {
                    RoomPanel.transform.GetChild(7).gameObject.SetActive(true);
                }
                break;
            case "준비중":
                GameObject.Find("ReadyOrStart").transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "준비";
                PT.GetComponent<roomPlayer>().setReady(false);
                buttonObject.GetComponent<Image>().color = buttonObject.GetComponent<Button>().colors.normalColor;
                break;
            case "시작":
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
                        print("시작 불가");
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
        if (PT.GetComponent<roomPlayer>().getTurn())//클라이언트의 턴에
        {
            switch (TurnManager.Inst.getPhase())
            {
                case PHASE.MAIN://메인 페이즈 중일때(저축중이거나 지불중이 아닐때)
                    GameObject[] collection = GameObject.FindGameObjectsWithTag("Player");
                    foreach (var item in collection)
                        item.GetComponent<roomPlayer>().setTurn(!item.GetComponent<PhotonView>().IsMine);
                    PT.GetComponent<roomPlayer>().setTurn(false);
                    break;
                case PHASE.PAYING://지불 중일때 : 패의 지불이 끝나고, 저축에서 지불 할때
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
                    //1.저축에서 코스트 지불 가능한지 확인
                    if (colorCost + neutralCost > saveColor + saveNeutral)
                        return;
                    else if (colorCost > saveColor)
                        return;
                    //2.가능할 경우 저축에서 코스트 지불
                    EntityManager.Inst.useSave(colorCost > 0);//유색 코스트가 남았다면 유색을, 아니라면 무색을
                    if (colorCost + neutralCost > 0)//코스트가 남았다면 반복
                        TurnEnd();
                    
                    //1)mySaves에서 지불할 엔티티를 제거한다
                    //2)해당 엔티티를 물리적으로 제거한다
                    //3)코스트에서 제거한 엔티티의 값만큼 깎는다
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

    #region 채팅
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

    #region 방리스트 갱신
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
        maxPage = (myList.Count % CellBtn.Length == 0) ? //방 개수에 따른 최대 페이지 수 처리
            myList.Count / CellBtn.Length : 
            myList.Count / CellBtn.Length + 1;
        prevBtn.interactable = (currPage <= 1) ? false : true;//1페이지라면 비활성화
        nextBtn.interactable = (currPage >= maxPage) ? false : true;//마지막페이지라면 비활성화

        multiple = (currPage - 1) * CellBtn.Length;//각 페이지의 첫번째 방
        for (int i = 0; i < CellBtn.Length; i++)
        {
            CellBtn[i].interactable = (multiple + i < myList.Count) ? true : false;//myList 범위 내라면 활성화
            CellBtn[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
            CellBtn[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + " / " + myList[multiple].MaxPlayers : "0 / 0";
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)//여기에서 받아야하므로 myList의 형태는 List<RoomInfo>
    {
        foreach (var item in roomList)
        {
            if (!item.RemovedFromList)//item이 현재 존재하는 방이라면
            {
                if (!myList.Contains(item)) myList.Add(item);//item이 myList에 없다면 추가
                else myList[myList.IndexOf(item)] = item;//있다면 갱신
            }
            else if (myList.IndexOf(item) != -1)//item의 index가 존재하지 않을 경우
                myList.RemoveAt(myList.IndexOf(item));//없앤다
        }
        MyListRenewal();//갱신
    }
    #endregion

    #region 덱
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

    public void reset(int j)//0:카드리스트, 1:덱리스트
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

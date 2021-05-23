using MLAPI;
using MLAPI.Transports.SteamP2P;
using Steamworks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ServersList : MonoBehaviour
{
    protected Callback<GameOverlayActivated_t> Callback_gameOverlay;
    protected Callback<LobbyMatchList_t> Callback_lobbyList;
    protected Callback<LobbyDataUpdate_t> Callback_lobbyInfo;
    protected Callback<LobbyEnter_t> Callback_lobbyEnter;
    protected Callback<LobbyCreated_t> Callback_LobbyCreate;
    protected Callback<LobbyChatMsg_t> Callback_lobbyChatMsg;
    protected Callback<LobbyChatUpdate_t> Callback_lobbyChatUpdate;
    protected Callback<AvatarImageLoaded_t> Callback_avatarImageLoaded;

    private NetworkManager networkManager;
    private SteamP2PTransport steamP2P;
    //private ISteamMatchmakingPingResponse pingResponse;

    ulong current_lobbyID;
    List<CSteamID> lobbyIDS;
    string personaName;

    [Header("Instantiate lobbies")]
    [SerializeField] private Transform content;
    [SerializeField] private RoomListing roomListing;

    [Header("Instantiate Players Lobby Info")]
    [SerializeField] private Transform contentPlayers;
    [SerializeField] private PlayerLobbyListing playerLobbyListing;

    [Header("Menu Changes")]
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject serverList;
    [SerializeField] private GameObject connectingLobby;
    [SerializeField] private GameObject inLobby;

    [Space]
    [SerializeField] private TMP_Text lobbiesNr;
    [SerializeField] private TMP_Text pingText;
    [SerializeField] private TMP_Text connectingText;
    [SerializeField] private TMP_Text chatText;
    [SerializeField] private TMP_InputField chatBox;

    void Start()
    {
        lobbyIDS = new List<CSteamID>();
        Callback_gameOverlay = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
        Callback_lobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbiesList);
        Callback_lobbyInfo = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyInfo);
        Callback_lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        Callback_LobbyCreate = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        Callback_lobbyChatMsg = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMsg);
        Callback_lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        Callback_avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
        //pingResponse = new ISteamMatchmakingPingResponse(OnServerResponded, OnServerFailedToRespond);

        networkManager = FindObjectOfType<NetworkManager>();
        steamP2P = FindObjectOfType<SteamP2PTransport>();

        if (SteamAPI.Init())
        {
            Debug.Log("Steam API init -- SUCCESS!");
            personaName = SteamFriends.GetPersonaName();
        }
        else
        {
            Debug.Log("Steam API init -- failure ...");
        }
    }
    void Update()
    {
        SteamAPI.RunCallbacks();

        //Chat
        if (chatBox.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SubmitChatText(chatBox.text);
                chatBox.text = "";
            }
        }
        else
        {
            if (!chatBox.isFocused && Input.GetKeyDown(KeyCode.Return))
            {
                chatBox.ActivateInputField();
            }
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            foreach (Transform child in contentPlayers)
            {
                Destroy(child.gameObject);
            }
            Debug.Log("Destroyed all players Info");

            CheckPlayersLobby();

            Debug.Log("Show up players info");
        }
    }
    private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
    {
        if (pCallback.m_bActive != 0)
        {
            Debug.Log("Steam Overlay has been activated");
        }
        else
        {
            Debug.Log("Steam Overlay has been closed");
        }
    }
    void OnGetLobbiesList(LobbyMatchList_t result)
    {
        Debug.Log("Found " + result.m_nLobbiesMatching + " lobbies!");
        for (int i = 0; i < result.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            Instantiate(roomListing, content);
            lobbyIDS.Add(lobbyID);
            SteamMatchmaking.RequestLobbyData(lobbyID);

            //+++++++++++++++++++++++++++++

            lobbiesNr.text = result.m_nLobbiesMatching.ToString();

            int numPlayers = SteamMatchmaking.GetNumLobbyMembers((CSteamID)lobbyIDS[i]);
            int numLimPlayers = SteamMatchmaking.GetLobbyMemberLimit((CSteamID)lobbyIDS[i]);

            roomListing.serverNr.text = i.ToString();
            roomListing.serverName.text = SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "name");
            roomListing.serverPlayers.text = numPlayers.ToString() + "/" + numLimPlayers.ToString();
        }
    }
    void OnGetLobbyInfo(LobbyDataUpdate_t result)
    {
        for (int i = 0; i < lobbyIDS.Count; i++)
        {
            if (lobbyIDS[i].m_SteamID == result.m_ulSteamIDLobby)
            {
                Debug.Log("Lobby " + i + " :: " + SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "name"));
                return;
            }
        }
    }
    void OnLobbyEntered(LobbyEnter_t result)
    {
        if (result.m_EChatRoomEnterResponse == 1)
        {
            current_lobbyID = result.m_ulSteamIDLobby;
            Debug.Log("Lobby joined!");
            connectingText.text = "Connected!";
            if (menu)
            {
                menu.SetActive(false);
            }
            if (serverList)
            {
                serverList.SetActive(false);
            }
            //inLobby.SetActive(true);
        }
        else
        {
            Debug.Log("Failed to join lobby.");
            connectingText.text = "Failed to connect!";
        }

        CheckPlayersLobby();

        if (networkManager.IsHost) { return; }
        steamP2P.ConnectToSteamID = (ulong)SteamMatchmaking.GetLobbyOwner((CSteamID)current_lobbyID);
        Debug.Log("New Steam Id Owner: " + steamP2P.ConnectToSteamID);

        networkManager.StartClient();

    }
    private void OnLobbyCreated(LobbyCreated_t pCallback)
    {
        if (pCallback.m_eResult == EResult.k_EResultOK)
        {
            Debug.Log("Lobby created -- SUCCESS!");
        }
        else
        {
            Debug.Log("Lobby created -- failure ...");

        }
        networkManager.StartHost();

        string personalName = SteamFriends.GetPersonaName();
        SteamMatchmaking.SetLobbyData((CSteamID)pCallback.m_ulSteamIDLobby, "name", personalName + " adica boss de boss");
    }
    public void LobbyEnter(int lobbyNr)
    {
        connectingLobby.SetActive(true);
        connectingText.text = "Connecting to " + "'" + SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[lobbyNr].m_SteamID, "name") + "'" + " !";

        Debug.Log("Connecting to " + "'" + SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[lobbyNr].m_SteamID, "name") + "'" + " !");
        SteamAPICall_t try_joinLobby = SteamMatchmaking.JoinLobby((CSteamID)lobbyIDS[lobbyNr]);
    }
    public void CreateLobby()
    {
        connectingLobby.SetActive(true);
        connectingText.text = "Trying to create lobby ...";
        Debug.Log("Trying to create lobby ...");
        SteamAPICall_t try_toHost = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 8);
    }
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }
    public void SubmitChatText(string text)
    {
        chatText.text += personaName + ": " + text + '\n';

        byte[] bytes = System.Text.Encoding.ASCII.GetBytes(text);
        Debug.Log("Chat: " + personaName + ": '" + text + "' Len: " + text.Length + " bLen: " + bytes.Length);
        SteamMatchmaking.SendLobbyChatMsg((CSteamID)current_lobbyID, bytes, bytes.Length + 1);
    }
    void OnLobbyChatUpdate(LobbyChatUpdate_t pCallback)
    {
        Debug.Log("[" + LobbyChatUpdate_t.k_iCallback + " - LobbyChatUpdate] - " + pCallback.m_ulSteamIDLobby + " -- " + pCallback.m_ulSteamIDUserChanged + " -- " + pCallback.m_ulSteamIDMakingChange + " -- " + pCallback.m_rgfChatMemberStateChange);
    }
    void OnLobbyChatMsg(LobbyChatMsg_t pCallback)
    {
        Debug.Log("[" + LobbyChatMsg_t.k_iCallback + " - LobbyChatMsg] - " + pCallback.m_ulSteamIDLobby + " -- " + pCallback.m_ulSteamIDUser + " -- " + pCallback.m_eChatEntryType + " -- " + pCallback.m_iChatID);

        CSteamID SteamIDUser;
        byte[] Data = new byte[4096];
        EChatEntryType ChatEntryType;
        int ret = SteamMatchmaking.GetLobbyChatEntry((CSteamID)pCallback.m_ulSteamIDLobby, (int)pCallback.m_iChatID, out SteamIDUser, Data, Data.Length, out ChatEntryType);

        Debug.Log("SteamMatchmaking.GetLobbyChatEntry(" + (CSteamID)pCallback.m_ulSteamIDLobby + ", " + (int)pCallback.m_iChatID + ", out SteamIDUser, Data, Data.Length, out ChatEntryType) : " + ret + " -- " + SteamIDUser + " -- " + System.Text.Encoding.UTF8.GetString(Data) + " -- " + ChatEntryType);

        //if (SteamIDUser.ToString() != SteamUser.GetSteamID().ToString())
        // {
        chatText.text = "";
        string data = System.Text.Encoding.Default.GetString(Data);
        chatText.text += personaName + ": " + data + '\n';
        //  }
    }
    public void LeaveLobbyButton()
    {
        //inLobby.SetActive(false);

        Debug.Log("Leaving Lobby!");
        connectingText.text = "Leaving!";

        SteamMatchmaking.LeaveLobby((CSteamID)current_lobbyID);
        if (networkManager.IsHost)
        {
            networkManager.StopHost();
            networkManager.StopServer();
        }
        else if (networkManager.IsClient)
        {
            networkManager.StopClient();
        }

        current_lobbyID = 0;
        Debug.Log("You are in menu!");
        menu.SetActive(true);
        connectingLobby.SetActive(false);

        foreach (Transform child in contentPlayers)
        {
            Destroy(child.gameObject);
        }
        Debug.Log("Destroyed all players Info");

        //Chat
        chatBox.text = "";
        chatText.text = "";
    }
    public void RefreshButton()
    {
        Debug.Log("Trying to get list of available lobbies ...");
        SteamAPICall_t try_getList = SteamMatchmaking.RequestLobbyList();

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }
    private void CheckPlayersLobby()
    {
        int numPlayers = SteamMatchmaking.GetNumLobbyMembers((CSteamID)current_lobbyID);

        Debug.Log("\t Number of players currently in lobby : " + numPlayers);
        for (int i = 0; i < numPlayers; i++)
        {
            int imageId = SteamFriends.GetLargeFriendAvatar((SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)current_lobbyID, i)));

            Debug.Log("\t Player(" + i + ") == " + SteamFriends.GetFriendPersonaName(SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)current_lobbyID, i)));

            Instantiate(playerLobbyListing, contentPlayers);

            playerLobbyListing.playerImage.texture = GetSteamImage(imageId);
            playerLobbyListing.playerName.text = SteamFriends.GetFriendPersonaName(SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)current_lobbyID, i)).ToString();
            playerLobbyListing.playerNrinLobby.text = (i + 1).ToString();
        }
    }
    private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID.m_SteamID != current_lobbyID) { return; }

        playerLobbyListing.playerImage.texture = GetSteamImage(callback.m_iImage);
    }
    private Texture2D GetSteamImage(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);

        if (isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }
        return texture;
    }
    public void DestroyLobbiesButton()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        Debug.Log("Destroyed all lobbies");
    }
    //Request Latency
    /*
    private void OnServerResponded(gameserveritem_t gsi)
    {
        Debug.Log("OnServerResponded" + gsi);
    }
    private void OnServerFailedToRespond()
    {
        Debug.Log("OnServerFailedToRespond");
    }
    private void GetLatency()
    {
      // if(IsLocalPlayer)
        pingText.text = SteamMatchmakingServers.PingServer((uint)current_lobbyID, (ushort)current_lobbyID, pingResponse).ToString();
    }*/
}
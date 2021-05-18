using MLAPI;
using Steamworks;
using UnityEngine;

public class SteamLobby : MonoBehaviour
{
    private const string HostAdressKey = "HostAdress";

    private NetworkManager networkManager;
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        networkManager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby),
            HostAdressKey,
            SteamUser.GetSteamID().ToString());
    }
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (networkManager.IsServer) { return; }

        string hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            HostAdressKey);

        //TODO ..hostAddress == ip ul cum plm fac :|
        networkManager.StartClient();

    }


}
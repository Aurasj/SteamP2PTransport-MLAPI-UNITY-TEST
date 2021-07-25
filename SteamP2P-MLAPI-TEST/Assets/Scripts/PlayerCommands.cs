using MLAPI;
using UnityEngine;

public class PlayerCommands : NetworkBehaviour
{
    private ServersList serverList;

    private void Awake()
    {
        serverList = FindObjectOfType<ServersList>();
    }
    private void Update()
    {
        if (IsLocalPlayer)
        {
            if (Input.GetKey(KeyCode.Tab))
            {
                serverList.inLobby.SetActive(true);
            }
            else
            {
                if(serverList)
                serverList.inLobby.SetActive(false);
            }
        }
    }
}

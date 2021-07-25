using UnityEngine;
using MLAPI;

public class Test : MonoBehaviour
{
    private void OnGUI()
    {
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
    }



}

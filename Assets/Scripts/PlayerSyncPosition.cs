using UnityEngine;
using TMPro;
using MLAPI;

public class Player : MonoBehaviour
{
    private NetworkManager networkManager;

    private void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();
    }


}

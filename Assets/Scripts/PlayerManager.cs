using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class PlayerManager : MonoBehaviour
{
    PhotonView PV;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    void Start()
    {
        // ensuring that the user can control the player
        if(PV.IsMine)
        {
            CreateController();
        }
    }

    void CreateController()
    {
        // Instatiating the player
        PhotonNetwork.Instantiate("PlayerController", Vector3.zero, Quaternion.identity);
    }
}

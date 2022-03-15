using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text text;
    Player player;
    public void SetUp(Player _player)
    {
        player = _player;
        text.text = _player.NickName;
    }

    // Destroying the Player name Object when a player leaves the room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if(player == otherPlayer)
        {
            Destroy(gameObject);
        }
    }

    // Waht to do when the user leaves the room
    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }
}

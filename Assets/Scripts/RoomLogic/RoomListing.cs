using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using TMPro;

namespace com.louis.shootball
{
    public class RoomListing : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text roomNameText;
        [SerializeField]
        private TMP_Text playerText;
        private string roomName;

        public void SetRoomIngo(RoomInfo roomInfo)
        {
            roomNameText.text = roomInfo.Name;
            playerText.text = roomInfo.PlayerCount + " / " + roomInfo.MaxPlayers;
            roomName = roomInfo.Name;
        }

        public void OnJoinButtonClicked()
        {
            if (!PhotonNetwork.IsConnected)
            {
                return;
            }
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }
            PhotonNetwork.JoinRoom(roomName);
        }


    }
}
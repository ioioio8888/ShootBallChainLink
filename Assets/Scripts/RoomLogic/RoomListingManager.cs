using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

namespace com.louis.shootball
{
    public class RoomListingManager : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        private RoomListing roomListingPrefab;
        [SerializeField]
        private Transform roomListRoot;
        string roomName;
        public bool IsPrivate = false;


        #region PublicMethods

        public void OnChangeRoomName(string _name) {
            roomName = _name;
        }

        public void OnChangePrivateRoom(bool state)
        {
            IsPrivate = state;
        }
        public void OnQuickMatchButtonClicked()
        {
            if (PhotonNetwork.IsConnected)
            {
                RoomOptions options = GetRoomOptions();
                PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: options);
            }
        }

        public void OnCreateRoomButtonClicked()
        {
            if (!PhotonNetwork.IsConnected || roomName == "")
            {
                return;
            }
            RoomOptions options = GetRoomOptions();
            if (IsPrivate)
            {
                options.IsVisible = false;
            }
            else
            {
                options.IsVisible = true;
            }
            PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
        }

        public void OnReconnectButtonClicked() {
            if (!PlayerPrefs.HasKey("LastRoom"))
            {
                LauncherUIManager.Instance.HideLoadingPanel();
                LauncherUIManager.Instance.ShowErrorPanel("Reconnect Failed");
                return;
            }
            else {
                string lastRoom = PlayerPrefs.GetString("LastRoom");
                PhotonNetwork.RejoinRoom(lastRoom);
            }

        }

        #endregion

        #region PrivateMethods
        private RoomOptions GetRoomOptions()
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = GameConfig.maxPlayersPerRoom;
            options.CleanupCacheOnLeave = false;
            return options;
        }
        #endregion

        #region PUNCallBacks
        public override void OnCreatedRoom()
        {
            Debug.Log("Created Room successfully.", this);
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.Log("Created Room failed. " + message, this);
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            foreach (Transform child in roomListRoot)
            {
                Destroy(child.gameObject);
            }

            foreach (RoomInfo info in roomList)
            {
                if (info.MaxPlayers > 0)
                {
                    RoomListing listing = Instantiate(roomListingPrefab, roomListRoot);
                    if (listing != null)
                    {
                        listing.SetRoomIngo(info);
                    }
                }
            }
        }
        #endregion
    }
}
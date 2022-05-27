using ExitGames.Client.Photon;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

namespace com.louis.shootball
{
    public class CustomMatchManager : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        private RoomListing roomListingPrefab;
        [SerializeField]
        private Transform roomListRoot;
        string roomName;
        bool IsPrivate = false;
        #region PublicMethods

        public void OnChangeRoomName(string _name)
        {
            roomName = _name;
        }

        public void OnChangePrivateRoom(bool state)
        {
            IsPrivate = state;
        }

        public void OnCreateRoomButtonClicked()
        {
            if (!PhotonNetwork.IsConnected || roomName == "")
            {
                return;
            }
            RoomOptions options = GetRoomOptions(GameMode.ThreeVSThree);
            Hashtable expectedCustomRoomProperties = new Hashtable();
            expectedCustomRoomProperties.Add(GameConfig.isQuickModeKey, false);
            expectedCustomRoomProperties.Add(GameConfig.GameModeKey, GameMode.ThreeVSThree);
            options.CustomRoomProperties = expectedCustomRoomProperties;
            options.CustomRoomPropertiesForLobby = new string[] { GameConfig.isQuickModeKey, GameConfig.GameModeKey };
            if (IsPrivate)
            {
                options.IsVisible = false;
            }
            else
            {
                options.IsVisible = true;
            }
            PhotonNetwork.CreateRoom(roomName:roomName, roomOptions: options, TypedLobby.Default);
        }

        public void OnJoinRoomButtonClicked()
        {
            if (!PhotonNetwork.IsConnected || roomName == "")
            {
                return;
            }
            RoomOptions options = GetRoomOptions(GameMode.ThreeVSThree);
            Hashtable expectedCustomRoomProperties = new Hashtable();
            expectedCustomRoomProperties.Add(GameConfig.isQuickModeKey, false);
            expectedCustomRoomProperties.Add(GameConfig.GameModeKey, GameMode.ThreeVSThree);
            options.CustomRoomProperties = expectedCustomRoomProperties;
            if (IsPrivate)
            {
                options.IsVisible = false;
            }
            else
            {
                options.IsVisible = true;
            }
            PhotonNetwork.JoinRoom(roomName: roomName);
        }
        #endregion

        #region Private Methods
        private RoomOptions GetRoomOptions(GameMode mode)
        {
            RoomOptions options = new RoomOptions();
            switch (mode)
            {
                case GameMode.ThreeVSThree:
                    options.MaxPlayers = GameConfig.ThreeVsThreeMode;
                    break;
                case GameMode.TwoVSTwo:
                    options.MaxPlayers = GameConfig.TwoVsTwoMode;
                    break;
                case GameMode.OneVSOne:
                    options.MaxPlayers = GameConfig.OneVsOneMode;
                    break;
                default:
                    options.MaxPlayers = GameConfig.ThreeVsThreeMode;
                    break;
            }
            options.CleanupCacheOnLeave = false;
            return options;
        }
        #endregion


        #region PunCallBacks
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
                    if (!(bool)info.CustomProperties[GameConfig.isQuickModeKey])
                    {
                        RoomListing listing = Instantiate(roomListingPrefab, roomListRoot);
                        if (listing != null)
                        {
                            listing.SetRoomIngo(info);
                        }
                    }
                }
            }
        }
        public override void OnJoinedRoom()
        {
            if ((bool)PhotonNetwork.CurrentRoom.CustomProperties[GameConfig.isQuickModeKey] == false)
            {
                // #Critical: We only load if we are the master player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
                if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        // #Critical
                        PhotonNetwork.LoadLevel("Room");
                    }
                }
            }
        }
        #endregion
    }
}
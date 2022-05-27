using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace com.louis.shootball
{
    public enum GameMode { 
        ThreeVSThree,
        TwoVSTwo,
        OneVSOne
    }

    public class QuickMatchManager : MonoBehaviourPunCallbacks
    {
        //lobby options if options[0] = 1 is quick match
        public GameObject QuickMatchButton;
        public GameObject CancelButton;
        public GameObject CancellingLabel;
        public bool joinedRoom = false;

        #region publicMethod
        public void OnQuickMatchButtonClicked()
        {
            if (PhotonNetwork.IsConnected)
            {
                Hashtable expectedCustomRoomProperties = new Hashtable();
                expectedCustomRoomProperties.Add(GameConfig.isQuickModeKey, true);
                expectedCustomRoomProperties.Add(GameConfig.GameModeKey, GameMode.OneVSOne);
                QuickMatchButton.SetActive(false);
                CancelButton.SetActive(true);
                CancellingLabel.SetActive(false);
                RoomOptions options = GetRoomOptions(GameMode.OneVSOne);
                options.CustomRoomPropertiesForLobby = new string[] { GameConfig.isQuickModeKey, GameConfig.GameModeKey };
                options.CustomRoomProperties = expectedCustomRoomProperties;
                PhotonNetwork.JoinRandomOrCreateRoom(expectedCustomRoomProperties: expectedCustomRoomProperties,roomOptions: options);
            }
        }

        public void CancelQuickMatch() {
            if (PhotonNetwork.InRoom) {
                PhotonNetwork.LeaveRoom();
            }
        }
        #endregion
        #region PrivateMethods
        private RoomOptions GetRoomOptions(GameMode mode)
        {
            RoomOptions options = new RoomOptions();
            switch (mode) {
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

        #region PUNCallBack
        public override void OnJoinedRoom()
        {
            Debug.Log("JoinedRoom" + PhotonNetwork.CurrentRoom.Name);
            if ((bool)PhotonNetwork.CurrentRoom.CustomProperties[GameConfig.isQuickModeKey] == true)
            {
                // #Critical: We only load if we are the master player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
                if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        // #Critical
                        PhotonNetwork.LoadLevel("Room");
                    }
                }
            }
        }

        public override void OnLeftRoom()
        {
            QuickMatchButton.SetActive(true);
            CancelButton.SetActive(false);
            CancellingLabel.SetActive(false);
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log("Player Joined" + newPlayer.NickName);
            if ((bool)PhotonNetwork.CurrentRoom.CustomProperties[GameConfig.isQuickModeKey] == true)
            {
                // #Critical: We only load if we are the master player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
                if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
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

using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace com.louis.shootball
{
    public class LauncherManager : MonoBehaviourPunCallbacks
    {
        public static LauncherManager Instance;
        #region Private Serializable Fields
        #endregion

        #region Private Fields

        /// <summary>
        /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
        /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
        /// Typically this is used for the OnConnectedToMaster() callback.
        /// </summary>
        bool isConnecting;
        string regionCode;

        #endregion
        #region MonoBehaviour CallBacks


        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        private void Start()
        {
            GetSavedRegion();
        }

        #endregion

        #region Public Methods
        //public void Reconnect() {
        //    PhotonNetwork.ReconnectAndRejoin();
        //} 
        //public void OnConfirmName()
        //{
        //    PhotonNetwork.GameVersion = GameConfig.gameVersion;
        //    if (!isConnecting)
        //    {
        //        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = regionCode;
        //        isConnecting = PhotonNetwork.ConnectUsingSettings();
        //    }
        //    LauncherUIManager.Instance.HideInitPanel();
        //    LauncherUIManager.Instance.ShowLoadingPanel();
        //}

        //public void OnClearGUID() {
        //    if (PlayerPrefs.HasKey("GUID"))
        //    {
        //        PlayerPrefs.DeleteKey("GUID");
        //    }
        //}

        //public void OnRegionListChange(int index)
        //{
        //    regionCode = GameConfig.regionsCode[index];
        //    PlayerPrefs.SetString("Region", regionCode);
        //}

        #endregion

        #region Private Methods
        private string GetGUID() {
            if (PlayerPrefs.HasKey("GUID"))
            {
                return PlayerPrefs.GetString("GUID");
            }
            else {
                string newGuid = System.Guid.NewGuid().ToString();
                PlayerPrefs.SetString("GUID", newGuid);
                return newGuid;
            }
        }


        private void GetSavedRegion() {
            if (PlayerPrefs.HasKey("Region"))
            {
                regionCode = PlayerPrefs.GetString("Region");
            }
            else {
                regionCode = "";
            }
        }
        #endregion



        #region MonoBehaviourPunCallbacks Callbacks


        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinLobby();
        }


        public override void OnDisconnected(DisconnectCause cause)
        {
            isConnecting = false;
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
            LauncherUIManager.Instance.HideLoadingPanel();
            //TODO: show error message
            LauncherUIManager.Instance.ShowErrorPanel(message);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            LauncherUIManager.Instance.HideLoadingPanel();
            //TODO: show error message
            LauncherUIManager.Instance.ShowErrorPanel(message);
        }

        public override void OnJoinedRoom()
        {
            // #Critical: We only load if we are the first player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                //Debug.Log("We load the 'Battleground' ");


                // #Critical
                // Load the Room Level.
                PhotonNetwork.LoadLevel("Room");
            }
        }

        #endregion

    }
}

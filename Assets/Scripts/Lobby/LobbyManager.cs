using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using TMPro;

namespace com.louis.shootball
{
    public class LobbyManager : MonoBehaviourPunCallbacks
    {
        public Slider LoadingProgress;
        public TMP_Text LoadingText; 
        public GameObject LoadingPanel;

        public void Start()
        {
            Cursor.lockState = CursorLockMode.None; 
        }

        public void Update()
        {
            if (PhotonNetwork.LevelLoadingProgress > 0 && PhotonNetwork.LevelLoadingProgress != 1)
            {
                if (!LoadingPanel.activeSelf)
                {
                    LoadingPanel.SetActive(true);
                }
                LoadingText.text = (PhotonNetwork.LevelLoadingProgress * 100).ToString("0");
                LoadingProgress.value = PhotonNetwork.LevelLoadingProgress;
            }
            else {
                if (LoadingPanel.activeSelf)
                {
                    LoadingPanel.SetActive(false);
                }
            }
        }

        #region PUNCallBacks
        public override void OnDisconnected(DisconnectCause cause)
        {
            SceneManager.LoadScene("Init");
        }
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.Log("Created Room failed. " + message, this);
            ErrorPanelManager.Instance.OnErrorMesssage(message);
        }
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log("Join Room failed. " + message, this);
            ErrorPanelManager.Instance.OnErrorMesssage(message);
        }
        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinLobby();
        }
        #endregion
    }
}
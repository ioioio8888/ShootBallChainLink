using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

namespace com.louis.shootball
{
    public class RoomUIManager : MonoBehaviour
    {
        public static RoomUIManager Instance;
        public Transform RedRoot;
        public Transform BlueRoot;
        public GameObject StartGameButton;
        public GameObject BackButton;
        public GameObject GameStartingText;
        public RoomManager roomManager;
        public Slider LoadingProgress;
        public TMP_Text LoadingText;
        public GameObject LoadingPanel;
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

        public void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            if ((bool)PhotonNetwork.CurrentRoom.CustomProperties[GameConfig.isQuickModeKey] == false)
            {
                BackButton.SetActive(true);
            }
            else {
                BackButton.SetActive(false);
            }
        }
        private void Update()
        {
            if (roomManager.startingGame)
            {
                BackButton.SetActive(false);
                StartGameButton.SetActive(false);
                GameStartingText.SetActive(true);
            }
            else
            {
                GameStartingText.SetActive(false);
                if (PhotonNetwork.IsMasterClient && CheckStartable())
                {
                    StartGameButton.SetActive(true);
                }
                else
                {
                    StartGameButton.SetActive(false);
                }
            }
            if (PhotonNetwork.LevelLoadingProgress > 0 && PhotonNetwork.LevelLoadingProgress != 1)
            {
                if (!LoadingPanel.activeSelf)
                {
                    LoadingPanel.SetActive(true);
                }
                LoadingText.text = (PhotonNetwork.LevelLoadingProgress * 100).ToString("0");
                LoadingProgress.value = PhotonNetwork.LevelLoadingProgress;
            }
            else
            {
                if (LoadingPanel.activeSelf)
                {
                    LoadingPanel.SetActive(false);
                }
            }
        }

        private bool CheckStartable()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount <= 1)
            {
                return false;
            }
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player.CustomProperties["Champion"] == null)
                {
                    return false;
                }

                ////below code is to check if the player is paid , it is temp solution
                //if (player.CustomProperties.ContainsKey("EnterancePaid"))
                //{
                //    if ((bool)player.CustomProperties["EnterancePaid"] == false)
                //    {
                //        return false;
                //    }
                //}
                //else 
                //{
                //    return false;
                //}
            }
            //TODO: think a checking method to check if it is startable
            //no checking for now
            return true;
        }

    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace com.louis.shootball
{
    public class ConnectionManager : MonoBehaviour
    {
        public GameObject disconnectPanel;

        public void Update()
        {
            if (!PhotonNetwork.IsConnected)
            {
                if (!disconnectPanel.activeInHierarchy)
                {
                    Cursor.lockState = CursorLockMode.None;
                    disconnectPanel.SetActive(true);
                }
            }
            else {
                if (disconnectPanel.activeInHierarchy) 
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    disconnectPanel.SetActive(false);
                }
            }
        }


        public void Disconnect() {
            PhotonNetwork.Disconnect();
        }

        public void Reconnect() {
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ReconnectAndRejoin();
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        }
    }
}
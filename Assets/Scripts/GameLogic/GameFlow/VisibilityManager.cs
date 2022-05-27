using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Photon.Pun;

namespace com.louis.shootball
{
    //this class is for photon webgl to deal with background app message rate problem
    public class VisibilityManager : MonoBehaviourPunCallbacks
    {

#if UNITY_WEBGL
        //called when webgl tab become hidden
        public void OnHidden()
        {
            if (SceneManager.GetActiveScene().name == "Football")
            {
                if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
                {
                    if (FootballGameManager.Instance.gameState == GameState.STARTED)
                    {
                        //disconnect if it is master
                        if (PhotonNetwork.IsMasterClient)
                        {
                            PassMaster();
                        }
                    }
                }
            }
            else
            {
                //Disconnect();
            }
        }

        //called when webgl tab become visible
        public void OnVisible()
        {
            Reconnect();
        }
#endif
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                if (SceneManager.GetActiveScene().name == "Football")
                {
                    if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
                    {
                        if (FootballGameManager.Instance.gameState == GameState.STARTED)
                        {
                            //disconnect if it is master
                            if (PhotonNetwork.IsMasterClient)
                            {
                                PassMaster();
                            }
                        }
                    }
                }
            }
            else
            {
                // app is foreground again
                Reconnect();
            }
        }

        private void PassMaster() {
            if (PhotonNetwork.InRoom)
            {
                foreach (var player in PhotonNetwork.PlayerList)
                {
                    if (!player.IsInactive)
                    {
                        PhotonNetwork.SetMasterClient(player);
                        break;
                    }
                }
            }
        }

        private void Disconnect() {
             PhotonNetwork.Disconnect();
        }

        private void Reconnect() {
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Photon.Pun;
using Photon.Realtime;

namespace com.louis.shootball
{
    public class ReconnectManager : MonoBehaviour
    {
        string userCurrentRoom = "";
        public GameObject reconnectButton;

        // Start is called before the first frame update
        void Start()
        {
            PlayFabGetUserDataCall(0);
        }

        void PlayFabGetUserDataCall(int retries)
        {
            List<string> keysToGet = new List<string>();
            keysToGet.Add("CurrentRoom");
            if (retries < 10)
            {
                PlayFabClientAPI.GetUserData(new GetUserDataRequest()
                {
                    Keys = keysToGet
                },
                result =>
                {
                    userCurrentRoom = result.Data["CurrentRoom"].Value;
                    Debug.Log("Successfully updated user data");
                    reconnectButton.SetActive(true);
                },
                error =>
                {
                    Debug.Log("Got error setting user data");
                    Debug.Log(error.GenerateErrorReport());
                    retries++;
                    PlayFabGetUserDataCall(retries);
                });
            }
        }

        public void OnReconnectButtonClicked()
        {
            if (userCurrentRoom != "")
            {
                PhotonNetwork.RejoinRoom(userCurrentRoom);
                return;
            }
        }
    }

}

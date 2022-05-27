using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Pun.UtilityScripts;

namespace com.louis.shootball
{
    public class ClearRoomProperties : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            ClearSelectedTeam();
            ClearSelectedChampion();
        }

        private void ClearSelectedChampion()
        {
            Hashtable ht = new Hashtable();
            ht.Add("Champion", null);
            PhotonNetwork.LocalPlayer.SetCustomProperties(ht);
        }

        private void ClearSelectedTeam() {
            PhotonNetwork.LocalPlayer.LeaveCurrentTeam();
        }
    }
}
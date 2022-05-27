using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

namespace com.louis.shootball
{
    public class PlayerStatusUIManager : MonoBehaviour
    {
        #region Private Constants
        [SerializeField]
        private TMP_Text _playerNameText;
        [SerializeField]
        private TMP_Text _regionText;
        #endregion


        #region MonoCallback
        // Start is called before the first frame update
        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            UpdatePlayerNameText();
            UpdateRegionText();
        }

        #endregion

        #region privateMethod
        private void UpdatePlayerNameText()
        {
            if (_playerNameText != null)
            {
                _playerNameText.text = PhotonNetwork.LocalPlayer.NickName;
            }
        }
        private void UpdateRegionText()
        {
            if (_regionText != null)
            {
                _regionText.text = "region: " + PhotonNetwork.CloudRegion;
            }
        }
        #endregion

    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using Photon.Realtime;

using System.Collections.Generic;
using System;


namespace com.louis.shootball
{
    /// <summary>
    /// Player name input field. Let the user input his name, will appear above the player in the game.
    /// </summary>
    public class LauncherUIManager : MonoBehaviourPunCallbacks
    {
        public static LauncherUIManager Instance;
        #region Private Serializable Fields
        [Tooltip("The Ui Panel to let the user enter name, connect and play")]
        [SerializeField]
        private GameObject InitPanel;
        [SerializeField]
        private GameObject controlPanel;
        [Tooltip("The UI Label to inform the user that the connection is in progress")]
        [SerializeField]
        private GameObject loadingPanel;
        [SerializeField]
        private GameObject errorPanel;
        [SerializeField]
        private TMP_Text errorText;
        #endregion

        #region Private Constants
        [SerializeField]
        private TMP_InputField _inputField;
        [SerializeField]
        private TMP_Text _playerNameText;
        [SerializeField]
        private TMP_Text _regionText;
        [SerializeField]
        private TMP_Dropdown _regionDropDown;
        // Store the PlayerPref Key to avoid typos
        const string playerNamePrefKey = "PlayerName";


        #endregion


        #region MonoBehaviour CallBacks

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
        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start()
        {
            CheckLoggedIn();
            Cursor.lockState = CursorLockMode.None;
            UpdatePlayerNameText();
            UpdateRegionList();
        }


        private void Update()
        {
            _regionText.text = "region: " + PhotonNetwork.CloudRegion;
            if (PhotonNetwork.LocalPlayer != null)
            {
                _playerNameText.text = PhotonNetwork.LocalPlayer.NickName;
            }
        }
        #endregion

        #region PrivateMethod
        private void CheckLoggedIn() {
            if (PhotonNetwork.IsConnected)
            {
                InitPanel.SetActive(false);
                controlPanel.SetActive(true);
            }
        }

        private void UpdatePlayerNameText() {
            if (_inputField != null)
            {
                if (PlayerPrefs.HasKey(playerNamePrefKey))
                {
                    _inputField.text = PlayerPrefs.GetString(playerNamePrefKey);
                }
            }
        }

        void UpdateRegionList()
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            for (int i = 0; i < GameConfig.regionsName.Length; i++)
            {
                TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
                option.text = GameConfig.regionsName[i];
                options.Add(option);
            }
            _regionDropDown.AddOptions(options);
            if (PlayerPrefs.HasKey("Region"))
            {
                _regionDropDown.value = Array.IndexOf(GameConfig.regionsCode, PlayerPrefs.GetString("Region"));
            }
        }

        #endregion


        #region Public Methods
        public void ShowLoadingPanel()
        {
            loadingPanel.SetActive(true);
        }
        public void HideLoadingPanel()
        {
            loadingPanel.SetActive(false);
        }
        public void ShowControlPanel()
        {
            controlPanel.SetActive(true);
        }
        public void HideControlPanel()
        {
            controlPanel.SetActive(false);
        }
        public void ShowInitPanel()
        {
            InitPanel.SetActive(true);
        }
        public void HideInitPanel()
        {
            InitPanel.SetActive(false);
        }
        public void ShowErrorPanel(string message)
        {
            errorText.text = message;
            errorPanel.SetActive(true);
        }
        public void HideErrorPanel()
        {
            errorPanel.SetActive(false);
        }

        ///// <summary>
        ///// Sets the name of the player, and save it in the PlayerPrefs for future sessions.
        ///// </summary>
        ///// <param name="value">The name of the Player</param>
        //public void SetPlayerName(string value)
        //{
        //    // #Important
        //    if (string.IsNullOrEmpty(value))
        //    {
        //        Debug.LogError("Player Name is null or empty");
        //        return;
        //    }
        //    PhotonNetwork.NickName = value; 
        //    PlayerPrefs.SetString(playerNamePrefKey, value);
        //}


        #endregion

        #region PUNCallBacks
        public override void OnDisconnected(DisconnectCause cause)
        {
            HideControlPanel();
            HideLoadingPanel();
            ShowInitPanel();
        }

        public override void OnConnectedToMaster()
        {
            HideLoadingPanel();
            ShowControlPanel();
            HideInitPanel();
        }
        #endregion

    }
}
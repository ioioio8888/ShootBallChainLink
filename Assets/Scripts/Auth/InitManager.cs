using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Facebook.Unity;
using TMPro;
using Michsky.UI.ModernUIPack;
using System;
using MoralisWeb3ApiSdk;
using Moralis.WebGL;
using Moralis.WebGL.Platform.Objects;
using ExitGames.Client.Photon;

namespace com.louis.shootball
{
    public class InitManager : MonoBehaviourPunCallbacks
    {
        public TMP_Text loadingText;
        public Slider loadingSlider;
        public GameObject loadingPanel;
        public GameObject launcherPanel;
        public GameObject displayNamePanel;
        public string displayName;
        private string _playFabPlayerIdCache;
        [SerializeField]
        private string launcherSceneName;
        public bool testing = false;
        public string testId = "";
        public CustomDropdown regionList; // Your dropdown variable
        int maxCheckPoint = 5;
        int checkPoint = 0;

        // Reference to our Authentication service
        private PlayFabAuthService _AuthService = PlayFabAuthService.Instance;

        // Settings for what data to get from playfab on login.
        public GetPlayerCombinedInfoRequestParams InfoRequestParams;


        public void Start()
        {
            PhotonNetwork.GameVersion = GameConfig.gameVersion;
            UpdateRegionList();
            loadingText.text = "Logging In...";
            _AuthService.InfoRequestParams = InfoRequestParams;
            _AuthService.RememberMe = true;
        }


        public override void OnDisable()
        {
            base.OnDisable();
            PlayFabAuthService.OnDisplayAuthentication -= OnDisplayAuthentication;
            PlayFabAuthService.OnLoginSuccess -= OnLoginSuccess;
            PlayFabAuthService.OnPlayFabError -= OnPlayFabError;
            PlayFabAuthService.OnPhotonAuthentication -= OnPhotonAuthentication;
            PlayFabAuthService.OnFacebookLoginError -= OnFacebookLoginFailed;
            MoralisManager.OnWeb3Error -= OnWeb3LoginFailed;
            MoralisManager.OnMoralisLoginError -= OnMoralisLoginFailed;
            MoralisManager.OnStartSigning -= OnStartSigning;
            MoralisManager.OnSigningEnd -= OnEndSigning;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            PlayFabAuthService.OnDisplayAuthentication += OnDisplayAuthentication;
            PlayFabAuthService.OnLoginSuccess += OnLoginSuccess;
            PlayFabAuthService.OnPlayFabError += OnPlayFabError;
            PlayFabAuthService.OnPhotonAuthentication += OnPhotonAuthentication;
            PlayFabAuthService.OnFacebookLoginError += OnFacebookLoginFailed;
            MoralisManager.OnWeb3Error += OnWeb3LoginFailed;
            MoralisManager.OnMoralisLoginError += OnMoralisLoginFailed; 
            MoralisManager.OnStartSigning += OnStartSigning;
            MoralisManager.OnSigningEnd += OnEndSigning;
        }

        public void Update()
        {
            loadingSlider.value = ((float)checkPoint / (float)maxCheckPoint) * 100;
        }

        public void ClearPlayerPrefs()
        {
            _AuthService.UnlinkSilentAuth();
            _AuthService.ClearRememberMe();
            _AuthService.AuthType = Authtypes.None;
        }

        private void UpdateRegionList() {
            for (int i = 0; i < GameConfig.regionsName.Length; i++)
            {
                regionList.CreateNewItemFast(GameConfig.regionsName[i],null);
            }
            regionList.SetupDropdown();
            if (PlayerPrefs.HasKey("Region"))
            {
                int index = Array.IndexOf(GameConfig.regionsCode, PlayerPrefs.GetString("Region"));
                regionList.ChangeDropdownInfo(index);
            }
            else {
                PlayerPrefs.SetString("Region", "");
                regionList.ChangeDropdownInfo(0);
            }
        }
        public void OnRegionListChange(int index)
        {
            string regionCode = GameConfig.regionsCode[index];
            PlayerPrefs.SetString("Region", regionCode);
        }

        /// <summary>
        /// Choose to display the Auth UI or any other action.
        /// </summary>
        private void OnDisplayAuthentication()
        {
            checkPoint++;
            //Here we have choses what to do when AuthType is None.
            loadingText.text = "Authenticating";
        }

        /// <summary>
        /// Login Successfully - Goes to next screen.
        /// </summary>
        /// <param name="result"></param>
        private void OnLoginSuccess(PlayFab.ClientModels.LoginResult result)
        {
            checkPoint++;
            Debug.LogFormat("Logged In as: {0}", result.PlayFabId);
            loadingText.text = "Getting Token from Server";
            _AuthService.RequestPhotonToken();
        }

        /// <summary>
        /// Choose to display the Auth UI or any other action.
        /// </summary>
        private void OnPhotonAuthentication()
        {
            checkPoint++;
            //Here we have choses what to do when AuthType is None.
            loadingText.text = "Getting Player Info!";
            OnLoginSuccess();
        }

        /// <summary>
        /// Error handling for when Login returns errors.
        /// </summary>
        /// <param name="error"></param>
        private void OnPlayFabError(PlayFabError error)
        {        
            //There are more cases which can be caught, below are some
            //of the basic ones.
            //switch (error.Error)
            //{
            //    case PlayFabErrorCode.InvalidEmailAddress:
            //    case PlayFabErrorCode.InvalidPassword:
            //    case PlayFabErrorCode.InvalidEmailOrPassword:
            //        break;
            //    case PlayFabErrorCode.AccountNotFound:
            //        return;
            //    default:
            //        ErrorPanelManager.Instance.OnErrorMesssage(error.GenerateErrorReport());
            //        break;
            //}
            ErrorPanelManager.Instance.OnErrorMesssage(error.GenerateErrorReport());
            LogMessage(error.GenerateErrorReport());
            loadingPanel.SetActive(false);
            launcherPanel.SetActive(true);
        }

        public void LogMessage(string message)
        {
            Debug.Log("PlayFab: " + message);
        }

        public async void OnConnectWalletButtonPressed()
        {
            launcherPanel.SetActive(false);
            loadingPanel.SetActive(true);
            bool success = await MoralisManager.instance.LoginWithWeb3();
            if (success)
            {
                MoralisUser user = await MoralisInterface.GetUserAsync();
                string address = user.ethAddress;
                _AuthService.AuthType = Authtypes.Silent;
                _AuthService.SilentlyAuthenticateWeb3(address);
                Hashtable ht = new Hashtable();
                ht.Add("address", address);
                PhotonNetwork.LocalPlayer.SetCustomProperties(ht);
            }
            else {
                loadingPanel.SetActive(false);
                launcherPanel.SetActive(true);
            }
        }


        private void OnFacebookLoginFailed(ILoginResult result) {
            ErrorPanelManager.Instance.OnErrorMesssage("Facebook Auth Failed: " + result.Error + "\n" + result.RawResult);
            loadingPanel.SetActive(false);
            launcherPanel.SetActive(true);
        }
        private void OnWeb3LoginFailed(string message)
        {
            ErrorPanelManager.Instance.OnErrorMesssage("Web3 Login Failed");
            loadingPanel.SetActive(false);
            launcherPanel.SetActive(true);
        }

        private void OnMoralisLoginFailed(string message)
        {
            ErrorPanelManager.Instance.OnErrorMesssage("Moralis Login Failed");
            loadingPanel.SetActive(false);
            launcherPanel.SetActive(true);
        }

        private void OnStartSigning()
        {

        }

        private void OnEndSigning()
        {

        }


        public void OnPlayButtonClicked()
        {
            launcherPanel.SetActive(false);
            loadingPanel.SetActive(true);
            if (testing)
            {
                _AuthService.AuthType = Authtypes.Silent;
                _AuthService.SilentlyAuthenticate(id: testId);
            }
            else
            {
                if (_AuthService.AuthType == Authtypes.None)
                {
                    //not logged In previously
                    OnGuestButtonClicked();
                }
                else
                {
                    //logged In previously, start auto login
                    _AuthService.AutoLogin();
                }
            }
        }

        public void OnFaceBookButtonClicked() {
            _AuthService.Authenticate(Authtypes.Facebook);
        }

        public void OnGuestButtonClicked()
        {
            _AuthService.Authenticate(authType: Authtypes.Silent);
        }

        private void OnLoginSuccess()
        {
            checkPoint++;
            loadingText.text = "Getting Player Info";
            _AuthService.GetPlayerInfo(OnGetPlayerInfo);
            //SceneManager.LoadScene(launcherSceneName);
        }

        private void OnGetPlayerInfo(GetPlayerProfileResult result) {
            if (result.PlayerProfile.DisplayName == null)
            {
                //this is a new account, we have to set an username for this account
                displayNamePanel.SetActive(true);
            }
            else {
                //this an old account, switch scene
                PhotonNetwork.NickName = result.PlayerProfile.DisplayName;
                ConnectToPhoton();
            }
        }

        public void OnDisplayNameChange(string name) {
            displayName = name;
        }

        public void OnDisplayNameConfirm()
        {
            if (displayName.Length > 3 && displayName.Length < 25)
            {
                _AuthService.SetPlayerDisplayName(displayName, OnSetPlayerDisplayName);
            }
        }

        private void OnSetPlayerDisplayName(UpdateUserTitleDisplayNameResult result)
        {

            PhotonNetwork.NickName = displayName;
            Debug.Log(PhotonNetwork.NickName);
            //Display Name is Set
            ConnectToPhoton();
        }

        private void ConnectToPhoton()
        {
            checkPoint++;
            loadingText.text = "Ready...";
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = PlayerPrefs.GetString("Region", "");
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinLobby();
            SceneManager.LoadScene(launcherSceneName);
        }
    }
}
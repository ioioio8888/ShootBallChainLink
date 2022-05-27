using System.Collections.Generic;
using UnityEngine;
using MoralisWeb3ApiSdk;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Unity;
using Nethereum.Web3;
using WalletConnectSharp.Core;
using WalletConnectSharp.NEthereum;
#if UNITY_WEBGL
using Moralis.WebGL;
using Moralis.WebGL.Platform.Objects;
#else
using Moralis;
using Moralis.Platform.Objects;
#endif
using Cysharp.Threading.Tasks;

public class MoralisManager : MonoBehaviour
{
    public enum LoginType
    {
        WalletConnect,
        Web3
    }

    public LoginType loginType = LoginType.Web3;
    public MoralisController moralisCtrl;
    public delegate void Web3ErrorEvent(string message);
    public static event Web3ErrorEvent OnWeb3Error;
    public delegate void StartSigningEvent();
    public static event StartSigningEvent OnStartSigning;
    public delegate void SigningEndEvent();
    public static event SigningEndEvent OnSigningEnd;
    public delegate void MoralisLoginErrorEvent(string message);
    public static event MoralisLoginErrorEvent OnMoralisLoginError;
    public static MoralisManager instance { get; protected set; }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }


    public async void Start()
    {
        if (moralisCtrl != null)
        {
            await moralisCtrl.Initialize();
        }
        else
        {
            Debug.LogError("no Ctrl");
        }
    }

#if UNITY_WEBGL
    public async UniTask<bool> LoginWithWeb3()
    {
        string userAddr = "";
        userAddr = await MoralisInterface.SetupWeb3();

        if (string.IsNullOrWhiteSpace(userAddr))
        {
            Debug.LogError("Could not login or fetch account from web3.");
            return false;
        }
        else
        {
            string address = Web3GL.Account().ToLower();
            string appId = MoralisInterface.GetClient().ApplicationId;
            long serverTime = 0;

            // Retrieve server time from Moralis Server for message signature
            Dictionary<string, object> serverTimeResponse = await MoralisInterface.GetClient().Cloud.RunAsync<Dictionary<string, object>>("getServerTime", new Dictionary<string, object>());

            if (serverTimeResponse == null || !serverTimeResponse.ContainsKey("dateTime") ||
                !long.TryParse(serverTimeResponse["dateTime"].ToString(), out serverTime))
            {
                Debug.Log("Failed to retrieve server time from Moralis Server!");
            }

            string signMessage = $"ShootBall Authentication\n\nId: {appId}:{serverTime}";
            Debug.Log(signMessage);
            string signature = "";
            if (OnStartSigning != null)
            {
                OnStartSigning.Invoke();
            }
            try
            {
                signature = await Web3GL.Sign(signMessage);
            }
            catch
            {
                await LogOut();
                Debug.LogError("Sign Error");
                if (OnWeb3Error != null)
                {
                    OnWeb3Error.Invoke("Sign Error");
                }
                return false;
            }

            if (OnSigningEnd != null)
            {
                OnSigningEnd.Invoke();
            }
            Debug.Log($"Signature {signature} for {address} was returned.");

            // Create moralis auth data from message signing response.
            Dictionary<string, object> authData = new Dictionary<string, object> { { "id", address }, { "signature", signature }, { "data", signMessage } };

            Debug.Log("Logging in user.");


            // Attempt to login user.
            MoralisUser user = await MoralisInterface.LogInAsync(authData);

            if (user != null)
            {
                Debug.Log($"User {user.username} logged in successfully. ");
                await MoralisInterface.GetUserAsync();
                loginType = LoginType.Web3;
                return true;
            }
            else
            {
                if (OnMoralisLoginError != null)
                {
                    OnMoralisLoginError.Invoke("User login failed.");
                }
                Debug.Log("User login failed.");
                return false;
            }
        }
    }
#endif

    private async void InitializeWeb3()
    {
#if UNITY_WEBGL
        await MoralisInterface.SetupWeb3();
#else
        MoralisInterface.SetupWeb3();
#endif
    }

    public async UniTask LogOut()
    {
        // Logout the Moralis User.
        await MoralisInterface.LogOutAsync();
    }

    public async UniTask<string> GetUserAddress()
    {
        MoralisUser user = await MoralisInterface.GetUserAsync();
        if (user != null)
        {
            return user.ethAddress;
        }
        else
        {
            Debug.Log("not logged in");
            return null;
        }
    }

    public bool IsLoggedIn()
    {
        return MoralisInterface.IsLoggedIn();
    }

    public async UniTask<string> SignMessage(string message)
    {
        if (loginType == LoginType.Web3)
        {
            try
            {
                string signature = await Web3GL.Sign(message);
                return signature;
            }
            catch
            {
                return null;
            }
        }
        return null;
        //else
        //{
        //    string address = await GetUserAddress();
        //    try
        //    {
        //        string response = await walletConnect.Session.EthPersonalSign(address, message);
        //        return response;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}
    }
}

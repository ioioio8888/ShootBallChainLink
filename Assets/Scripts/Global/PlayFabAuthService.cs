//--------------------------------------------------------------------------------------
// PlayFabAuthService.cs
//
// Advanced Technology Group (ATG)
// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//--------------------------------------------------------------------------------------

using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using LoginResult = PlayFab.ClientModels.LoginResult;
using System;
using Photon.Pun;
using Photon.Realtime;
using Facebook.Unity;

/// <summary>
/// Supported Authentication types
/// See - https://api.playfab.com/documentation/client#Authentication
/// </summary>
public enum Authtypes
{
    None,
    Silent,
    Facebook,
    UsernameAndPassword,
    EmailAndPassword,
    RegisterPlayFabAccount
}

namespace com.louis.shootball
{
    public class PlayFabAuthService
    {
        // Events to subscribe to for this service
        public delegate void DisplayAuthenticationEvent();
        public static event DisplayAuthenticationEvent OnDisplayAuthentication;

        // Events to subscribe to for this service
        public delegate void GetPhotonTokenEvent();
        public static event GetPhotonTokenEvent OnGetPhotonToken;

        public delegate void PhotonAuthenticationEvent();
        public static event PhotonAuthenticationEvent OnPhotonAuthentication;

        public delegate void LoginSuccessEvent(LoginResult success);
        public static event LoginSuccessEvent OnLoginSuccess;

        public delegate void PlayFabErrorEvent(PlayFabError error);
        public static event PlayFabErrorEvent OnPlayFabError;

        public delegate void FacebookLoginErrorEvent(ILoginResult result);
        public static event FacebookLoginErrorEvent OnFacebookLoginError;
        // These are fields that we set when we are using the service.
        public string Email;
        public string Username;
        public string Password;
        public string AuthTicket;
        public GetPlayerCombinedInfoRequestParams InfoRequestParams;

        // This is a force link flag for custom ids for demoing
        public bool ForceLink = false;

        // Accessbility for PlayFab ID & Session Tickets
        public static string PlayFabId { get { return _playFabId; } }
        private static string _playFabId;

        public static string SessionTicket { get { return _sessionTicket; } }
        private static string _sessionTicket;

        private const string _LoginRememberKey = "PlayFabLoginRemember";
        private const string _PlayFabRememberMeIdKey = "PlayFabIdPassGuid";
        private const string _FacebookTokenKey = "FacebookTokenId";
        private const string _PlayFabAuthTypeKey = "PlayFabAuthType";

        public static PlayFabAuthService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PlayFabAuthService();
                }
                return _instance;
            }
        }

        private static PlayFabAuthService _instance;

        public PlayFabAuthService()
        {
            _instance = this;
        }

        /// <summary>
        /// Remember the user next time they log in
        /// This is used for Auto-Login purpose.
        /// </summary>
        public bool RememberMe
        {
            get
            {
                return PlayerPrefs.GetInt(_LoginRememberKey, 0) == 0 ? false : true;
            }
            set
            {
                PlayerPrefs.SetInt(_LoginRememberKey, value ? 1 : 0);
            }
        }

        /// <summary>
        /// Remember the type of authenticate for the user
        /// </summary>
        public Authtypes AuthType
        {
            get
            {
                return (Authtypes)PlayerPrefs.GetInt(_PlayFabAuthTypeKey, 0);
            }
            set
            {
                PlayerPrefs.SetInt(_PlayFabAuthTypeKey, (int)value);
            }
        }

        /// <summary>
        /// Generated Remember Me ID
        /// Pass Null for a value to have one auto-generated.
        /// </summary>
        private string RememberMeId
        {
            get
            {
                return PlayerPrefs.GetString(_PlayFabRememberMeIdKey, "");
            }
            set
            {
                var guid = value ?? Guid.NewGuid().ToString();
                PlayerPrefs.SetString(_PlayFabRememberMeIdKey, guid);
            }
        }

        public void ClearRememberMe()
        {
            PlayerPrefs.DeleteKey(_LoginRememberKey);
            PlayerPrefs.DeleteKey(_PlayFabRememberMeIdKey);
            PlayerPrefs.DeleteKey(_PlayFabAuthTypeKey);
        }
        /// <summary>
        /// Kick off the authentication process with Web3 moralis
        /// </summary>
        /// <param name="authType"></param>
        public void AuthenticateWithWeb3(string address)
        {
            AuthType = Authtypes.Silent;
            SilentlyAuthenticateWeb3(address);
        }


        public void SilentlyAuthenticateWeb3(string address, System.Action<LoginResult> callback = null)
        {
            string customId = address;
            PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                CustomId = customId,
                CreateAccount = true,
                InfoRequestParameters = InfoRequestParams
            }, (result) =>
            {
                //Store Identity and session
                _playFabId = result.PlayFabId;
                _sessionTicket = result.SessionTicket;
                //check if we want to get this callback directly or send to event subscribers.
                if (callback == null && OnLoginSuccess != null)
                {
                    //report login result back to the subscriber
                    OnLoginSuccess.Invoke(result);
                }
                else if (callback != null)
                {
                    //report login result back to the caller
                    callback.Invoke(result);
                }
            }, (error) =>
            {
                //report errro back to the subscriber
                if (callback == null && OnPlayFabError != null)
                {
                    OnPlayFabError.Invoke(error);
                }
                else
                {
                    //make sure the loop completes, callback with null
                    callback.Invoke(null);
                    //Output what went wrong to the console.
                    Debug.LogError(error.GenerateErrorReport());
                }

            });
        }




        /// <summary>
        /// Kick off the authentication process by specific authtype.
        /// </summary>
        /// <param name="authType"></param>
        public void Authenticate(Authtypes authType)
        {
            AuthType = authType;
            Authenticate();
        }

        /// <summary>
        /// Authenticate the user by the Auth Type that was defined.
        /// </summary>
        public void Authenticate()
        {
            switch (AuthType)
            {
                case Authtypes.None:
                    if (OnDisplayAuthentication != null)
                    {
                        OnDisplayAuthentication.Invoke();
                    }
                    break;

                case Authtypes.Silent:
                    SilentlyAuthenticate();
                    break;

                case Authtypes.EmailAndPassword:
                    AuthenticateEmailPassword();
                    break;

                case Authtypes.RegisterPlayFabAccount:
                    AddAccountAndPassword();
                    break;
                case Authtypes.Facebook:
                    AuthenticateFacebook();
                    break;
            }
        }

        /// <summary>
        /// OnFacebook Initialized
        /// </summary>
        private void OnFacebookInitialized()
        {
            FB.LogInWithReadPermissions(null, OnFacebookLoggedIn);
        }


        /// <summary>
        /// On Facebook logged in, start Playfab login process
        /// </summary>
        private void OnFacebookLoggedIn(ILoginResult result)
        {
            // If result has no errors, it means we have authenticated in Facebook successfully
            if (result == null || string.IsNullOrEmpty(result.Error))
            {
                Debug.Log("Facebook Auth Complete! Access Token: " + AccessToken.CurrentAccessToken.TokenString + "\nLogging into PlayFab...");

                /*
                 * We proceed with making a call to PlayFab API. We pass in current Facebook AccessToken and let it create
                 * and account using CreateAccount flag set to true. We also pass the callback for Success and Failure results
                 */
                PlayFabClientAPI.LoginWithFacebook(new LoginWithFacebookRequest()
                {
                    CreateAccount = true,
                    AccessToken = AccessToken.CurrentAccessToken.TokenString,
                },// Success
                    (LoginResult result) =>
                    {
                    //Store identity and session
                    _playFabId = result.PlayFabId;
                        _sessionTicket = result.SessionTicket;
                        if (RememberMe)
                        {
                            AuthType = Authtypes.Facebook;
                            PlayerPrefs.SetString(_FacebookTokenKey, AccessToken.CurrentAccessToken.TokenString);
                        }
                        if (OnLoginSuccess != null)
                        {
                        //report login result back to subscriber
                        OnLoginSuccess.Invoke(result);
                        }
                    },

                (PlayFabError error) =>
                {
                    if (OnPlayFabError != null)
                    {
                    //report error back to subscriber
                    OnPlayFabError.Invoke(error);
                    }
                });
            }
            else
            {
                // If Facebook authentication failed, we stop the cycle with the message
                if (OnFacebookLoginError != null)
                {
                    OnFacebookLoginError.Invoke(result);
                }
            }
        }


        /// <summary>
        /// Authenticate a user in PlayFab using Facebook
        /// </summary>
        private void AuthenticateFacebook()
        {
            if (!FB.IsInitialized)
            {
                FB.Init(OnFacebookInitialized);
            }
            else
            {
                FB.LogInWithReadPermissions(null, OnFacebookLoggedIn);
            }
        }

        /// <summary>
        /// Auto Login
        /// </summary>
        public void AutoLogin()
        {
            switch (AuthType)
            {
                case Authtypes.Silent:
                    AutoSilentlyLogin();
                    break;
                case Authtypes.Facebook:
                    //if (!FB.IsInitialized)
                    //{
                    //    FB.Init(OnFacebookInitialized);
                    //}
                    //AutoLoginWithFacebookID();
                    break;
            }
        }

        /// <summary>
        /// AutoLoginWithFacebookID
        /// </summary>
        private void AutoLoginWithFacebookID()
        {
            string token = PlayerPrefs.GetString(_FacebookTokenKey, "");
            if (token == "")
            {
                Debug.Log("FBLoginError");
                return;
            }

            PlayFabClientAPI.LoginWithFacebook(new LoginWithFacebookRequest()
            {
                AccessToken = token
            },// Success
                       (LoginResult result) =>
                       {
                       //Store identity and session
                       _playFabId = result.PlayFabId;
                           _sessionTicket = result.SessionTicket;
                           if (OnLoginSuccess != null)
                           {
                           //report login result back to subscriber
                           OnLoginSuccess.Invoke(result);
                           }
                       },

                   (PlayFabError error) =>
                   {
                       if (OnPlayFabError != null)
                       {
                       //report error back to subscriber
                       OnPlayFabError.Invoke(error);
                       }
                   });
        }

        private void AutoSilentlyLogin(System.Action<LoginResult> callback = null)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        //Login with the android device ID
        PlayFabClientAPI.LoginWithAndroidDeviceID(new LoginWithAndroidDeviceIDRequest() {
            TitleId = PlayFabSettings.TitleId,
            AndroidDevice = SystemInfo.deviceModel,
            OS = SystemInfo.operatingSystem,
            AndroidDeviceId = PlayFabSettings.DeviceUniqueIdentifier,
            InfoRequestParameters = InfoRequestParams
        }, (result) => {
            
            //Store Identity and session
            _playFabId = result.PlayFabId;
            _sessionTicket = result.SessionTicket;

            //check if we want to get this callback directly or send to event subscribers.
            if (callback == null && OnLoginSuccess != null)
            {
                //report login result back to the subscriber
                OnLoginSuccess.Invoke(result);
            }else if (callback != null)
            {
                //report login result back to the caller
                callback.Invoke(result);
            }
        }, (error) => {

            //report errro back to the subscriber
            if(callback == null && OnPlayFabError != null){
                OnPlayFabError.Invoke(error);
            }else{
                //make sure the loop completes, callback with null
                callback.Invoke(null);
                //Output what went wrong to the console.
                Debug.LogError(error.GenerateErrorReport());
            }
        });

#elif UNITY_IPHONE || UNITY_IOS && !UNITY_EDITOR
        PlayFabClientAPI.LoginWithIOSDeviceID(new LoginWithIOSDeviceIDRequest() {
            TitleId = PlayFabSettings.TitleId,
            DeviceModel = SystemInfo.deviceModel, 
            OS = SystemInfo.operatingSystem,
            DeviceId = PlayFabSettings.DeviceUniqueIdentifier,
            InfoRequestParameters = InfoRequestParams
        }, (result) => {
            //Store Identity and session
            _playFabId = result.PlayFabId;
            _sessionTicket = result.SessionTicket;

            //check if we want to get this callback directly or send to event subscribers.
            if (callback == null && OnLoginSuccess != null)
            {
                //report login result back to the subscriber
                OnLoginSuccess.Invoke(result);
            }else if (callback != null)
            {
                //report login result back to the caller
                callback.Invoke(result);
            }
        }, (error) => {
            //report errro back to the subscriber
            if(callback == null && OnPlayFabError != null){
                OnPlayFabError.Invoke(error);
            }else{
                //make sure the loop completes, callback with null
                callback.Invoke(null);
                //Output what went wrong to the console.
                Debug.LogError(error.GenerateErrorReport());
            }
        });
#elif UNITY_WEBGL
            PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                CustomId = GetGUID(),
                InfoRequestParameters = InfoRequestParams
            }, (result) =>
            {
                //Store Identity and session
                _playFabId = result.PlayFabId;
                _sessionTicket = result.SessionTicket;
                //check if we want to get this callback directly or send to event subscribers.
                if (callback == null && OnLoginSuccess != null)
                {
                    //report login result back to the subscriber
                    OnLoginSuccess.Invoke(result);
                }
                else if (callback != null)
                {
                    //report login result back to the caller
                    callback.Invoke(result);
                }
            }, (error) =>
            {

                Debug.Log(4);
                //report errro back to the subscriber
                if (callback == null && OnPlayFabError != null)
                {
                    OnPlayFabError.Invoke(error);
                }
                else
                {
                    //make sure the loop completes, callback with null
                    callback.Invoke(null);
                    //Output what went wrong to the console.
                    Debug.LogError(error.GenerateErrorReport());
                }

            });
#else
            PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                CustomId = PlayFabSettings.DeviceUniqueIdentifier,
                InfoRequestParameters = InfoRequestParams
            }, (result) =>
            {
                //Store Identity and session
                _playFabId = result.PlayFabId;
                _sessionTicket = result.SessionTicket;
                //check if we want to get this callback directly or send to event subscribers.
                if (callback == null && OnLoginSuccess != null)
                {
                    //report login result back to the subscriber
                    OnLoginSuccess.Invoke(result);
                }
                else if (callback != null)
                {
                    //report login result back to the caller
                    callback.Invoke(result);
                }
            }, (error) =>
            {

                Debug.Log(4);
                //report errro back to the subscriber
                if (callback == null && OnPlayFabError != null)
                {
                    OnPlayFabError.Invoke(error);
                }
                else
                {
                    //make sure the loop completes, callback with null
                    callback.Invoke(null);
                    //Output what went wrong to the console.
                    Debug.LogError(error.GenerateErrorReport());
                }

            });
#endif
        }


        /// <summary>
        /// Authenticate a user in PlayFab using an Email & Password combo
        /// </summary>
        private void AuthenticateEmailPassword()
        {
            //Check if the users has opted to be remembered.
            if (RememberMe && !string.IsNullOrEmpty(RememberMeId))
            {
                // If the user is being remembered, then log them in with a customid that was 
                // generated by the RememberMeId property
                PlayFabClientAPI.LoginWithCustomID(
                    new LoginWithCustomIDRequest()
                    {
                        TitleId = PlayFabSettings.TitleId,
                        CustomId = RememberMeId,
                        CreateAccount = true,
                        InfoRequestParameters = InfoRequestParams
                    },

                    // Success
                    (LoginResult result) =>
                    {
                    //Store identity and session
                    _playFabId = result.PlayFabId;
                        _sessionTicket = result.SessionTicket;

                        if (OnLoginSuccess != null)
                        {
                        //report login result back to subscriber
                        OnLoginSuccess.Invoke(result);
                        }
                    },

                    // Failure
                    (PlayFabError error) =>
                    {
                        if (OnPlayFabError != null)
                        {
                        //report error back to subscriber
                        OnPlayFabError.Invoke(error);
                        }
                    });

                return;
            }

            // If username & password is empty, then do not continue, and Call back to Authentication UI Display 
            if (string.IsNullOrEmpty(Email) && string.IsNullOrEmpty(Password))
            {
                OnDisplayAuthentication.Invoke();
                return;
            }

            // We have not opted for remember me in a previous session, so now we have to login the user with email & password.
            PlayFabClientAPI.LoginWithEmailAddress(
                new LoginWithEmailAddressRequest()
                {
                    TitleId = PlayFabSettings.TitleId,
                    Email = Email,
                    Password = Password,
                    InfoRequestParameters = InfoRequestParams
                },

                // Success
                (LoginResult result) =>
                {
                // Store identity and session
                _playFabId = result.PlayFabId;
                    _sessionTicket = result.SessionTicket;

                // Note: At this point, they already have an account with PlayFab using a Username (email) & Password
                // If RememberMe is checked, then generate a new Guid for Login with CustomId.
                if (RememberMe)
                    {
                        RememberMeId = Guid.NewGuid().ToString();
                        AuthType = Authtypes.EmailAndPassword;

                    // Fire and forget, but link a custom ID to this PlayFab Account.
                    PlayFabClientAPI.LinkCustomID(
                            new LinkCustomIDRequest
                            {
                                CustomId = RememberMeId,
                                ForceLink = ForceLink
                            },
                            null,   // Success callback
                            null    // Failure callback
                            );
                    }

                    if (OnLoginSuccess != null)
                    {
                    //report login result back to subscriber
                    OnLoginSuccess.Invoke(result);
                    }
                },

                // Failure
                (PlayFabError error) =>
                {
                    if (OnPlayFabError != null)
                    {
                    //Report error back to subscriber
                    OnPlayFabError.Invoke(error);
                    }
                });
        }


        /// <summary>
        /// Register a user with an Email & Password
        /// Note: We are not using the RegisterPlayFab API
        /// </summary>
        private void AddAccountAndPassword()
        {
            // Any time we attempt to register a player, first silently authenticate the player.
            // This will retain the players True Origination (Android, iOS, Desktop)
            SilentlyAuthenticate(
                (LoginResult result) =>
                {
                    if (result == null)
                    {
                    //something went wrong with Silent Authentication, Check the debug console.
                    OnPlayFabError.Invoke(new PlayFabError()
                        {
                            Error = PlayFabErrorCode.UnknownError,
                            ErrorMessage = "Silent Authentication by Device failed"
                        });
                    }

                // Note: If silent auth is success, which is should always be and the following 
                // below code fails because of some error returned by the server ( like invalid email or bad password )
                // this is okay, because the next attempt will still use the same silent account that was already created.

                // Now add our username & password.
                PlayFabClientAPI.AddUsernamePassword(
                        new AddUsernamePasswordRequest()
                        {
                            Username = Username ?? result.PlayFabId, // Because it is required & Unique and not supplied by User.
                        Email = Email,
                            Password = Password,
                        },

                        // Success
                        (AddUsernamePasswordResult addResult) =>
                        {
                            if (OnLoginSuccess != null)
                            {
                            // Store identity and session
                            _playFabId = result.PlayFabId;
                                _sessionTicket = result.SessionTicket;

                            // If they opted to be remembered on next login.
                            if (RememberMe)
                                {
                                // Generate a new Guid 
                                RememberMeId = Guid.NewGuid().ToString();

                                // Fire and forget, but link the custom ID to this PlayFab Account.
                                PlayFabClientAPI.LinkCustomID(
                                        new LinkCustomIDRequest()
                                        {
                                            CustomId = RememberMeId,
                                            ForceLink = ForceLink
                                        },
                                        null,
                                        null
                                        );
                                }

                            // Override the auth type to ensure next login is using this auth type.
                            AuthType = Authtypes.EmailAndPassword;

                            // Report login result back to subscriber.
                            OnLoginSuccess.Invoke(result);
                            }
                        },

                        // Failure
                        (PlayFabError error) =>
                        {
                            if (OnPlayFabError != null)
                            {
                            //Report error result back to subscriber
                            OnPlayFabError.Invoke(error);
                            }
                        });
                });
        }

        public void SilentlyAuthenticate(System.Action<LoginResult> callback = null, string id = null)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        //Login with the android device ID
        PlayFabClientAPI.LoginWithAndroidDeviceID(new LoginWithAndroidDeviceIDRequest() {
            TitleId = PlayFabSettings.TitleId,
            AndroidDevice = SystemInfo.deviceModel,
            OS = SystemInfo.operatingSystem,
            AndroidDeviceId = PlayFabSettings.DeviceUniqueIdentifier,
            CreateAccount = true,
            InfoRequestParameters = InfoRequestParams
        }, (result) => {
            
            //Store Identity and session
            _playFabId = result.PlayFabId;
            _sessionTicket = result.SessionTicket;

            //check if we want to get this callback directly or send to event subscribers.
            if (callback == null && OnLoginSuccess != null)
            {
                //report login result back to the subscriber
                OnLoginSuccess.Invoke(result);
            }else if (callback != null)
            {
                //report login result back to the caller
                callback.Invoke(result);
            }
        }, (error) => {

            //report errro back to the subscriber
            if(callback == null && OnPlayFabError != null){
                OnPlayFabError.Invoke(error);
            }else{
                //make sure the loop completes, callback with null
                callback.Invoke(null);
                //Output what went wrong to the console.
                Debug.LogError(error.GenerateErrorReport());
            }
        });

#elif UNITY_IPHONE || UNITY_IOS && !UNITY_EDITOR
        PlayFabClientAPI.LoginWithIOSDeviceID(new LoginWithIOSDeviceIDRequest() {
            TitleId = PlayFabSettings.TitleId,
            DeviceModel = SystemInfo.deviceModel, 
            OS = SystemInfo.operatingSystem,
            DeviceId = PlayFabSettings.DeviceUniqueIdentifier,
            CreateAccount = true,
            InfoRequestParameters = InfoRequestParams
        }, (result) => {
            //Store Identity and session
            _playFabId = result.PlayFabId;
            _sessionTicket = result.SessionTicket;

            //check if we want to get this callback directly or send to event subscribers.
            if (callback == null && OnLoginSuccess != null)
            {
                //report login result back to the subscriber
                OnLoginSuccess.Invoke(result);
            }else if (callback != null)
            {
                //report login result back to the caller
                callback.Invoke(result);
            }
        }, (error) => {
            //report errro back to the subscriber
            if(callback == null && OnPlayFabError != null){
                OnPlayFabError.Invoke(error);
            }else{
                //make sure the loop completes, callback with null
                callback.Invoke(null);
                //Output what went wrong to the console.
                Debug.LogError(error.GenerateErrorReport());
            }
        });
#elif UNITY_WEBGL
            string customId = GetGUID();
            if (id != null)
            {
                customId = id;
            }

            PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                CustomId = customId,
                CreateAccount = true,
                InfoRequestParameters = InfoRequestParams
            }, (result) =>
            {
                //Store Identity and session
                _playFabId = result.PlayFabId;
                _sessionTicket = result.SessionTicket;
                //check if we want to get this callback directly or send to event subscribers.
                if (callback == null && OnLoginSuccess != null)
                {
                    //report login result back to the subscriber
                    OnLoginSuccess.Invoke(result);
                }
                else if (callback != null)
                {
                    //report login result back to the caller
                    callback.Invoke(result);
                }
            }, (error) =>
            {
                //report errro back to the subscriber
                if (callback == null && OnPlayFabError != null)
                {
                    OnPlayFabError.Invoke(error);
                }
                else
                {
                    //make sure the loop completes, callback with null
                    callback.Invoke(null);
                    //Output what went wrong to the console.
                    Debug.LogError(error.GenerateErrorReport());
                }

            });
#else
            string customId = PlayFabSettings.DeviceUniqueIdentifier;
            if (id != null)
            {
                customId = id;
            }

            PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                CustomId = customId,
                CreateAccount = true,
                InfoRequestParameters = InfoRequestParams
            }, (result) =>
            {
                //Store Identity and session
                _playFabId = result.PlayFabId;
                _sessionTicket = result.SessionTicket;
                //check if we want to get this callback directly or send to event subscribers.
                if (callback == null && OnLoginSuccess != null)
                {
                    //report login result back to the subscriber
                    OnLoginSuccess.Invoke(result);
                }
                else if (callback != null)
                {
                    //report login result back to the caller
                    callback.Invoke(result);
                }
            }, (error) =>
            {
                //report errro back to the subscriber
                if (callback == null && OnPlayFabError != null)
                {
                    OnPlayFabError.Invoke(error);
                }
                else
                {
                    //make sure the loop completes, callback with null
                    callback.Invoke(null);
                    //Output what went wrong to the console.
                    Debug.LogError(error.GenerateErrorReport());
                }

            });
#endif
        }

        private string GetGUID()
        {
            if (PlayerPrefs.HasKey("GUID"))
            {
                return PlayerPrefs.GetString("GUID");
            }
            else
            {
                string newGuid = System.Guid.NewGuid().ToString();
                PlayerPrefs.SetString("GUID", newGuid);
                return newGuid;
            }
        }

        public void RequestPhotonToken()
        {
            if (OnGetPhotonToken != null)
            {
                OnGetPhotonToken.Invoke();
            }
            PlayFabClientAPI.GetPhotonAuthenticationToken(new GetPhotonAuthenticationTokenRequest()
            {
                PhotonApplicationId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime
            }, AuthenticateWithPhoton, (error) =>
            {
                //report errro back to the subscriber
                if (OnPlayFabError != null)
                {
                    OnPlayFabError.Invoke(error);
                    Debug.LogError(error.GenerateErrorReport());
                }

            });
        }

        private void LinkCustomId()
        {
            PlayFabClientAPI.LinkCustomID(new LinkCustomIDRequest()
            {
                CustomId = PlayFabSettings.DeviceUniqueIdentifier,
            }, (result) =>
            {
            }
            , (PlayFabError error) =>
            {
                if (OnPlayFabError != null)
                {
                //Report error result back to subscriber
                OnPlayFabError.Invoke(error);
                }
            });
        }

        private void AuthenticateWithPhoton(GetPhotonAuthenticationTokenResult obj)
        {
            //We set AuthType to custom, meaning we bring our own, PlayFab authentication procedure.
            var customAuth = new AuthenticationValues { AuthType = CustomAuthenticationType.Custom };
            //We add "username" parameter. Do not let it confuse you: PlayFab is expecting this parameter to contain player PlayFab ID (!) and not username.
            customAuth.AddAuthParameter("username", _playFabId);    // expected by PlayFab custom auth service

            //We add "token" parameter. PlayFab expects it to contain Photon Authentication Token issues to your during previous step.
            customAuth.AddAuthParameter("token", obj.PhotonCustomAuthenticationToken);

            //We finally tell Photon to use this authentication parameters throughout the entire application.
            PhotonNetwork.AuthValues = customAuth;
            if (OnPhotonAuthentication != null)
            {
                OnPhotonAuthentication.Invoke();
            }
        }

        public void GetPlayerInfo(System.Action<GetPlayerProfileResult> callback = null)
        {
            if (_playFabId == "")
            {
                return;
            }
            PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest()
            {
                PlayFabId = _playFabId
            },
            (result) =>
            {
                if (callback != null)
                {
                    callback.Invoke(result);
                }
            }
            ,
            (PlayFabError error) =>
            {
                if (error.Error == PlayFabErrorCode.ProfileDoesNotExist)
                {
                    AutoLogin();
                }
                else
                {
                    if (OnPlayFabError != null)
                    {
                    //Report error result back to subscriber
                    OnPlayFabError.Invoke(error);
                    }
                }
            });
        }

        public void SetPlayerDisplayName(string displayName, System.Action<UpdateUserTitleDisplayNameResult> callback = null)
        {
            PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest()
            {
                DisplayName = displayName
            }, (result) =>
            {
                if (callback != null)
                {
                    callback.Invoke(result);
                }
            },
            (PlayFabError error) =>
            {
                if (OnPlayFabError != null)
                {
                //Report error result back to subscriber
                OnPlayFabError.Invoke(error);
                }
            }); ;
        }


        public void UnlinkSilentAuth()
        {
            SilentlyAuthenticate((result) =>
            {

#if UNITY_ANDROID && !UNITY_EDITOR
            //Get the device id from native android
            AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
            AndroidJavaClass secure = new AndroidJavaClass("android.provider.Settings$Secure");
            string deviceId = secure.CallStatic<string>("getString", contentResolver, "android_id");

            //Fire and forget, unlink this android device.
            PlayFabClientAPI.UnlinkAndroidDeviceID(new UnlinkAndroidDeviceIDRequest() {
                AndroidDeviceId = deviceId
            }, null, null);

#elif UNITY_IPHONE || UNITY_IOS && !UNITY_EDITOR
            PlayFabClientAPI.UnlinkIOSDeviceID(new UnlinkIOSDeviceIDRequest()
            {
                DeviceId = SystemInfo.deviceUniqueIdentifier
            }, null, null);
#else
            PlayFabClientAPI.UnlinkCustomID(new UnlinkCustomIDRequest()
                {
                    CustomId = SystemInfo.deviceUniqueIdentifier
                }, null, null);
#endif

        });
        }


    }
}
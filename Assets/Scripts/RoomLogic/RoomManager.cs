using ExitGames.Client.Photon;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using MoralisWeb3ApiSdk;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
#if UNITY_WEBGL
using Moralis.WebGL.Platform.Objects;
using Moralis.WebGL.Web3Api.Models;
using Moralis.WebGL.Hex.HexTypes;
#endif

namespace com.louis.shootball
{
    public class RoomManager : MonoBehaviourPunCallbacks
    {
        private class SetGameRequest
        {
            public string guid;
            public List<string> player_blue;
            public List<string> player_red;
            public SetGameRequest(string _guid, List<string> _red, List<string> _blue) {
                guid = _guid;
                player_red = _red;
                player_blue = _blue;
            }
        }

        public GameObject roomPlayerPrefab;
        public static GameObject LocalPlayerInstance;
        public bool startingGame = false;
        public bool started = false;
        public string LobbySceneName = "Lobby";

        #region MonoCallbacks
        void Start()
        {
            if (PhotonNetwork.IsMasterClient) {
                PhotonNetwork.CurrentRoom.PlayerTtl = 0;
                PhotonNetwork.CurrentRoom.IsOpen = true;
            }
            SetUpRoomPorperties();
            SetupPlayer();
        }


        public async void Update()
        {
            if (startingGame && !started)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    if (CheckStartable())
                    {
                        photonView.RPC("SetStarted", RpcTarget.All, true);
                        PhotonNetwork.CurrentRoom.PlayerTtl = 300000;
                        PhotonNetwork.CurrentRoom.EmptyRoomTtl = 0;
                        PhotonNetwork.LoadLevel("Football");
                    }
                }
            }
        }

        #endregion

        #region PUNcallbacks
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(LobbySceneName);
        }

        public override void OnDisconnected(DisconnectCause cause) {
            LeaveRoom();
        }
        #endregion

        #region Public Methods
        public async void CheckButton() {
            //await CheckPaid("0x9B8139a4EE3C294a15c76Ec982AE8e8073a6108F");
        }

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        public void SwapTeam(string teamName)
        {
            if (startingGame)
            {
                return;
            }
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("SwapTeam", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer, teamName);
        }

        public async void StartGame()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            if (PhotonNetwork.CurrentRoom.PlayerCount <= 1) {
                return;
            }

            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player.CustomProperties.ContainsKey("address"))
                {
                    string address = (string)player.CustomProperties["address"];
                    Debug.Log(address);
                    bool paid = await CheckPaid(address);
                    if (!paid) {
                        Debug.Log(address + ": havent paid!!!!");
                        ErrorPanelManager.Instance.OnErrorMesssage(address + ": havent paid!!!!");
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            bool success = await SetGame();
            if (!success) {
                return;
            }
            PhotonNetwork.CurrentRoom.IsOpen = false;
            photonView.RPC("SetStartingGame", RpcTarget.All, true);
            AssignSpawnPosition();
            ClearSpawnedProperties();
        }

        public async void ReadyGame()
        {
            //Do some checking before ready

            //Contract interaction
            bool success = await PlayerReadyContractInteraction();
            if (success)
            {
                //should changed to backend
                Hashtable ht = new Hashtable();
                ht.Add("EnterancePaid", true);
                PhotonNetwork.LocalPlayer.SetCustomProperties(ht);
            }
            else {

            }
        }


        #endregion

        #region RPC

        [PunRPC]
        public void SetStarted(bool state)
        {
            started = state;
        }

        [PunRPC]
        private void SwapTeam(Player Player, string teamName)
        {
            Player[] Players;
            PhotonTeamsManager.Instance.TryGetTeamMembers(teamName, out Players);

            int memebers = Players.Length;
            if (memebers < (PhotonNetwork.CurrentRoom.MaxPlayers / 2))
            {
                if (Player.GetPhotonTeam() != null)
                {
                    Player.SwitchTeam(teamName);
                }
                else {
                    Player.JoinTeam(teamName);
                }
            }
            else
            {
                Debug.LogWarning(teamName + " team is full");
            }
        }
        [PunRPC]
        public void SetStartingGame(bool state)
        {
            startingGame = state;
        }
        #endregion

        #region Private Methods
        private async UniTask<bool> SetGame()
        {
            string roomHash = PhotonNetwork.CurrentRoom.GetHashCode().ToString();
            Player[] redPlayers;
            PhotonTeamsManager.Instance.TryGetTeamMembers("Red", out redPlayers);
            List<string> redAddresses = new List<string>();
            foreach (var redp in redPlayers)
            {
                redAddresses.Add((string)redp.CustomProperties["address"]);
            }
            Player[] bluePlayers;
            PhotonTeamsManager.Instance.TryGetTeamMembers("Blue", out bluePlayers);
            List<string> blueAddresses = new List<string>();
            foreach (var bluep in bluePlayers)
            {
                blueAddresses.Add((string)bluep.CustomProperties["address"]);
            }
            SetGameRequest request = new SetGameRequest(roomHash, redAddresses, blueAddresses);
            string msg = JsonUtility.ToJson(request);
            Debug.Log(msg);
            UnityWebRequest www = UnityWebRequest.Put("https://l3fynv0esl.execute-api.ap-southeast-1.amazonaws.com/dev/game", msg);
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("Content-type", "application/json");
            www.SetRequestHeader("Accept", "application/json");
            await www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                return false;
            }
            else
            {
                Debug.Log("Form upload complete!");
                Debug.Log(www.downloadHandler.text);
                return true;
            }

        }
        private async UniTask<bool> CheckPaid(string address)
        {
            // Function ABI input parameters
            object[] inputParams = new object[1];
            inputParams[0] = new { internalType = "address", name = "address", type = "address" };
            // Function ABI Output parameters
            object[] outputParams = new object[1];
            outputParams[0] = new { internalType = "uint256", name = "", type = "uint256" };
            // Function ABI
            object[] abi = new object[1];
            abi[0] = new { inputs = inputParams, name = Constants.VERIFY_FUNCTION, outputs = outputParams, stateMutability = "view", type = "function" };
            // Define request object
            RunContractDto rcd = new RunContractDto()
            {
                Abi = abi,
                Params = new { address = address }
            };

            Debug.Log(rcd);

            string resp = await MoralisInterface.GetClient().Web3Api.Native.RunContractFunction(Constants.ENTRANCE_CONTRACT_ADDRESS, Constants.VERIFY_FUNCTION, rcd, ChainList.kovan);
            // Set gas estimate
            //object[] pars = new object[1];
            //pars[0] = address;
            //HexBigInteger gas = new HexBigInteger(0);
            //HexBigInteger value = new HexBigInteger(new System.Numerics.BigInteger(0));
            //string resp = await MoralisInterface.ExecuteContractFunction(Constants.ENTRANCE_CONTRACT_ADDRESS, Constants.ENTRANCE_ABI, Constants.VERIFY_FUNCTION, pars, value, gas, gas);
            Debug.Log(resp);
            if (resp == "1" || resp == "2")
            {
                return true;
            }
            else {
                return false;
            }
        }

        private async UniTask<bool> PlayerReadyContractInteraction() {

            // Need the user for the wallet address
            MoralisUser user = await MoralisInterface.GetUserAsync();

            string addr = user.authData["moralisEth"]["id"].ToString();

            // get team
            string team = PhotonNetwork.LocalPlayer.GetPhotonTeam().Name;
            object[] pars = new object[1];
            if (team == "Blue")
            {
                pars[0] = 1;
            }
            else if (team == "Red")
            {
                pars[0] = 2;
            }
            // Set gas estimate
            HexBigInteger gas = new HexBigInteger(0);
            HexBigInteger value = new HexBigInteger(new System.Numerics.BigInteger(1000000000000000));
            string resp = await MoralisInterface.ExecuteContractFunction(Constants.ENTRANCE_CONTRACT_ADDRESS, Constants.ENTRANCE_ABI, Constants.ENTRANCE_FUNCTION, pars, value, gas, gas);
            Debug.Log(resp);
            //TODO
            return true;
        }

        private void SetupPlayer() {

            if (roomPlayerPrefab == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color>  roomPlayerPrefab Reference. Please set it up in GameObject 'Room Manager'", this);
            }
            else
            {
                if (RoomManager.LocalPlayerInstance == null)
                {
                    Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                    // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                    PhotonNetwork.Instantiate(this.roomPlayerPrefab.name, Vector3.zero, Quaternion.identity);
                    photonView.RPC("JoinTeamRPC", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
                    SetUserData();
                }
                else
                {
                    Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
                }
            }
        }


        private void SetUpRoomPorperties() {
            if ((bool)PhotonNetwork.CurrentRoom.CustomProperties[GameConfig.isQuickModeKey] == true)
            {
                PhotonNetwork.CurrentRoom.PlayerTtl = 300000;
                PhotonNetwork.CurrentRoom.EmptyRoomTtl = 12000;
            }
            else {
                PhotonNetwork.CurrentRoom.PlayerTtl = 0;
                PhotonNetwork.CurrentRoom.EmptyRoomTtl = 12000;
            }
        }

        void SetUserData()
        {
            Debug.Log("SettingUserData");
            PlayFabUserDataCall(0);
        }

        void PlayFabUserDataCall(int retries) {
            if (retries < 10)
            {
                PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
                {
                    Data = new Dictionary<string, string>() {
                    {"CurrentRoom", PhotonNetwork.CurrentRoom.Name}
                }
                },
                result =>
                {
                    Debug.Log("Successfully updated user data");
                    return;
                },
                error =>
                {
                    Debug.Log("Got error setting user data");
                    Debug.Log(error.GenerateErrorReport());
                    retries++;
                    PlayFabUserDataCall(retries);
                });
            }
            else {
                ErrorPanelManager.Instance.OnErrorMesssage("Error when Joining room");
                PhotonNetwork.LeaveRoom();
            }
        }

        private void ClearSpawnedProperties()
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                Hashtable ht = player.CustomProperties;
                ht.Remove("Spawned");
                player.SetCustomProperties(ht);
            }
        }

        [PunRPC]
        private void JoinTeamRPC(Player newPlayer)
        {
            newPlayer.LeaveCurrentTeam();
            Player[] BluePlayers;
            Player[] RedPlayers;
            PhotonTeamsManager.Instance.TryGetTeamMembers("Blue", out BluePlayers);
            PhotonTeamsManager.Instance.TryGetTeamMembers("Red", out RedPlayers);
            int blueMemebers = BluePlayers.Length;
            int redMemebers = RedPlayers.Length;
            PhotonTeam currentTeam = newPlayer.GetPhotonTeam();
            if (currentTeam != null)
            {
                if (redMemebers > blueMemebers)
                {
                    newPlayer.SwitchTeam("Blue");
                }
                else
                {
                    newPlayer.SwitchTeam("Red");
                }
            }
            else
            {
                if (redMemebers > blueMemebers)
                {
                    newPlayer.JoinTeam("Blue");
                }
                else
                {
                    newPlayer.JoinTeam("Red");
                }
            }
        }
        private void RoomMasterJoinTeam()
        {
            SwapTeam(PhotonNetwork.LocalPlayer, "Red");
        }

        //Assign Spawn Position For Each Player
        private void AssignSpawnPosition()
        {
            int redIndex = 0;
            int blueIndex = 0;
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                PhotonTeam team = player.GetPhotonTeam();
                if (team.Name == "Red")
                {
                    Hashtable hash = player.CustomProperties;
                    if (hash.ContainsKey("TeamOrder"))
                    {
                        hash["TeamOrder"] = redIndex;
                    }
                    else {
                        hash.Add("TeamOrder", redIndex);
                    }
                    player.SetCustomProperties(hash);
                    redIndex++;
                }
                else if (team.Name == "Blue")
                {
                    Hashtable hash = player.CustomProperties;
                    if (hash.ContainsKey("TeamOrder"))
                    {
                        hash["TeamOrder"] = blueIndex;
                    }
                    else
                    {
                        hash.Add("TeamOrder", blueIndex);
                    }
                    player.SetCustomProperties(hash);
                    blueIndex++;
                }
            }
        }
        private bool CheckStartable()
        {

            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player.CustomProperties["TeamOrder"] == null)
                {
                    Debug.Log("NO TEAM ID for player : " + player.NickName);
                    return false;
                }
                if (player.CustomProperties.ContainsKey("Spawned"))
                {
                    Debug.Log("Got Spawned for player : " + player.NickName);
                    return false;
                }
            }
            //TODO: think a checking method to check if it is startable
            //no checking for now
            return true;
        }
#endregion

    }
}
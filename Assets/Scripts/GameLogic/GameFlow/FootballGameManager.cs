using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using UnityEngine.Events;
using Cinemachine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace com.louis.shootball
{
    public enum GameState { 
        WAITING,
        COUNTING,
        STARTED,
        ENDED
    }

    public class FootballGameManager : MonoBehaviourPunCallbacks, IPunObservable, IInRoomCallbacks
    {
        private class SetWinnerRequest {
            public int winner;
            public string guid;
            public SetWinnerRequest(int _winner, string _guid) {
                winner = _winner;
                guid = _guid;
            }
        }

        [SerializeField]
        public int blueScore = 0;

        [SerializeField]
        public int redScore = 0;

        public static FootballGameManager Instance;

        [Tooltip("The ChampionList for spawning Player")]
        public ChampionList championList;
        [Tooltip("The prefab to use for representing the player")]
        public GameObject ballPrefab;
        public CinemachineVirtualCamera orbitcam;
        public Vector3 BallStartingPosition = new Vector3(0, 0.7f, 0);

        public Transform[] BlueSpawnPoint;
        public Transform[] RedSpawnPoint;

        int PlayerSpawnCount = 0;
        int maxPlayer = 99;
        public GameState gameState = GameState.WAITING;
        public Animator countDown;
        float startTime = 0;
        public float timer = 0;
        public float GameLength = 600;
        public UnityEvent CountDownEvents;
        public UnityEvent StartGameEvents;
        public UnityEvent RedScoreEvents;
        public UnityEvent BlueScoreEvents;
        public UnityEvent outBoundEvents;
        public UnityEvent EndGameEvents;

        #region MonoCallbacks

        void Awake() {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        void Start()
        {
            if (championList == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> champion List Reference. Please set it up", this);
            }
            else
            {
                photonView.RPC("CheckPlayerExistAndSpawnPlayer", RpcTarget.MasterClient);
            }
            CollectActivePlayerAndSetMaxPlayer();
            // we call this on every client since the master client will change
            InvokeRepeating("SetPlayerTTL", 30, 30);
        }

        void Update()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (gameState == GameState.WAITING)
                {
                    if (PlayerSpawnCount >= maxPlayer)
                    {
                        photonView.RPC("StartCountDown", RpcTarget.All);
                        Invoke("StartGame", 3);
                        gameState = GameState.COUNTING;
                    }
                }
                if (gameState == GameState.STARTED) {
                    if (timer >= GameLength) {
                        EndGame();   
                    }
                }
            }
            if (gameState == GameState.STARTED)
            {
                if (startTime != 0)
                {
                    // Example for a increasing timer
                    timer = (float)PhotonNetwork.Time - startTime;
                }
            }
        }

        #endregion

        #region PhotonRPC
        [PunRPC]
        void CheckPlayerExistAndSpawnPlayer(PhotonMessageInfo info) {
            bool exist = false;
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (PhotonView.Get(player).Owner == info.Sender)
                {
                    exist = true;
                }
            }
            if (!exist) {
                photonView.RPC("SpawnPlayerRPC", info.Sender);
            }
        }

        [PunRPC]
        void SpawnPlayerRPC() { 
            SpawnPlayer();
        }


        [PunRPC]
        void SetRedScore(int score)
        {
            redScore = score;
        }

        [PunRPC]
        void SetBlueScore(int score)
        {
            blueScore = score;
        }

        [PunRPC]
        void StartCountDown()
        {
            countDown.SetTrigger("CountDown");
            CountDownEvents.Invoke();
        }


        [PunRPC]
        public void OnPlayerSpawn(int ID)
        {
            PlayerSpawnCount++;
        }

        [PunRPC]
        public void OnBlueScore() {
            BlueScoreEvents.Invoke();
        }

        [PunRPC]
        public void OnRedScore() {
            RedScoreEvents.Invoke();
        }

        [PunRPC]
        public void OnStartGame()
        {
            StartGameEvents.Invoke();
        }

        [PunRPC]
        public void OnEndGame(Team WinningTeam)
        {
            FootballUIManager.Instance.ShowBanner(WinningTeam);
            EndGameEvents.Invoke();
        }

        [PunRPC]
        public void SetLastPosition(Vector3 pos, Quaternion rot, bool shouldEnable)
        {
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (PhotonView.Get(player).Owner == PhotonNetwork.LocalPlayer)
                {
                    SkillPanel.Instance.SetUpPanel(player);
                    Debug.Log(shouldEnable);
                    if (shouldEnable) {
                        player.GetComponent<FootballThirdPersonController>().EnableMovement();
                        orbitcam.Priority = 0;
                    }
                    player.GetComponent<Teleporter>().Teleport(pos, rot);
                }
            }
        }

        #endregion

        #region Public Methods
        public void BackToRoom() {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.DestroyAll();
                PhotonNetwork.LoadLevel("Room");
            }
        }

        public void StartGame() {
            Hashtable ht = new Hashtable();
            ht.Add("StartTime", (float)PhotonNetwork.Time);
            gameState = GameState.STARTED;
            PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
            OnSpawnBall(BallStartingPosition, Team.NULL);
            photonView.RPC("OnStartGame", RpcTarget.All);
        }

        public async void EndGame() {
            gameState = GameState.ENDED;
            await SetGameWInner();
            if (redScore > blueScore)
            {
                photonView.RPC("OnEndGame", RpcTarget.All, Team.RED);
            }
            else if (blueScore > redScore)
            {
                photonView.RPC("OnEndGame", RpcTarget.All, Team.BLUE);
            }
            else
            {
                photonView.RPC("OnEndGame", RpcTarget.All, Team.NULL);
            }
            Invoke("BackToRoom", 10f);
        }
        
        public void OnSpawnBall(Vector3 position, Team kickableTeam) {
            if (PhotonNetwork.IsMasterClient)
            {
                GameObject ball = PhotonNetwork.InstantiateRoomObject(ballPrefab.name, position, Quaternion.identity);
                ball.GetComponent<BallBehaviour>().SetMoveableTeam(kickableTeam);
                ball.GetComponent<Rigidbody>().isKinematic = true;
            }
        }

        public void BlueTeamScore() {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("SetBlueScore",RpcTarget.AllBuffered , blueScore + 1);
                photonView.RPC("OnBlueScore", RpcTarget.All);
            }
        }

        public void RedTeamScore()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("SetRedScore", RpcTarget.AllBuffered, redScore + 1);
                photonView.RPC("OnRedScore", RpcTarget.All);
            }
        }

        #endregion

        #region Private Methods
        private async UniTask SetGameWInner() {
            string roomHash = PhotonNetwork.CurrentRoom.GetHashCode().ToString();
            int winner = 0;
            if (redScore > blueScore)
            {
                winner = 2;
            }
            else if (blueScore > redScore)
            {
                winner = 1;
            }
            else
            {
                winner = 3;
            }
            SetWinnerRequest request = new SetWinnerRequest(winner, roomHash);
            string msg = JsonUtility.ToJson(request);
            Debug.Log(msg);
            UnityWebRequest www = UnityWebRequest.Put("https://l3fynv0esl.execute-api.ap-southeast-1.amazonaws.com/dev/winner", msg);
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("Content-type", "application/json");
            www.SetRequestHeader("Accept", "application/json");
            await www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
                Debug.Log(www.downloadHandler.text);
            }
        }

        private void CollectActivePlayerAndSetMaxPlayer() {
            int activePlayersCount = 0;
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (!player.IsInactive)
                {
                    activePlayersCount++;
                }
            }
            maxPlayer = activePlayersCount;
        }

        /// <summary>
        /// this function is to set the player TTL to corresponding remaining time
        /// </summary>
        private void SetPlayerTTL() {
            if (PhotonNetwork.IsMasterClient)
            {
                float remainTime = GameLength - timer;
                int ttl = Mathf.FloorToInt(remainTime * 1000);
                PhotonNetwork.CurrentRoom.PlayerTtl = ttl;
            }
        }
        
        private void SpawnPlayer()
        {
            PhotonTeam team = PhotonNetwork.LocalPlayer.GetPhotonTeam();

            Hashtable ht = new Hashtable();
            ht.Add("Spawned", true);
            PhotonNetwork.LocalPlayer.SetCustomProperties(ht);

            int ID = (int)PhotonNetwork.LocalPlayer.CustomProperties["TeamOrder"];
            Transform SpawnPoint;
            if (team.Name == "Red")
            {
                SpawnPoint = RedSpawnPoint[ID];
            }
            else
            {
                SpawnPoint = BlueSpawnPoint[ID];
            }
            ht = PhotonNetwork.LocalPlayer.CustomProperties;
            int selectedID = (int)ht["Champion"];
            foreach (ChampionObject champion in championList.Champions)
            {
                if (champion.ChampionID == selectedID)
                {
                    GameObject player = PhotonNetwork.Instantiate(champion.ChampionInGamePlayer.name, SpawnPoint.position, SpawnPoint.rotation, 0);
                    SkillPanel.Instance.SetUpPanel(player);
                    photonView.RPC("OnPlayerSpawn", RpcTarget.MasterClient, PhotonView.Get(player).ViewID);
                }
            }
        }
        #endregion

        #region Photon Callbacks
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (newPlayer.HasRejoined)
                {
                    foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
                    {
                        if (PhotonView.Get(player).Owner == newPlayer)
                        {
                            if (gameState == GameState.WAITING)
                            {
                                photonView.RPC("SetLastPosition", newPlayer, player.transform.position, player.transform.rotation, false);
                            }
                            else
                            {
                                photonView.RPC("SetLastPosition", newPlayer, player.transform.position, player.transform.rotation, true);
                            }
                        }
                    }
                }
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (otherPlayer.CustomProperties.ContainsKey("Spawned"))
                {
                    bool spawned = (bool)otherPlayer.CustomProperties["Spawned"];
                    if (spawned)
                    {
                        PlayerSpawnCount--;
                    }
                }
            }
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            object propsTime;
            if (propertiesThatChanged.TryGetValue("StartTime", out propsTime))
            {
                startTime = (float)propsTime;
            }
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            SceneManager.LoadScene("Launcher");
        }
        #endregion



        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(gameState);
                stream.SendNext(PlayerSpawnCount);
                stream.SendNext(startTime);
            }
            else
            {
                this.gameState = (GameState)stream.ReceiveNext();
                this.PlayerSpawnCount = (int)stream.ReceiveNext();
                this.startTime = (float)stream.ReceiveNext();
            }
        }

        #endregion

    }
}
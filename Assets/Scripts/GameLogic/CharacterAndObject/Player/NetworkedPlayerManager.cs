using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Cinemachine;
using UnityEngine.InputSystem;
using Photon.Realtime;

namespace com.louis.shootball
{
    public class NetworkedPlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        FootballThirdPersonController controller;
        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;
        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField]
        private GameObject PlayerUiPrefab;
        public Transform camRoot;
        public PlayerInput input;
        public Team team = Team.RED;
        [SerializeField]
        private bool testing = false;


        void Start()
        {
            controller = this.GetComponent<FootballThirdPersonController>();
            SetUpPlayer();
        }

        void SetUpPlayer() {
            EnableControls();
            SetUpTeam();
            SetUpUI();
        }

        void SetUpUI() {
            if (PlayerUiPrefab != null)
            {
                GameObject _uiGo = Instantiate(PlayerUiPrefab, this.transform);
                //_uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
                _uiGo.GetComponent<PlayerUIManager>().SetTarget(this.gameObject, team);
            }
            else
            {
                Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
            }
        }

        void SetUpTeam() {
            if (photonView.Owner != null)
            {
                PhotonTeam playerPhotonTeam = photonView.Owner.GetPhotonTeam();
                if (playerPhotonTeam != null)
                {
                    string playerTeam = playerPhotonTeam.Name;
                    if (playerTeam == "Red")
                    {
                        team = Team.RED;
                        gameObject.layer = LayerMask.NameToLayer("RedPlayer");
                    }
                    else if (playerTeam == "Blue")
                    {
                        team = Team.BLUE;
                        gameObject.layer = LayerMask.NameToLayer("BluePlayer");
                    }
                }
            }
        }

        public void EnableControls() {
            if (photonView.Owner == PhotonNetwork.LocalPlayer || testing)
            {
                NetworkedPlayerManager.LocalPlayerInstance = this.gameObject;
                GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineVirtualCamera>().Follow = camRoot;
                input.enabled = true;
                input.ActivateInput();
            }
            else
            {
                input.enabled = false;
                controller.enabled = false;
                input.DeactivateInput();
            }
        }

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(team);
            }
            else
            {
                this.team = (Team)stream.ReceiveNext();
            }
        }

        #endregion

    }
}
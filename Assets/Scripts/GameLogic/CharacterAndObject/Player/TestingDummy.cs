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
    public class TestingDummy : MonoBehaviour
    {
        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;
        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField]
        private GameObject PlayerUiPrefab;
        public PlayerInput input;
        public Team team = Team.RED;

        // Start is called before the first frame update
        void Start()
        {
            SetUpPlayer();
        }

        void SetUpPlayer()
        {
            SetUpUI();
        }

        void SetUpUI()
        {
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
    }
}
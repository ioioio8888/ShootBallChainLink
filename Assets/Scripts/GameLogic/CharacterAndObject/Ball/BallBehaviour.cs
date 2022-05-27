using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Cysharp.Threading.Tasks;

namespace com.louis.shootball
{
    public class BallBehaviour : MonoBehaviourPun
    {
        [Tooltip("InUse means this ball will trigger events")]
        public bool inUse = true;
        public int lastTouchedID = 0;
        public Team lastTouchedTeam = Team.NULL;

        public Team moveableTeam = Team.NULL;
        public bool unlockAfterImpapct = true;
        public GameObject blueBlock;
        public GameObject redBlock;
        [SerializeField]
        public Renderer _renderer;
        public UnityEvent OnGroundCollision;

        private void Update()
        {
            switch (moveableTeam) {
                case Team.NULL:
                    _renderer.material.SetColor("_BaseColor", Color.white);
                    if (blueBlock.activeSelf == true)
                    {
                        blueBlock.SetActive(false);
                    }
                    if (redBlock.activeSelf == true)
                    {
                        redBlock.SetActive(false);
                    }
                    break;
                case Team.RED:
                    _renderer.material.SetColor("_BaseColor", Color.red);
                    if (blueBlock.activeSelf == false)
                    {
                        blueBlock.SetActive(true);
                    }
                    if (redBlock.activeSelf == true)
                    {
                        redBlock.SetActive(false);
                    }
                    break;
                case Team.BLUE:
                    _renderer.material.SetColor("_BaseColor", Color.blue);
                    if (blueBlock.activeSelf == true)
                    {
                        blueBlock.SetActive(false);
                    }
                    if (redBlock.activeSelf == false)
                    {
                        redBlock.SetActive(true);
                    }
                    break;
            }
        }

        public void SetInUse(bool state) {
            photonView.RPC("SetInUseRpc", RpcTarget.AllBuffered, state);
        }

        [PunRPC]
        public void SetInUseRpc(bool state) {
            inUse = state;
        }
        public void SetMoveableTeam(Team team)
        {
            photonView.RPC("SetMoveableTeamRpc", RpcTarget.AllBuffered, team);
        }

        [PunRPC]
        public async void SetMoveableTeamRpc(Team team)
        {
            moveableTeam = team;
            await UniTask.Delay(10000);
            moveableTeam = Team.NULL;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                
                PhotonView view = collision.gameObject.GetComponent<PhotonView>();
                NetworkedPlayerManager manager = collision.gameObject.GetComponent<NetworkedPlayerManager>();
                if ((view != null) && (manager != null))
                {
                    SetLastTouched(view.ViewID, manager.team);
                }
            }
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                OnGroundCollision.Invoke();
            }
        }

        public void SetLastTouched(int viewID, Team team) {
            photonView.RPC("SetLastTouchedRpc", RpcTarget.AllBuffered, viewID, team);
        }


        [PunRPC]
        public void SetLastTouchedRpc(int viewID, Team team)
        {
            lastTouchedTeam = team;
            lastTouchedID = viewID;
        }

    }

}
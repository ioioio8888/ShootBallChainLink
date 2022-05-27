using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cysharp.Threading.Tasks;

namespace com.louis.shootball
{
    public class GateBehaviour : MonoBehaviourPun
    {
        public Team gateType;

        private void OnTriggerEnter(Collider other)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (other.tag == "Ball")
                {
                    BallBehaviour ball = other.GetComponent<BallBehaviour>();
                    if (ball != null) {
                        if (ball.inUse)
                        {
                            //disable the ball's inUse to avoid multiple trigger
                            ball.SetInUse(false);
                            OnGoal(other.gameObject);
                        }
                    }
                }
            }
        }

        private void OnGoal(GameObject ball) {
           
            if (gateType == Team.RED)
            {
                FootballGameManager.Instance.BlueTeamScore();
                photonView.RPC("OnGoalRPC", RpcTarget.All, PhotonView.Get(ball).ViewID, Team.RED);
            }
            else
            {
                FootballGameManager.Instance.RedTeamScore();
                photonView.RPC("OnGoalRPC", RpcTarget.All, PhotonView.Get(ball).ViewID, Team.BLUE);
            }
        }

        [PunRPC]
        public async UniTask OnGoalRPC(int ViewID, Team nextStartTeam)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2));
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(PhotonView.Find(ViewID).gameObject);
                FootballGameManager.Instance.OnSpawnBall(FootballGameManager.Instance.BallStartingPosition, nextStartTeam);
            }
        }

    }
}
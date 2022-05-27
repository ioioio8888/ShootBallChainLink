using System;
using UnityEngine;
using Photon.Pun;
using Cysharp.Threading.Tasks;

namespace com.louis.shootball
{
    public enum BoundaryPosition { 
        NORTH,
        NORTHEAST,
        NORTHWEST,
        SOUTH,
        SOUTHEAST,
        SOUTHWEST,
        EAST,
        WEST
    }

    public class Boundaries : MonoBehaviourPun
    {
        public BoundaryPosition pos;
        public float eastRestartLineX = 28.5f;
        public float westRestartLineX = -28.5f;
        //north is the Red Goal
        [Tooltip("North is the Red Goal")]
        public Vector3 northRestartPoint = new Vector3(0, 0.7f, 38);
        public Vector3 neRestartPoint = new Vector3(28.5f, 0.7f, 45);
        public Vector3 nwRestartPoint = new Vector3(-28.5f, 0.7f, 45);
        //South is the Blue Goal
        [Tooltip("South is the Blue Goal")]
        public Vector3 southRestartPoint = new Vector3(0, 0.7f, -38);
        public Vector3 seRestartPoint = new Vector3(28.5f, 0.7f, -45);
        public Vector3 swRestartPoint = new Vector3(-28.5f, 0.7f, -45);

        private void OnTriggerEnter(Collider other)
        {
            if (PhotonNetwork.IsMasterClient) {
                if (other.tag == "Ball")
                {
                    //check if the ball has ball Behaviour
                    BallBehaviour ball = other.GetComponent<BallBehaviour>();
                    if (ball == null) {
                        return;
                    }
                    if (!ball.inUse)
                    {
                        return;
                    }

                    //disable the ball's inUse to avoid multiple trigger
                    ball.SetInUse(false);
                    Vector3 spawnPos;
                    Team kickableTeam;
                    if (ball.lastTouchedTeam == Team.BLUE) {
                        kickableTeam = Team.RED;
                    }
                    else {
                        kickableTeam = Team.BLUE;
                    }
                    switch (pos) {
                        case BoundaryPosition.EAST:
                            spawnPos = new Vector3(eastRestartLineX, 0.6f, other.transform.position.z);
                            photonView.RPC("BounderiesRPC", RpcTarget.All, PhotonView.Get(other.gameObject).ViewID ,spawnPos, kickableTeam);
                            //await ProcessBoundaries(other.gameObject, spawnPos, kickableTeam);
                            break;
                        case BoundaryPosition.WEST:
                            spawnPos = new Vector3(westRestartLineX, 0.6f, other.transform.position.z);
                            photonView.RPC("BounderiesRPC", RpcTarget.All, PhotonView.Get(other.gameObject).ViewID, spawnPos, kickableTeam);
                            break;
                        case BoundaryPosition.NORTH:
                            if (kickableTeam == Team.RED)
                            {
                                spawnPos = northRestartPoint;
                                photonView.RPC("BounderiesRPC", RpcTarget.All, PhotonView.Get(other.gameObject).ViewID, spawnPos, kickableTeam);
                            }
                            else {
                                spawnPos = neRestartPoint;
                                photonView.RPC("BounderiesRPC", RpcTarget.All, PhotonView.Get(other.gameObject).ViewID, spawnPos, kickableTeam);
                            }
                            break;
                        case BoundaryPosition.NORTHEAST:
                            if (kickableTeam == Team.RED)
                            {
                                spawnPos = northRestartPoint;
                                photonView.RPC("BounderiesRPC", RpcTarget.All, PhotonView.Get(other.gameObject).ViewID, spawnPos, kickableTeam);
                            }
                            else
                            {
                                spawnPos = neRestartPoint;
                                photonView.RPC("BounderiesRPC", RpcTarget.All, PhotonView.Get(other.gameObject).ViewID, spawnPos, kickableTeam);
                            }
                            break;
                        case BoundaryPosition.NORTHWEST:
                            if (kickableTeam == Team.RED)
                            {
                                spawnPos = northRestartPoint;
                                photonView.RPC("BounderiesRPC", RpcTarget.All, PhotonView.Get(other.gameObject).ViewID, spawnPos, kickableTeam);
                            }
                            else
                            {
                                spawnPos = nwRestartPoint;
                                photonView.RPC("BounderiesRPC", RpcTarget.All, PhotonView.Get(other.gameObject).ViewID, spawnPos, kickableTeam);
                            }
                            break;
                        case BoundaryPosition.SOUTH:
                            if (kickableTeam == Team.BLUE)
                            {
                                spawnPos = southRestartPoint;
                                photonView.RPC("BounderiesRPC", RpcTarget.All, PhotonView.Get(other.gameObject).ViewID, spawnPos, kickableTeam);
                            }
                            else
                            {
                                spawnPos = seRestartPoint;
                                photonView.RPC("BounderiesRPC", RpcTarget.All, PhotonView.Get(other.gameObject).ViewID, spawnPos, kickableTeam);
                            }
                            break;
                        case BoundaryPosition.SOUTHEAST:
                            if (kickableTeam == Team.BLUE)
                            {
                                spawnPos = southRestartPoint;
                                photonView.RPC("BounderiesRPC", RpcTarget.All, PhotonView.Get(other.gameObject).ViewID, spawnPos, kickableTeam);
                            }
                            else
                            {
                                spawnPos = seRestartPoint;
                                photonView.RPC("BounderiesRPC", RpcTarget.All, PhotonView.Get(other.gameObject).ViewID, spawnPos, kickableTeam);
                            }
                            break;
                        case BoundaryPosition.SOUTHWEST:
                            if (kickableTeam == Team.BLUE)
                            {
                                spawnPos = southRestartPoint;
                                photonView.RPC("BounderiesRPC", RpcTarget.All, PhotonView.Get(other.gameObject).ViewID, spawnPos, kickableTeam);
                            }
                            else
                            {
                                spawnPos = swRestartPoint;
                                photonView.RPC("BounderiesRPC", RpcTarget.All, PhotonView.Get(other.gameObject).ViewID, spawnPos, kickableTeam);
                            }
                            break;
                    }
                }
            }
        }

        [PunRPC]
        public async void BounderiesRPC(int viewID, Vector3 spawnPos, Team kickableTeam) {
            await ProcessBoundaries(PhotonView.Find(viewID).gameObject, spawnPos, kickableTeam);
        }




        public async UniTask ProcessBoundaries(GameObject ball, Vector3 spawnPos, Team kickableTeam) {
            FootballGameManager.Instance.outBoundEvents.Invoke();
            await UniTask.Delay(TimeSpan.FromSeconds(2));
            if (PhotonNetwork.IsMasterClient) { 
                PhotonNetwork.Destroy(ball);
                FootballGameManager.Instance.OnSpawnBall(spawnPos, kickableTeam);
            }
        }

    }
}
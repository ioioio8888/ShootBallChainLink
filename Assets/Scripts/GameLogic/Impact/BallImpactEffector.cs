using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
namespace com.louis.shootball
{
    public class BallImpactEffector : RigidBodyImpactEffector
    {
        private BallBehaviour ballBehaviour;
        [SerializeField]
        private AudioSource audioSource;
        protected override void Start()
        {
            base.Start();
            ballBehaviour = GetComponent<BallBehaviour>();
        }

        public override void AddImpact(Vector3 dir, float force, object source)
        {
            body.isKinematic = false;
            GameObject impacter = source as GameObject;
            NetworkedPlayerManager manager = impacter.GetComponent<NetworkedPlayerManager>();
            if (manager != null) {
                ballBehaviour.SetLastTouched(PhotonView.Get(impacter).ViewID, manager.team);
                if (force > 3)
                {
                    audioSource.Play();
                }
                if (ballBehaviour.moveableTeam == Team.NULL)
                {
                    pushObject(dir, force);
                }
                else
                {
                    if (manager.team == ballBehaviour.moveableTeam)
                    {
                        pushObject(dir, force);
                        if (ballBehaviour.unlockAfterImpapct) {
                            ballBehaviour.SetMoveableTeam(Team.NULL);
                        }
                    }
                }
            }
        }
    }
}
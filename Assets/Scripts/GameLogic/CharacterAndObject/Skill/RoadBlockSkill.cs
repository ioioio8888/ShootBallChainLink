using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

namespace com.louis.shootball
{
    public class RoadBlockSkill : SkillBase
    {
        public GameObject RoadBlockPrefab;
        public UnityEvent castStartFeedbackEvent;
        public UnityEvent castEndFeedbackEvent;

        public override async void CastSkill()
        {
            await ProcessSkill();
        }
        public override bool IsCastable()
        {
            if (!ability._thirdPersonController.Grounded)
            {
                return false;
            }
            if (casting) {
                return false;
            }
            return true;
        }

        private async UniTask ProcessSkill()
        {
            casting = true;
            photonView.RPC("RoadBlockCastAnimation", RpcTarget.All);
            ability._thirdPersonController.DisableMovement(1.5f);
            await UniTask.Delay(700);
            Vector3 pos = transform.forward * 3 + transform.position;
            photonView.RPC("SpawnRoadBlock", RpcTarget.MasterClient, pos, transform.rotation);
            photonView.RPC("RoadBlockEndCastAnimation", RpcTarget.All);
            await UniTask.Delay(800);
            casting = false;
        }

        [PunRPC]
        void RoadBlockCastAnimation()
        {
            ability._animator.SetTrigger("SmallSpell");
            castStartFeedbackEvent.Invoke();
        }

        [PunRPC]
        void SpawnRoadBlock(Vector3 position, Quaternion rotation)
        {
            PhotonNetwork.InstantiateRoomObject(RoadBlockPrefab.name, position, rotation);
        }

        [PunRPC]
        void RoadBlockEndCastAnimation()
        {
            castEndFeedbackEvent.Invoke();
        }
    }
}
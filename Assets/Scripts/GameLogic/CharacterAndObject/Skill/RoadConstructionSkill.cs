using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

namespace com.louis.shootball
{
    public class RoadConstructionSkill : SkillBase
    {
        public GameObject RoadConstructionPrefab;
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
            if (casting)
            {
                return false;
            }
            return true;
        }

        private async UniTask ProcessSkill()
        {
            casting = true;
            photonView.RPC("RoadConstructionCastAnimation", RpcTarget.All);
            ability._thirdPersonController.DisableMovement(2.5f);
            await UniTask.Delay(1300);
            float z = 0;
            if (this.transform.forward.z > 0)
            {
                z = this.transform.position.z + 7;
            }
            else
            {
                z = this.transform.position.z - 7;
            }
            photonView.RPC("SpawnRoadConstruction", RpcTarget.MasterClient, z);
            photonView.RPC("RoadConstructionEndCastAnimation", RpcTarget.All);
            await UniTask.Delay(1200);
            casting = false;
        }

        [PunRPC]
        void RoadConstructionCastAnimation()
        {
            ability._animator.SetTrigger("GroundSpell");
            castStartFeedbackEvent.Invoke();
        }

        [PunRPC]
        void SpawnRoadConstruction(float Zposition)
        {
            Vector3 pos = new Vector3(0, 0, Zposition);
            PhotonNetwork.InstantiateRoomObject(RoadConstructionPrefab.name, pos, Quaternion.identity);
        }

        [PunRPC]
        void RoadConstructionEndCastAnimation()
        {
            castEndFeedbackEvent.Invoke();
        }
    }
}
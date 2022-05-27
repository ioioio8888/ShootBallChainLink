using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

namespace com.louis.shootball
{
    public class RestrictedAreaSkill : SkillBase
    {
        public GameObject BlueRestrictedAreaPrefab;
        public GameObject RedRestrictedAreaPrefab;
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
            photonView.RPC("RestrictedAreaCastAnimation", RpcTarget.All);
            ability._thirdPersonController.DisableMovement(2.5f);
            await UniTask.Delay(1300);
            Team playerTeam = GetComponent<NetworkedPlayerManager>().team;
            photonView.RPC("SpawnRestrictedArea", RpcTarget.MasterClient, playerTeam, this.transform.position);
            photonView.RPC("RestrictedAreaEndCastAnimation", RpcTarget.All);
            await UniTask.Delay(1200);
            casting = false;
        }

        [PunRPC]
        void RestrictedAreaCastAnimation()
        {
            ability._animator.SetTrigger("GroundSpell");
            castStartFeedbackEvent.Invoke();
        }

        [PunRPC]
        void SpawnRestrictedArea(Team playerTeam, Vector3 position)
        {
            if (playerTeam == Team.RED)
            {
                PhotonNetwork.InstantiateRoomObject(BlueRestrictedAreaPrefab.name, position, Quaternion.identity);
            }
            else {
                PhotonNetwork.InstantiateRoomObject(RedRestrictedAreaPrefab.name, position, Quaternion.identity);
            }
        }

        [PunRPC]
        void RestrictedAreaEndCastAnimation()
        {
            castEndFeedbackEvent.Invoke();
        }
    }
}
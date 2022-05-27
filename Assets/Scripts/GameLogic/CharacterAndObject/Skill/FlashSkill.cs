using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

namespace com.louis.shootball
{
    public class FlashSkill : SkillBase
    {
        public UnityEvent castStartFeedbackEvent;
        public UnityEvent castEndFeedbackEvent;

        public override async void CastSkill()
        {
            await ProcessSkill();
        }
        public override bool IsCastable()
        {
            if (casting)
            {
                return false;
            }
            return true;
        }

        private async UniTask ProcessSkill()
        {
            casting = true;
            photonView.RPC("FlashCastAnimation", RpcTarget.All);
            Vector3 newPos = transform.forward * 5 + transform.position;
            this.GetComponent<Teleporter>().Teleport(newPos, transform.rotation);
            await UniTask.Delay(500);
            casting = false;
            photonView.RPC("FlashEndCastAnimation", RpcTarget.All);
        }

        [PunRPC]
        void FlashCastAnimation()
        {
            castStartFeedbackEvent.Invoke();
        }

        [PunRPC]
        void FlashEndCastAnimation()
        {
            castEndFeedbackEvent.Invoke();
        }
    }
}
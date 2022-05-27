using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

namespace com.louis.shootball
{
    public class SpeedUpSkill : SkillBase
    {
        public float buffDuration = 7f;
        public float buffedSpeed = 10f;
        public UnityEvent castStartFeedbackEvent;
        public UnityEvent buffEndFeedbackEvent;

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
        async UniTask ProcessSkill() {
            casting = true;
            photonView.RPC("SpeedUpPowerUpAnimation", RpcTarget.All);
            ability._thirdPersonController.DisableMovement(1.2f);
            await UniTask.Delay(1200);
            ability._thirdPersonController.SprintSpeedChange(2, (int)(buffDuration * 1000));
            await UniTask.Delay((int)(buffDuration * 1000));
            SpeedUpEnded();
            casting = false;
        }

        void SpeedUpEnded() {
            photonView.RPC("SpeedUpBuffEndRPC", RpcTarget.All);
        }

        [PunRPC]
        void SpeedUpBuffEndRPC()
        {
            buffEndFeedbackEvent.Invoke();
        }        
        
        [PunRPC]
        void SpeedUpPowerUpAnimation()
        {
            ability._animator.SetTrigger("PowerUp");
            castStartFeedbackEvent.Invoke();
        }
    }
}
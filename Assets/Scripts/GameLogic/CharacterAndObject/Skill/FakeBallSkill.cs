using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

namespace com.louis.shootball
{
    public class FakeBallSkill : SkillBase
    {
        public GameObject FakeBallPrefab;
        public MMF_Player MMplayer;
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
            photonView.RPC("FakeBallCastAnimation", RpcTarget.AllViaServer);
            ability._thirdPersonController.DisableMovement(3.3f);
            await UniTask.Delay(1800);
            photonView.RPC("SpawnFakeBall", RpcTarget.MasterClient); 
            photonView.RPC("FakeBallEndCastAnimation", RpcTarget.AllViaServer);
            await UniTask.Delay(1500);
            casting = false;
        }

        [PunRPC]
        void FakeBallCastAnimation()
        {
            ability._animator.SetTrigger("LargeSpell");
            castStartFeedbackEvent.Invoke();
        }

        [PunRPC]
        void SpawnFakeBall() {
            GameObject ball = GameObject.FindGameObjectWithTag("Ball");
            for (int i = 0; i < 10; i++)
            {
                PhotonNetwork.InstantiateRoomObject(FakeBallPrefab.name, ball.transform.position + new Vector3(Random.Range(0.3f,0.5f), Random.Range(0.3f, 0.5f), Random.Range(0.3f, 0.5f)), Quaternion.identity);
            }
        }

        [PunRPC]
        void FakeBallEndCastAnimation()
        {
            GameObject ball = GameObject.FindGameObjectWithTag("Ball");
            MMF_ParticlesInstantiation MMparticle = MMplayer.GetFeedbackOfType<MMF_ParticlesInstantiation>("BallSpawnEffect");
            MMparticle.TargetWorldPosition = ball.transform.position;
            castEndFeedbackEvent.Invoke();
        }

    }
}
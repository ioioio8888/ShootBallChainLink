using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

namespace com.louis.shootball
{
    public class BozoPushSkill : SkillBase
	{
		public LayerMask pushLayers;
		public bool canPush = false;
		public float strength = 50f;
		public float damage = 10f;
		private bool rpcLock = false;
		private float rpcUnlockRate = 0.1f;
		public float buffDuration = 7f;
		public UnityEvent castStartFeedbackEvent;
		public UnityEvent collisionFeedbackEvent;
		public UnityEvent buffEndFeedbackEvent;
		public override async void CastSkill()
		{
			await ProcessSkill();
		}
		public override bool IsCastable()
		{
			if (!ability._thirdPersonController.Grounded) {
				return false;
			}
			if (casting)
			{
				return false;
			}
			return true;
		}
		async UniTask ProcessSkill()
		{
			casting = true;
			photonView.RPC("BozoPowerUpAnimation", RpcTarget.All);
			ability._thirdPersonController.DisableMovement(1.2f);
			await UniTask.Delay(1200);
			canPush = true;
			await UniTask.Delay((int)(buffDuration * 1000));
			canPush = false;
			casting = false;
			photonView.RPC("BozoBuffEndRPC", RpcTarget.All);
		}

		private void OnControllerColliderHit(ControllerColliderHit hit)
		{
			//reject all non player interaction
			if (!photonView.IsMine)
			{
				return;
			}

			if (canPush) PushRigidBodies(hit);
		}

		void UnlockRPC()
		{
			rpcLock = false;
		}

		private void PushRigidBodies(ControllerColliderHit hit)
		{
			if (rpcLock)
			{
				return;
			}

			// make sure it has an impact receiver
			ImpactReceiver receiver = hit.gameObject.GetComponent<ImpactReceiver>();
			if (receiver == null)
			{
				return;
			}

			//calculate direction
			// make sure we only push desired layer(s)
			var bodyLayerMask = 1 << hit.gameObject.layer;
			if ((bodyLayerMask & pushLayers.value) == 0) return;

			// We dont want to push objects below us
			if (hit.moveDirection.y < -0.3f) return;

			Health health = hit.gameObject.GetComponent<Health>();
			if (health != null) {
				health.OnDamage(damage);
			}

			PhotonView view = hit.gameObject.GetComponent<PhotonView>();
			Vector3 pushDir;
			// Calculate push direction from move direction, horizontal motion only
			pushDir = new Vector3(hit.moveDirection.x, 0.3f, hit.moveDirection.z);
			photonView.RPC("BozuPushObject", RpcTarget.AllViaServer, view.ViewID, pushDir, strength);
			rpcLock = true;
			Invoke("UnlockRPC", rpcUnlockRate);
		}

		[PunRPC]
		void BozuPushObject(int id, Vector3 dir, float force)
		{
			PhotonView view = PhotonView.Find(id);
			ImpactReceiver receiver = view.gameObject.GetComponent<ImpactReceiver>();
			receiver.ApplyImpact(dir, force, this.gameObject);
			collisionFeedbackEvent.Invoke();
		}

		[PunRPC]
		void BozoBuffEndRPC()
		{
			buffEndFeedbackEvent.Invoke();
		}

		[PunRPC]
		void BozoPowerUpAnimation()
		{
			ability._animator.SetTrigger("PowerUp");
			castStartFeedbackEvent.Invoke();
		}
	}
}
using UnityEngine;
using Photon.Pun;

namespace com.louis.shootball
{
	public class KickAbility : MonoBehaviourPunCallbacks, IPunObservable
	{
		public LayerMask pushLayers;
		public bool canPush; 
		public float maxForce = 100;
		public float holdTime = 0;
		private FootballInput _input;
		private bool lastKickInput = false;
		private bool lastHighKickInput = false;
		private bool shouldKick = false;
		private bool shouldHighKick = false;
		private bool rpcLock = false;
		private float rpcUnlockRate = 0.1f;
		[SerializeField]
		public float maxHoldTime = 1;
		private Animator _animator;
		private FootballThirdPersonController _thirdPersonController;

		private void Start()
        {
			_input = this.GetComponent<FootballInput>();
			_animator = this.GetComponent<Animator>();
			_thirdPersonController = this.GetComponent<FootballThirdPersonController>();
			ResetKick();
		}

        private void Update()
        {
			if (_input.kick && !_input.highKick)
			{
				holdTime += Time.deltaTime;
				shouldKick = false;
			}
			else {
				if (lastKickInput == true)
				{
					shouldKick = true;
					Invoke("ResetKick", 0.5f);
				}
			}
			lastKickInput = _input.kick;

			if (_input.highKick && !_input.kick)
			{
				holdTime += Time.deltaTime;
				shouldHighKick = false;
			}
			else
			{
				if (lastHighKickInput == true)
				{
					shouldHighKick = true;
					Invoke("ResetKick", 0.5f);
				}
			}
			lastHighKickInput = _input.highKick;
		}

		private void ResetKick() {
			shouldHighKick = false;
			shouldKick = false;
			holdTime = 0;
		}

		private float Remap(float value, float from1, float to1, float from2, float to2)
		{
			return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
		}

		private void OnControllerColliderHit(ControllerColliderHit hit)
		{
			//reject all non player interaction
			if (!photonView.IsMine) {
				return;
			}

			if (canPush) PushRigidBodies(hit);
		}

		void UnlockRPC() {
			rpcLock = false;
		}

		private void PushRigidBodies(ControllerColliderHit hit)
		{
			if (rpcLock) {
				return;
			}

			// make sure it has an impact receiver
			ImpactReceiver receiver = hit.gameObject.GetComponent<ImpactReceiver>();
			if (receiver == null) {
				return;
			}

			//calculate direction
			// make sure we only push desired layer(s)
			var bodyLayerMask = 1 << hit.gameObject.layer;
			if ((bodyLayerMask & pushLayers.value) == 0) return;

			// We dont want to push objects below us
			if (hit.moveDirection.y < -0.3f) return;

			PhotonView view = hit.gameObject.GetComponent<PhotonView>();
			Vector3 pushDir;
			if (shouldKick)
			{
                pushDir = new Vector3(hit.moveDirection.x, 0.3f, hit.moveDirection.z);
                float cappedHoldTime = Mathf.Min(maxHoldTime, holdTime);
                float force = Remap(cappedHoldTime, 0, maxHoldTime, 0, maxForce);
                photonView.RPC("KickAnimation", RpcTarget.AllViaServer, cappedHoldTime);
                photonView.RPC("KickObject", RpcTarget.AllViaServer, view.ViewID, pushDir, force);
                _thirdPersonController.DisableMovement(0.5f);
                ResetKick();
                rpcLock = true;
                Invoke("UnlockRPC", rpcUnlockRate);
            }
            else if (shouldHighKick)
            {
                pushDir = new Vector3(hit.moveDirection.x * 0.8f, 0.7f, hit.moveDirection.z * 0.8f);
                float cappedHoldTime = Mathf.Min(maxHoldTime, holdTime);
				float force = Remap(cappedHoldTime, 0, maxHoldTime, 0, maxForce);
				photonView.RPC("KickAnimation", RpcTarget.AllViaServer, cappedHoldTime);
				photonView.RPC("KickObject", RpcTarget.AllViaServer, view.ViewID, pushDir, force);
				_thirdPersonController.DisableMovement(0.5f);
				ResetKick();
				rpcLock = true;
				Invoke("UnlockRPC", rpcUnlockRate);
			}
		}

		[PunRPC]
		void KickObject(int id, Vector3 dir, float force)
		{
			PhotonView view = PhotonView.Find(id);
			ImpactReceiver receiver = view.gameObject.GetComponent<ImpactReceiver>();
			receiver.ApplyImpact(dir, force, this.gameObject);
		}

		[PunRPC]
		void KickAnimation(float _cappedHoldTime)
		{
			_animator.SetFloat("KickForce", _cappedHoldTime);
			_animator.SetTrigger("Kick");
		}

		#region IPunObservable implementation

		public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if (stream.IsWriting)
			{
				stream.SendNext(holdTime);
			}
			else
			{
				this.holdTime = (float)stream.ReceiveNext();
			}
		}

		#endregion

	}

}
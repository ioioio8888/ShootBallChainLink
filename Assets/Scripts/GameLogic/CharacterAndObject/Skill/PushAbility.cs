using UnityEngine;
using Photon.Pun;

namespace com.louis.shootball
{
	public class PushAbility : MonoBehaviourPunCallbacks
	{
		public LayerMask pushLayers;
		public bool canPush;
		[Range(0.5f, 5f)] public float strength = 1.1f;
		private bool rpcLock = false;
		private float rpcUnlockRate = 0.1f;

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

			PhotonView view = hit.gameObject.GetComponent<PhotonView>();
			Vector3 pushDir;
			// Calculate push direction from move direction, horizontal motion only
			pushDir = new Vector3(hit.moveDirection.x, 0.3f, hit.moveDirection.z);
            photonView.RPC("PushObject", RpcTarget.AllViaServer, view.ViewID, pushDir, strength);
            rpcLock = true;
            Invoke("UnlockRPC", rpcUnlockRate);
        }

		[PunRPC]
		void PushObject(int id, Vector3 dir, float force)
		{
			PhotonView view = PhotonView.Find(id);
			ImpactReceiver receiver = view.gameObject.GetComponent<ImpactReceiver>();
			receiver.ApplyImpact(dir, force, this.gameObject);
		}

	}

}
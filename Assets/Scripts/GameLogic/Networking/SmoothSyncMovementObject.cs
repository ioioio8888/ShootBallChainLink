// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmoothSyncMovement.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Utilities, 
// </copyright>
// <summary>
//  Smoothed out movement for network gameobjects
// </summary>                                                                                             
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

namespace com.louis.shootball
{
    /// <summary>
    /// Smoothed out movement for network gameobjects
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    public class SmoothSyncMovementObject : MonoBehaviourPun, IPunObservable
    {
        public float SmoothingDelay = 5;
        bool sendLock = true;
        public void Awake()
        {
            bool observed = false;
            foreach (Component observedComponent in this.photonView.ObservedComponents)
            {
                if (observedComponent == this)
                {
                    observed = true;
                    break;
                }
            }
            if (!observed)
            {
                Debug.LogWarning(this + " is not observed by this object's photonView! OnPhotonSerializeView() in this class won't be used.");
            }
            Invoke("Unlock", 1);
        }

        void Unlock() {
            sendLock = false;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                //We own this player: send the others our data
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
            }
            else
            {
                //Network player, receive data
                correctPlayerPos = (Vector3)stream.ReceiveNext();
                correctPlayerRot = (Quaternion)stream.ReceiveNext();
            }
        }

        Vector3 correctPlayerPos = Vector3.zero; //We lerp towards this
        Quaternion correctPlayerRot = Quaternion.identity; //We lerp towards this

        public void Update()
        {
            if (!photonView.IsMine)
            {
                //Update remote player (smooth this, this looks good, at the cost of some accuracy)
                transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * this.SmoothingDelay);
                transform.rotation = Quaternion.Lerp(transform.rotation, correctPlayerRot, Time.deltaTime * this.SmoothingDelay);
            }
        }

    }
}
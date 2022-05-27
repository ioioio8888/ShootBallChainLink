using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


namespace com.louis.shootball
{
    public class PlayerSafeNet : MonoBehaviour
    {
        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                if (PhotonView.Get(other).Owner != PhotonNetwork.LocalPlayer)
                {
                    return;
                }
                MovePlayer(other);
            }
        }
        void MovePlayer(Collider other)
        {
            other.GetComponent<Teleporter>().Teleport(Vector3.zero, other.transform.rotation);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

namespace com.louis.shootball
{
    public class PlayerBlocker : MonoBehaviour
    {
        public Team blockTeam;
        public float radius = 10;
        public UnityEvent OnReject;
        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                if (PhotonView.Get(other).Owner != PhotonNetwork.LocalPlayer)
                {
                    return;
                }
                if (blockTeam == Team.RED)
                {
                    if (other.gameObject.layer == LayerMask.NameToLayer("RedPlayer"))
                    {
                        MovePlayer(other);
                    }
                }
                else if (blockTeam == Team.BLUE)
                {
                    if (other.gameObject.layer == LayerMask.NameToLayer("BluePlayer"))
                    {
                        MovePlayer(other);
                    }
                }
            }
        }

        void MovePlayer(Collider other)
        {
            Vector3 dir = (other.transform.position - this.transform.position).normalized;
            dir = new Vector3(dir.x, 0, dir.z);
            Vector3 newPos = dir * (radius + 1) + this.transform.position;
            newPos = new Vector3(newPos.x, 0, newPos.z);
            other.GetComponent<Teleporter>().Teleport(newPos, other.transform.rotation);
            OnReject.Invoke();
        }

    }
}
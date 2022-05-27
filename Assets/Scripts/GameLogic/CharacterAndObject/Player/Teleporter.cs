using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace com.louis.shootball
{
    public class Teleporter : MonoBehaviourPun
    {
        CharacterController _Controller;
        private void Start()
        {
            _Controller = GetComponent<CharacterController>();
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (_Controller != null)
            {
                _Controller.enabled = false;
            }
            this.transform.position = position;
            this.transform.rotation = rotation;

            if (_Controller != null)
            {
                _Controller.enabled = true;
            }
        }
    }
}
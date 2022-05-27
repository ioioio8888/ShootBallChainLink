using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace com.louis.shootball
{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidBodyImpactEffector : MonoBehaviour, IImpactEffector
    {
        protected Rigidbody body;

        protected virtual void Start()
        {
            body = GetComponent<Rigidbody>();
            if (body == null) {
                Debug.LogError(this.gameObject + " has no rigidbody or it is kinematic, Destorying this game Object");
                Destroy(this.gameObject);
            }
        }

        public virtual void AddImpact(Vector3 dir, float force, object source)
        {
            pushObject(dir, force);
        }

        protected void pushObject(Vector3 dir, float force) {
            Rigidbody body = GetComponent<Rigidbody>();
            if (body == null) return;
            // Apply the push and take strength into account
            body.AddForce(dir * force, ForceMode.Impulse);
        }
    }
}

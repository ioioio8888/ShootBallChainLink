using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace com.louis.shootball
{
    public class ImpactReceiver : MonoBehaviour
    {
        public UnityEvent<Vector3, float, object> applyImpactEvent;

        public void ApplyImpact(Vector3 dir, float force, object source) {
            applyImpactEvent.Invoke(dir, force, source);
        }

	}
}
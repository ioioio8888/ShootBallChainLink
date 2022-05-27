using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.louis.shootball
{
    public interface IImpactEffector
    {
        public void AddImpact(Vector3 dir, float force, object source);
    }
}
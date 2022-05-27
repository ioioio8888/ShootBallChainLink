using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.louis.shootball
{
    public interface ISkill
    {
        public void CastSkill();

        public bool IsCastable();
    }
}
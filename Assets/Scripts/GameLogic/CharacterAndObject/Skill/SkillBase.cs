using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace com.louis.shootball
{
    public enum SkillType
    {
        NORMAL,
        STACKED,
        TOGGLE
    }
    public abstract class SkillBase : MonoBehaviourPunCallbacks , ISkill
    {
        public SkillType skillType = SkillType.NORMAL;
        public string SkillName = "";
        public float maxCoolDown = 0;
        public int maxStack = 0;
        public Sprite skillIcon;
        protected SkillAbility ability;
        protected bool casting = false;
        public void SetUp(SkillAbility _ability) {
            ability = _ability;
        }

        public virtual void CastSkill() {

        }


        //check if it castable besides from cooldown
        public virtual bool IsCastable() {
            return true;
        }
    }
}
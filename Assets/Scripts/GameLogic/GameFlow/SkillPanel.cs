using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.louis.shootball
{
    public class SkillPanel : MonoBehaviour
    {
        public static SkillPanel Instance;
        public SkillSlotUI skill1UI;
        public SkillSlotUI skill2UI;
        public GameObject referencePlayer;

        public void SetUpSkill1(SkillAbility _ability) {
            skill1UI.SetUpSkillSlot(_ability);
        }
        public void SetUpSkill2(SkillAbility _ability)
        {
            skill2UI.SetUpSkillSlot(_ability);
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        public void SetUpPanel(GameObject player)
        {
            if (player != null)
            {
                referencePlayer = player;
            }

            SkillAbility[] abilities = referencePlayer.GetComponents<SkillAbility>();
            foreach (var ability in abilities)
            {
                if (ability.Binding == SkillBind.SKILL1)
                {
                    SetUpSkill1(ability);
                }
                else if (ability.Binding == SkillBind.SKILL2)
                {
                    SetUpSkill2(ability);
                }
            }
        }
    }
}
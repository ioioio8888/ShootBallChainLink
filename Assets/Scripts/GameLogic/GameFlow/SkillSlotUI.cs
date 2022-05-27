using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace com.louis.shootball
{
    public class SkillSlotUI : MonoBehaviour
    {
        public Image SkillIconCD;
        public Image SkillIconBG;
        public TMP_Text CoolDownText;
        public GameObject StackedObject;
        public TMP_Text StackedText;
        private SkillAbility ability;
        

        public void SetUpSkillSlot(SkillAbility _ability)
        {
            ability = _ability;
            if (_ability.Skill != null)
            {
                SkillIconBG.sprite = _ability.Skill.skillIcon;
                if (_ability.Skill.skillType == SkillType.STACKED)
                {
                    StackedObject.SetActive(true);
                }
                else
                {
                    StackedObject.SetActive(false);
                }
            }
        }

        public void Update()
        {
            if (ability != null)
            {
                float current = ability.GetCurrentCoolDown();
                float max = ability.GetMaxCoolDown();
                SkillIconCD.fillAmount =  (1 - current / max);
                if ((max - current) > 0)
                {
                    CoolDownText.gameObject.SetActive(true);
                    CoolDownText.text = (max - current).ToString("0.0");
                }
                else {
                    CoolDownText.gameObject.SetActive(false);
                }
                StackedText.text = ability.GetCurrentStack().ToString();
            }
        }
    }
}
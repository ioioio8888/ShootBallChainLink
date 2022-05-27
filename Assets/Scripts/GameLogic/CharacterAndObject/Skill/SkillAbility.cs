using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace com.louis.shootball
{
    public enum SkillBind
    {
        SKILL1,
        SKILL2
    }
    public class SkillAbility : MonoBehaviour
    {
        public SkillBind Binding;
        
        public SkillBase Skill;
        [HideInInspector]
        public Animator _animator;
        [HideInInspector]
        public FootballThirdPersonController _thirdPersonController;
        [HideInInspector]
        public CharacterController _characterController;

        #region Private Fields
        private FootballInput _input;
        private float maxCoolDown = 0;
        private float currentCoolDown = 0;
        private float lastCastTime = 0;
        private bool lastSkillInput = false;
        public float holdTime = 0;
        [SerializeField]
        private int currentStack = 0;
        [SerializeField]
        private int maxStack = 1;
        #endregion
        #region MonoCallBack
        // Start is called before the first frame update
        private void Start()
        {
            _input = this.GetComponent<FootballInput>();
            _animator = this.GetComponent<Animator>();
            _characterController = this.GetComponent<CharacterController>();
            _thirdPersonController = this.GetComponent<FootballThirdPersonController>();
            if (Skill != null)
            {
                Skill.SetUp(this);
                ResetSkill();
            }
        }


        // Update is called once per frame
        void Update()
        {
            if (Skill != null)
            {
                ProcessSkills();
                ProcessCoolDown();
            }
        }

        #endregion

        #region PublicMethod
        public float GetMaxCoolDown() {
            return maxCoolDown;
        }

        public float GetCurrentCoolDown() {
            return currentCoolDown;
        }

        public float GetMaxStack()
        {
            return maxStack;
        }

        public float GetCurrentStack()
        {
            return currentStack;
        }

        #endregion

        #region PrivateMethod
        private void ProcessCoolDown() {
            switch (Skill.skillType) {
                case SkillType.NORMAL:
                    ProcessNormalCoolDown();
                    break;
                case SkillType.STACKED:
                    ProcessStackedCoolDown();
                    break;
                default:
                    ProcessNormalCoolDown();
                    break;
            }
        }

        private void ProcessNormalCoolDown() {
            if (currentCoolDown < maxCoolDown)
            {
                currentCoolDown = (float)PhotonNetwork.Time - lastCastTime;
            }
        }

        private void ProcessStackedCoolDown() {
            if (currentCoolDown < maxCoolDown)
            {
                currentCoolDown = (float)PhotonNetwork.Time - lastCastTime;
            }
            if ((maxStack > currentStack) && (currentCoolDown >= maxCoolDown))
            {
                currentStack++;
                if (maxStack > currentStack)
                {
                    lastCastTime = (float)PhotonNetwork.Time;
                    currentCoolDown = 0;
                }
            }
        }


        private void ProcessSkills() {
            if (_thirdPersonController.movementPermitted)
            {
                if (Binding == SkillBind.SKILL1)
                {
                    ProcessSKill1();
                }
                else if (Binding == SkillBind.SKILL2)
                {
                    ProcessSKill2();
                }
            }
        }

        private void ProcessSKill1() {
            if (_input.skill1)
            {
                holdTime += Time.deltaTime;
            }
            else
            {
                if (lastSkillInput == true && Skill.IsCastable())
                {
                    switch (Skill.skillType)
                    {
                        case SkillType.NORMAL:
                            ProcessNormalSkill();
                            break;
                        case SkillType.STACKED:
                            ProcessStackedSkill();
                            break;
                        default:
                            ProcessNormalSkill();
                            break;
                    }
                }
            }
            lastSkillInput = _input.skill1;
        }

        private void ProcessSKill2()
        {
            if (_input.skill2)
            {
                holdTime += Time.deltaTime;
            }
            else
            {
                if (lastSkillInput == true && Skill.IsCastable()){
                    switch (Skill.skillType)
                    {
                        case SkillType.NORMAL:
                            ProcessNormalSkill();
                            break;
                        case SkillType.STACKED:
                            ProcessStackedSkill();
                            break;
                        default:
                            ProcessNormalSkill();
                            break;
                    }
                }
            }
            lastSkillInput = _input.skill2;
        }

        private void ResetSkill() {
            maxCoolDown = Skill.maxCoolDown;
            currentCoolDown = maxCoolDown;
            maxStack = Skill.maxStack;
            currentStack = maxStack;
            lastCastTime = (float)PhotonNetwork.Time;
        }

        private void ProcessNormalSkill() {
            if (currentCoolDown >= maxCoolDown)
            {
                Skill.CastSkill();
                lastCastTime = (float)PhotonNetwork.Time;
                currentCoolDown = 0;
            }
        }
        private void ProcessStackedSkill()
        {
            if (currentStack > 0)
            {
                Skill.CastSkill();
                //temp work around for cd is restored imediately
                if (currentStack == maxStack) {
                    lastCastTime = (float)PhotonNetwork.Time;
                    currentCoolDown = 0;
                }
                currentStack--;
            }
        }
        #endregion



    }
}
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace com.louis.shootball
{
	public class FootballInput : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool kick;
		public bool highKick;
		public bool skill1;
		public bool skill2;

		[Header("Movement Settings")]
		public bool analogMovement;

#if !UNITY_IOS || !UNITY_ANDROID
        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;
#endif
        public PlayerInput input;


#if ENABLE_INPUT_SYSTEM

		private void Start()
		{
			input = GetComponent<PlayerInput>();
            //TODO, should change to suitable devices
            if (input.inputIsActive)
            {
#if ENABLE_INPUT_SYSTEM && (UNITY_IOS || UNITY_ANDROID)
				input.enabled = false;   
				input.neverAutoSwitchControlSchemes = true;
#endif
				input.SwitchCurrentControlScheme("KeyboardMouse", Keyboard.current, Mouse.current);
            }
        }

        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

		public void OnLook(InputValue value)
		{
			if (cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnKick(InputValue value)
		{
			KickInput(value.isPressed);
		}

		public void OnHighKick(InputValue value)
		{
			HighKickInput(value.isPressed);
		}

		public void OnSkill1(InputValue value)
		{
			Skill1Input(value.isPressed);
		}

		public void OnSkill2(InputValue value)
		{
			Skill2Input(value.isPressed);
		}
#else
				// old input sys if we do decide to have it (most likely wont)...
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		}

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		public void KickInput(bool newKickState)
		{
			kick = newKickState;
		}

		public void HighKickInput(bool newKickState)
		{
			highKick = newKickState;
		}

		public void Skill1Input(bool newSkill1State)
		{
			skill1 = newSkill1State;
		}
		public void Skill2Input(bool newSkill2State)
		{
			skill2 = newSkill2State;
		}
	}

}
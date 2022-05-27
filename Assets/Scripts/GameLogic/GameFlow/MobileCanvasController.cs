using UnityEngine;
using Photon.Pun;

namespace com.louis.shootball
{
    public class MobileCanvasController : MonoBehaviour
    {

        [Header("Output")]
        public FootballInput footballInputs = null;
        public bool testMobile = false;
        void Start()
        {
#if ENABLE_INPUT_SYSTEM && (UNITY_IOS || UNITY_ANDROID)
            gameObject.SetActive(true);
#else
            if (!testMobile)
            {
                gameObject.SetActive(false);
            }
#endif
        }


        void Update()
        {
            if (footballInputs == null)
            {
                SearchForPlayer();
            }
        }

        private void SearchForPlayer() {
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (PhotonView.Get(player).Owner == PhotonNetwork.LocalPlayer)
                {
                    footballInputs = player.GetComponent<FootballInput>();
                }
            }
        }

        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            footballInputs.MoveInput(virtualMoveDirection);
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            footballInputs.LookInput(virtualLookDirection);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            footballInputs.JumpInput(virtualJumpState);
        }

        public void VirtualSprintInput(bool virtualSprintState)
        {
            footballInputs.SprintInput(virtualSprintState);
        }
        public void VirtualKickInput(bool virtualKickState)
        {
            footballInputs.KickInput(virtualKickState);
        }
        public void VirtualHighKickInput(bool virtualHighKickState)
        {
            footballInputs.HighKickInput(virtualHighKickState);
        }

        public void VirtualSKill1Input(bool virtualHighKickState)
        {
            footballInputs.Skill1Input(virtualHighKickState);
        }
        public void VirtualSKill2Input(bool virtualHighKickState)
        {
            footballInputs.Skill2Input(virtualHighKickState);
        }

    }

}

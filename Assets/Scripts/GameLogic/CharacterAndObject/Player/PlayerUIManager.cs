using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

namespace com.louis.shootball
{
    public class PlayerUIManager : MonoBehaviour
    {
        #region Private Fields
        private GameObject target;
        private Team team = Team.RED;
        [Tooltip("UI Text to display Player's Name")]
        [SerializeField]
        private TMP_Text playerNameText;
        [Tooltip("UI Slider to display Player's Health")]
        [SerializeField]
        private Image playerHealthImage;
        [SerializeField]
        private Image playerForceImage;
        [SerializeField]
        private GameObject BlueTeamTag;
        [SerializeField]
        private GameObject RedTeamTag;

        private KickAbility ballInteract;
        private Health health;
        #endregion

        // Update is called once per frame
        void Update()
        {

            transform.forward = Camera.main.transform.forward;

            // Reflect the Player Health
            if (playerHealthImage != null)
            {
                playerHealthImage.fillAmount = health.GetCurrentHealth() / health.GetMaxHealth();
            }
            // Reflect the Player Health
            if (playerForceImage != null)
            {
                float filled = Mathf.Min(2, ballInteract.holdTime);

                playerForceImage.fillAmount = filled / ballInteract.maxHoldTime;
            }
            if (team == Team.RED)
            {
                BlueTeamTag.SetActive(false);
                RedTeamTag.SetActive(true);
            }
            else if (team == Team.BLUE)
            {
                BlueTeamTag.SetActive(true);
                RedTeamTag.SetActive(false);
            }


            // Destroy itself if the target is null, It's a fail safe when Photon is destroying Instances of a Player over the network
            if (target == null)
            {
                Destroy(this.gameObject);
                return;
            }

        }


        #region Public Methods

        public void SetTarget(GameObject _target, Team _team)
        {
            if (_target == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> PlayMakerManager target for PlayerUI.SetTarget.", this);
                return;
            }
            // Cache references for efficiency
            target = _target;
            if (playerNameText != null)
            {
                if (PhotonView.Get(_target).Owner != null)
                {
                    playerNameText.text = PhotonView.Get(_target).Owner.NickName;
                }
                else {
                    playerNameText.text = "-";
                }
            }
            team = _team;
            ballInteract = target.GetComponent<KickAbility>();
            health = target.GetComponent<Health>();
        }
        #endregion
    }
}
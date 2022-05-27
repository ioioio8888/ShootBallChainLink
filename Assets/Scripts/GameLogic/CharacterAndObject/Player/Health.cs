using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Cysharp.Threading.Tasks;

namespace com.louis.shootball
{
    public class Health : MonoBehaviourPun, IPunObservable
    {
        #region private fields

        [SerializeField]
        private float health = 100f;
        [SerializeField]
        private float maxHealth = 100f;
        private Animator _anim;
        private FootballThirdPersonController _ctrl;
        #endregion

        #region public fields
        public UnityEvent<GameObject> OnDeathEvents;
        public UnityEvent<GameObject> OnReviveEvents;

        public UnityEvent OnDeathFeedbackEvents;
        public UnityEvent OnReviveFeedbackEvents;
        public UnityEvent OnHealEvents;
        public bool alive = true;
        #endregion

        #region MonoCallbacks

        public void Start()
        {
            _anim = GetComponent<Animator>();
            _ctrl = GetComponent<FootballThirdPersonController>();
        }
        public void Update()
        {
            if (photonView.Owner == PhotonNetwork.LocalPlayer)
            {
                if (health <= 0 & alive)
                {
                    alive = false;
                    OnDeathEvents.Invoke(this.gameObject);
                    photonView.RPC("DieFeedBack", RpcTarget.AllBuffered);
                }
                if (health <= 0)
                {
                    //keep the character unable to move when dead
                    _ctrl.DisableMovement();
                }
                if (health > 0 & !alive)
                {
                    alive = true;
                    OnReviveEvents.Invoke(this.gameObject);
                    photonView.RPC("HealFeedBack", RpcTarget.AllBuffered);
                }
                if (alive)
                {
                    _anim.SetBool("Death", false);
                }
                else
                {
                    _anim.SetBool("Death", true);
                }
            }
        }

        #endregion

        #region publicMethods
        public async void ReviveAfterSeconds(int seconds) {
            await UniTask.Delay(seconds * 1000);
            health = maxHealth;
        }

        public float GetCurrentHealth() {
            return health;
        }

        public float GetMaxHealth()
        {
            return maxHealth;
        }


        public void OnDamage(float amount) {
            if (amount < 0 && alive)
            {
                Debug.LogError("Cant Damage a negative number");
                return;
            }
            photonView.RPC("DamageRPC", photonView.Owner, amount);
        }

        public void OnHeal(float amount)
        {
            if (amount < 0 && alive)
            {
                Debug.LogError("Cant Heal a negative number");
                return;
            }
            photonView.RPC("HealRPC", photonView.Owner, amount);
        }
        #endregion


        #region PUNrpc
        [PunRPC]
        public void DamageRPC(float amount) {
            if (amount < 0 && alive)
            {
                Debug.LogError("Cant Damage a negative number");
                return;
            }
            health -= amount;
            if (health < 0) {
                health = 0;
            }
        }

        [PunRPC]
        public void HealRPC(float amount)
        {
            if (amount < 0 && alive)
            {
                Debug.LogError("Cant Heal a negative number");
                return;
            }
            photonView.RPC("HealFeedBack", RpcTarget.AllBuffered);
            OnHealEvents.Invoke();
            health += amount;
            if (health > maxHealth) {
                health = maxHealth;
            }
        }


        [PunRPC]
        public void DieFeedBack() {
            OnDeathFeedbackEvents.Invoke();
        }
        [PunRPC]
        public void HealFeedBack()
        {
            OnReviveFeedbackEvents.Invoke();
        }
        #endregion

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(health);
            }
            else
            {
                this.health = (float)stream.ReceiveNext();
            }
        }

        #endregion
    }
}
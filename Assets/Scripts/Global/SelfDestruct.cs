using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Cysharp.Threading.Tasks;

namespace com.louis.shootball
{
    public class SelfDestruct : MonoBehaviourPun
    {
        public float selfDestructTime = 10f;
        public UnityEvent destructEvent;

        // Start is called before the first frame update
        async void Start()
        {
            await SelfDestrcut();
        }

        public async UniTask SelfDestrcut() {
            await UniTask.Delay((int)(selfDestructTime*1000));
            destructEvent.Invoke();
            if (photonView.Owner == PhotonNetwork.LocalPlayer)
            {
                PhotonNetwork.Destroy(this.gameObject);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;

namespace com.louis.shootball
{
    public class RoomPlayerUIManager : MonoBehaviourPunCallbacks
    {
        public TMP_Text PlayerName;
        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;
        public GameObject MasterBadge;
        public Image BackGround;
        public Image PlayerIcon;
        public ChampionList championList;

        // Start is called before the first frame update
        void Start()
        {
            PlayerName.text = PhotonView.Get(this).Owner.NickName;
           
        }

        void Update()
        {
            if (SceneManager.GetActiveScene().name != "Room")
            {
                Destroy(this.gameObject);
                return;
            }

            PhotonTeam team = PhotonView.Get(this).Owner.GetPhotonTeam();
            if (team != null)
            {
                if (team.Name == "Red")
                {
                    if (transform.parent != RoomUIManager.Instance.RedRoot)
                    {
                        transform.SetParent(RoomUIManager.Instance.RedRoot);
                        transform.localScale = Vector3.one;
                        BackGround.color = Color.red;
                        PlayerIcon.transform.localScale = new Vector3(1, 1, 1);
                    }
                }
                else if (team.Name == "Blue")
                {
                    if (transform.parent != RoomUIManager.Instance.BlueRoot)
                    {
                        transform.SetParent(RoomUIManager.Instance.BlueRoot);
                        transform.localScale = Vector3.one;
                        BackGround.color = Color.blue;
                        PlayerIcon.transform.localScale = new Vector3(-1, 1, 1);
                    }
                }
            }
            if (PhotonView.Get(this).Owner.IsMasterClient)
            {
                MasterBadge.SetActive(true);
            }
            else
            {
                MasterBadge.SetActive(false);
            }
            UpdateChampionIcon();
        }

        private void UpdateChampionIcon() {
            Hashtable ht = photonView.Owner.CustomProperties;
            if (ht.ContainsKey("Champion"))
            {
                int selectedID = (int)ht["Champion"];
                foreach (var champion in championList.Champions)
                {
                    if (champion.ChampionID == selectedID)
                    {
                        PlayerIcon.sprite = champion.ChampionIcon;
                    }
                }
            }
            else {
                PlayerIcon.sprite = null;
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            if (otherPlayer.ActorNumber == this.photonView.CreatorActorNr) {
                PhotonNetwork.Destroy(this.gameObject);
            }
        }

    }
}

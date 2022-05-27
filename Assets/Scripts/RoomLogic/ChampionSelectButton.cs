using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using ExitGames.Client.Photon;

namespace com.louis.shootball
{
    public class ChampionSelectButton : MonoBehaviour
    {
        public Image ChampionIcon;
        public TMP_Text ChampionNameText;
        public ChampionObject Champion;
        public int ChampionID;
        public GameObject AttackerTag;
        public GameObject DefendererTag;
        public GameObject SupporterTag;
        public Image[] frame;
        public Color attackerColor;
        public Color defenderColor;
        public Color supporterColor;

        public void SetChampion(ChampionObject champion) {
            ChampionID = champion.ChampionID;
            ChampionIcon.sprite = champion.ChampionIcon;
            ChampionNameText.text = champion.ChampionName;
            switch (champion.ChampionType) {
                case ChampionType.ATTACKER:
                    AttackerTag.SetActive(true);
                    DefendererTag.SetActive(false);
                    SupporterTag.SetActive(false);
                    foreach (var item in frame)
                    {
                        item.color = attackerColor;
                    }
                    break;
                case ChampionType.DEFENDER:
                    AttackerTag.SetActive(false);
                    DefendererTag.SetActive(true);
                    SupporterTag.SetActive(false);
                    foreach (var item in frame)
                    {
                        item.color = defenderColor;
                    }
                    break;
                case ChampionType.SUPPORTER:
                    AttackerTag.SetActive(false);
                    DefendererTag.SetActive(false);
                    SupporterTag.SetActive(true);
                    foreach (var item in frame)
                    {
                        item.color = supporterColor;
                    }
                    break;
            }
            Champion = champion;
        }

        public void OnChampionSelectButtonClicked() {
            Hashtable ht = new Hashtable();
            ht.Add("Champion", ChampionID);
            PhotonNetwork.LocalPlayer.SetCustomProperties(ht);
        }
    }
}
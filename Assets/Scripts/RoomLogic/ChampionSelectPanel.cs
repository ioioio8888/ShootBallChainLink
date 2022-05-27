using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.louis.shootball
{
    public class ChampionSelectPanel : MonoBehaviour
    {
        public Transform selectPanelRoot;
        public ChampionList championList;
        public ChampionSelectButton buttonPrefab;
        // Start is called before the first frame update
        private void Start()
        {
            SetUpChampionSelectPanel();
        }

        private void SetUpChampionSelectPanel() {
            foreach (ChampionObject champion in championList.Champions)
            {
                ChampionSelectButton button = Instantiate(buttonPrefab, selectPanelRoot);
                button.SetChampion(champion);
            }
        }
    }
}
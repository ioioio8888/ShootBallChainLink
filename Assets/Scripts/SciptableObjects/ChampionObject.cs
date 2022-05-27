using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace com.louis.shootball
{
    public enum ChampionType { 
        ATTACKER,
        SUPPORTER,
        DEFENDER
    }

    [CreateAssetMenu(fileName = "Champion", menuName = "ShootBall/ChampionObject", order = 1)]
    public class ChampionObject : ScriptableObject
    {
        public int ChampionID = 0;
        public ChampionType ChampionType;
        public string ChampionName = "Default Champion";
        public Sprite ChampionIcon;
        public GameObject ChampionInGamePlayer;
    }
}
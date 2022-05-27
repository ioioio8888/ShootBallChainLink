using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.louis.shootball
{
    [CreateAssetMenu(fileName = "Championlist", menuName = "ShootBall/ChampionList", order = 1)]
    public class ChampionList : ScriptableObject
    {
        public List<ChampionObject> Champions = new List<ChampionObject>();
    }
}
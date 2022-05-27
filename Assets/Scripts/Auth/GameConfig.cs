using System.Collections.Generic;

namespace com.louis.shootball
{
    public class GameConfig
    {
        public static byte maxPlayersPerRoom = 6;
        public static byte ThreeVsThreeMode = 6;
        public static byte TwoVsTwoMode = 4;
        public static byte OneVsOneMode = 2;
        public static string gameVersion = "2";
        public static string isQuickModeKey = "q";
        public static string GameModeKey = "m";
        public static string[] regionsCode = new string[15] {"","asia","au","cae","eu","in","jp","ru","rue","za","sa","kr","tr","us","usw"};
        public static string[] regionsName = new string[15] { "Auto Detect", "Asia", "Australia", "Canada", "Europe", "India", "Japan", "Russia", "Russia, East", "South Africa", "South America", "South Korea", "Turkey", "USA, East", "USA, West" };
    }
}
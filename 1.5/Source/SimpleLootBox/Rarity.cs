using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SimpleLootBox
{
    public enum Rarity : int
    {
        None = 0,
        Common = 1,
        Uncommon = 2,
        Rare = 3,
        Epic = 4,
        Legendary = 5,
    }
    public static class RarityExtensions
    {
        public static string TranslateLabel(this Rarity rarity)
        {
            return ("SimpleLootBox_Rarity_" + rarity.ToString()).Translate();
        }
    }
}

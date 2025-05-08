using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimpleLootBox
{
    public static class RarityColors
    {
        public static Dictionary<Rarity, Color> rarityColorDict = new Dictionary<Rarity, Color>
    {
        { Rarity.None, new Color(0.6f, 0.6f, 0.6f) },
        { Rarity.Common, new Color(0.3f, 0.3f, 1f) },
        { Rarity.Uncommon, new Color(0.8f, 0.3f, 1f) },
        { Rarity.Rare, new Color(1f, 0.3f, 0.8f) },
        { Rarity.Epic, new Color(1f, 0.3f, 0.3f) },
        { Rarity.Legendary, new Color(0.85f, 0.7f, 0.2f) }
    };

        public static Color GetColor(Rarity rarity)
        {
            return rarityColorDict.TryGetValue(rarity, out var color) ? color : Color.white;
        }
    }
}

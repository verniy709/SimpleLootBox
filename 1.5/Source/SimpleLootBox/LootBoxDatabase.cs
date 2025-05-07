using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SimpleLootBox
{
    public static class LootBoxDatabase
    {
        public static List<LootBox> allLootBoxes = new List<LootBox>();

        static LootBoxDatabase()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                var props = def.GetCompProperties<CompProperties_SpawnLootBox>();
                if (props?.lootBoxList != null)
                {
                    foreach (var lootBox in props.lootBoxList)
                    {
                        allLootBoxes.Add(lootBox);
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace SimpleLootBox
{
    public class CompProperties_SpawnLootBox : CompProperties
    {
        public List<LootBox> lootBoxList;

        public CompProperties_SpawnLootBox()
        {
            this.compClass = typeof(SpawnCompLootBox);
        }
    }
}

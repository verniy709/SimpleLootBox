using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace SimpleLootBox
{
    public class CompProperties_LootBox : CompProperties_UseEffect
    {
        public CompProperties_LootBox()
        {
            this.compClass = typeof(CompLootBox);
        }

        public List<LootBoxThingDef> lootBoxThingDef;

        public List<LootBoxPawnKindDef> lootBoxPawnKindDef;

        public SoundDef lootBoxOpenSound;

        public SoundDef lootBoxSpinningSound;

        public SoundDef lootBoxBackgroundMusicSound;

    }
}

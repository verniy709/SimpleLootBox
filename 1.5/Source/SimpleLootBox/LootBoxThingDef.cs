using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace SimpleLootBox
{
    public class LootBoxThingDef
    {
        public ThingDef thingDef;

        public float weight = 1f;

        public int count = 1;

        public ThingDef stuff;

        public QualityCategory quality;

        public Rarity None;
    }
}

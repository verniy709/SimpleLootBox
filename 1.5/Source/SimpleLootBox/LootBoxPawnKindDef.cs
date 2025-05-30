﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace SimpleLootBox
{
    public class LootBoxPawnKindDef
    {
        public PawnKindDef pawnKindDef;

        public float weight = 1f;

        public int count = 1;

        public EffecterDef effecterDef;

        public Rarity rarity = 0;

        public SoundDef lootBoxFinalizingRewardSound;

        public bool isHostile;

        public bool isHidden;

    }
}

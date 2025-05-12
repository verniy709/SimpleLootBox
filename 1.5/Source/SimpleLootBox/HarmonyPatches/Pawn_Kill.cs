using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SimpleLootBox.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class Pawn_Kill_Patch
    {
        static void Postfix(Pawn __instance)
        {
            if (!__instance.SpawnedOrAnyParentSpawned) 
                return;

            foreach (var lootBox in LootBoxDatabase.allLootBoxes)
            {
                if (Rand.Value <= lootBox.chance)
                {
                    Thing thing = ThingMaker.MakeThing(lootBox.thingDef);
                    GenPlace.TryPlaceThing(thing, __instance.PositionHeld, __instance.MapHeld, ThingPlaceMode.Near);
                }
            }
        }
    }
}

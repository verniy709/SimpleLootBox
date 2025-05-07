using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace SimpleLootBox
{
    public class CompLootBox : CompUseEffect
    {
        public CompProperties_LootBox Props => (CompProperties_LootBox)props;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);

            float thingWeightSum = 0f;
            float pawnWeightSum = 0f;

            if (Props.lootBoxThingDef != null)
                thingWeightSum = Props.lootBoxThingDef
                    .Where(t => t.thingDef != null && t.weight > 0)
                    .Sum(t => t.weight);

            if (Props.lootBoxPawnKindDef != null)
                pawnWeightSum = Props.lootBoxPawnKindDef
                    .Where(t => t.pawnKindDef != null && t.weight > 0)
                    .Sum(t => t.weight);

            float totalWeight = thingWeightSum + pawnWeightSum;

            if (totalWeight <= 0f)
            {
                Log.Warning("SimpleLootBox: No valid thingDef or pawnKindDef.");
            }
            else
            {
                float roll = Rand.Value * totalWeight;
                if (roll < thingWeightSum)
                {
                    SpawnRandomThing();
                }
                else
                {
                    SpawnRandomPawn();
                }
            }

            DeleteBox(1);

        }

        private void DeleteBox(int count)
        {
            if (parent.stackCount > count)
            {
                parent.stackCount -= count;
            }
            else
            {
                parent.Destroy(DestroyMode.Vanish);
            }
        }

        private void PlayEffect(EffecterDef effecterDef, IntVec3 position, Map map)
        {
            if (effecterDef != null)
            {
                Effecter effecter = effecterDef.Spawn();
                effecter.Trigger(new TargetInfo(position, map), new TargetInfo(position, map));
                effecter.Cleanup();
            }
        }

        private void SpawnRandomThing()
        {
            if (Props.lootBoxThingDef == null)
            {
                Log.Message("SimpleLootBox: No lootBoxThingDef in the reward list.");
                return;
            }

            var validThings = Props.lootBoxThingDef
                .Where(t => t.thingDef != null && t.weight > 0)
                .ToList();

            if (validThings.Count == 0)
            {
                Log.Message("SimpleLootBox: No valid thingDef for the lootbox.");
                return;
            }

            var selectedThing = validThings.RandomElementByWeight(t => t.weight);

            for (int i = 0; i < selectedThing.count; i++)
            {
                Thing thing;

                if (selectedThing.thingDef.MadeFromStuff && selectedThing.stuff != null)
                {
                    thing = ThingMaker.MakeThing(selectedThing.thingDef, selectedThing.stuff);
                }
                else
                {
                    thing = ThingMaker.MakeThing(selectedThing.thingDef);
                }

                if (thing.TryGetComp<CompQuality>() != null)
                {
                    thing.TryGetComp<CompQuality>().SetQuality(selectedThing.quality, ArtGenerationContext.Outsider);
                }

                if (thing.def.Minifiable)
                {
                    Thing minifiedThing = MinifyUtility.MakeMinified(thing);
                    GenPlace.TryPlaceThing(minifiedThing, parent.Position, parent.Map, ThingPlaceMode.Near);
                }
                else
                {
                    GenPlace.TryPlaceThing(thing, parent.Position, parent.Map, ThingPlaceMode.Near);
                }
            }

            PlayEffect(selectedThing.effecterDef, parent.Position, parent.Map);
        }

        private void SpawnRandomPawn()
        {
            if (Props.lootBoxPawnKindDef == null)
            {
                Log.Message("SimpleLootBox: No lootBoxPawnKindDef in the reward list.");
                return;
            }

            var validPawns = Props.lootBoxPawnKindDef
                .Where(t => t.pawnKindDef != null && t.weight > 0)
                .ToList();

            if (validPawns.Count == 0)
            {
                Log.Message("SimpleLootBox: No valid pawnKind for the lootbox.");
                return;
            }

            var selectedPawn = validPawns.RandomElementByWeight(t => t.weight);

            for (int i = 0; i < selectedPawn.count; i++)
            {
                PawnGenerationRequest request = new PawnGenerationRequest(
                    kind: selectedPawn.pawnKindDef,
                    context: PawnGenerationContext.NonPlayer, 
                    canGeneratePawnRelations: false,
                    colonistRelationChanceFactor: 0f,
                    forceGenerateNewPawn: true
                );
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                pawn.SetFaction(Faction.OfPlayer);
                if (pawn.ideo != null)
                {
                    pawn.ideo.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
                }

                GenPlace.TryPlaceThing(pawn, parent.Position, parent.Map, ThingPlaceMode.Near);
            }

            PlayEffect(selectedPawn.effecterDef, parent.Position, parent.Map);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace SimpleLootBox
{
    public class CompLootBox : CompUseEffect
    {
        public CompProperties_LootBox Props => (CompProperties_LootBox)props;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            LootBoxOpeningWindow window = new LootBoxOpeningWindow(this);
            Find.WindowStack.Add(window);
        }

        public bool Spawn(LootBoxSpinner.SpinItem reward)
        {
            if (reward.count <= 0) 
                return false;

            Map map = parent.Map;
            IntVec3 position = parent.Position;

            if (reward.thingDef != null)
            {
                for (int i = 0; i < reward.count; i++)
                {
                    Thing thing;
                    if (reward.thingDef.MadeFromStuff)
                    {
                        ThingDef overriddenStuff = reward.stuff;

                        if (overriddenStuff == null)
                        {
                            var allStuffCategories = reward.thingDef.stuffCategories;

                            if (allStuffCategories != null)
                            {
                                var validStuffs = DefDatabase<ThingDef>.AllDefsListForReading
                                    .Where(def => def.IsStuff && def.stuffProps?.categories != null &&
                                                  def.stuffProps.categories.Any(c => allStuffCategories.Contains(c)))
                                    .ToList();

                                overriddenStuff = validStuffs.RandomElement();
                            }
                        }

                        thing = ThingMaker.MakeThing(reward.thingDef, overriddenStuff);
                    }
                    else
                    {
                        thing = ThingMaker.MakeThing(reward.thingDef);
                    }

                    CompQuality compQuality = thing.TryGetComp<CompQuality>();
                    if (compQuality != null)
                    {
                        var overriddenQuality = Props.lootBoxThingDef?.FirstOrDefault(t => t.thingDef == reward.thingDef);
                        QualityCategory finalQuality = overriddenQuality?.quality.HasValue == true
                            ? overriddenQuality.quality.Value
                            : QualityUtility.GenerateQualityRandomEqualChance();
                        compQuality.SetQuality(finalQuality, ArtGenerationContext.Outsider);
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

                    if (reward.effecterDef != null)
                    {
                        Effecter effecter = reward.effecterDef.Spawn();
                        effecter.Trigger(new TargetInfo(position, map), new TargetInfo(position, map));
                        effecter.Cleanup();
                    }
                }
            }
            else if (reward.pawnKindDef != null)
            {
                for (int i = 0; i < reward.count; i++)
                {
                    PawnGenerationRequest request = new PawnGenerationRequest(
                        kind: reward.pawnKindDef,
                        context: PawnGenerationContext.NonPlayer,
                        canGeneratePawnRelations: false,
                        colonistRelationChanceFactor: 0f,
                        forceGenerateNewPawn: true,
                        //Because people really want to spawn baby using the loot box
                        developmentalStages: reward.pawnKindDef.pawnGroupDevelopmentStage ?? DevelopmentalStage.Adult,
                        allowDowned: true
                    );
                    Pawn pawn = PawnGenerator.GeneratePawn(request);

                    if (reward.isHostile)
                    {
                        //Faction enemy = Find.FactionManager.AllFactionsVisible
                        //    .Where(f => !f.IsPlayer && f.HostileTo(Faction.OfPlayer) && f.def.humanlikeFaction)
                        //    .RandomElementWithFallback();
                        Faction faction = null;
                        pawn.SetFaction(faction);

                        if (pawn.RaceProps.Animal)
                        {
                            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, forced: true);
                        }
                        else if (pawn.RaceProps.Humanlike)
                        {
                            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, forced: true);
                        }
                        else if (pawn.RaceProps.IsMechanoid)
                        {
                            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.BerserkMechanoid, forced: true);
                        }
                        else if (pawn.RaceProps.IsAnomalyEntity)
                        {
                            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, forced: true);
                        }
                    }
                    else
                    {
                        pawn.SetFaction(Faction.OfPlayer);
                        if (pawn.ideo != null)
                        {
                            pawn.ideo.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
                        }
                    }

                    GenSpawn.Spawn(pawn, position, map, WipeMode.Vanish);

                    if (reward.effecterDef != null)
                    {
                        Effecter effecter = reward.effecterDef.Spawn();
                        effecter.Trigger(new TargetInfo(position, map), new TargetInfo(position, map));
                        effecter.Cleanup();
                    }
                }
            }
            return true;
        }

        public void DeleteBox(int count)
        {
            if (parent.stackCount > count)
            {
                parent.stackCount -= count;
            }
            else if (parent.stackCount == count)
            {
                parent.Destroy(DestroyMode.Vanish);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using SimpleLootBox.SimpleLootBox;
using Verse;

namespace SimpleLootBox
{
    public class GameComponent_SpawnLootBox : GameComponent
    {
        private Dictionary<string, int> nextSpawnTick = new Dictionary<string, int>();

        public GameComponent_SpawnLootBox(Game game) : base() { }

        public override void GameComponentTick()
        {
            if (Current.Game?.World?.worldObjects == null || Find.TickManager.TicksGame % 250 != 0)
                return;

            int currentTick = Find.TickManager.TicksGame;

            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.HasComp(typeof(CompSpawnLootBox)))
                {
                    var spawnComp = def.GetCompProperties<CompProperties_SpawnLootBox>();
                    if (spawnComp?.lootBoxList == null) continue;

                    foreach (var lootBox in spawnComp.lootBoxList)
                    {
                        if (lootBox.thingDef == null || lootBox.daysBetweenLootBoxSpawns <= 0) continue;

                        string key = def.defName + "_" + lootBox.thingDef.defName;

                        if (!nextSpawnTick.TryGetValue(key, out int tick))
                        {
                            tick = currentTick + (int)(lootBox.daysBetweenLootBoxSpawns * 60000);
                            nextSpawnTick[key] = tick;
                        }

                        if (currentTick >= tick)
                        {
                            Spawn(lootBox.thingDef);
                            nextSpawnTick[key] = currentTick + (int)(lootBox.daysBetweenLootBoxSpawns * 60000);
                        }
                    }
                }
            }
        }

        private void Spawn(ThingDef def)
        {
            Map map = Find.AnyPlayerHomeMap;
            if (map == null) return;

            IntVec3 pos = DropCellFinder.TradeDropSpot(map);
            Thing thing = ThingMaker.MakeThing(def);
            GenPlace.TryPlaceThing(thing, pos, map, ThingPlaceMode.Near);

            Messages.Message("SimpleLootBox_ThingArrived".Translate(def.label.CapitalizeFirst()),
                new TargetInfo(pos, map), MessageTypeDefOf.PositiveEvent);
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref nextSpawnTick, "SimpleLootBox_nextSpawnTick", LookMode.Value, LookMode.Value);
        }
    }
}

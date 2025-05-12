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
        private bool initialized;
        private bool shouldTick;
        public GameComponent_SpawnLootBox(Game game) : base() { }

        public void CheckLootBoxForTick()
        {
            shouldTick = LootBoxDatabase.allLootBoxes?.Any(lb => lb.lootBoxSpawnGroup != null) ?? false;

            if (!shouldTick)
            {
                Log.Message("SimpleLootBox: No loot box found with a valid lootBoxSpawnGroup. No loot box spawn.");
            }
        }

        public override void GameComponentTick()
        {
            if (!initialized)
            {
                initialized = true;
                CheckLootBoxForTick();
            }

            if (!shouldTick || Find.TickManager.TicksGame % 250 != 0)
                return;

            var allLootBoxes = LootBoxDatabase.allLootBoxes;
            if (allLootBoxes == null || Current.Game?.World?.worldObjects == null)
                return;

            int currentTick = Find.TickManager.TicksGame;

            foreach (var group in allLootBoxes
                         .Where(lootBox => lootBox.lootBoxSpawnGroup != null)
                         .GroupBy(lootBox => lootBox.lootBoxSpawnGroup))
            {
                var lootBoxGroup = group.ToList();
                if (lootBoxGroup.Count == 0)
                    continue;


                float firstInterval = lootBoxGroup[0].daysBetweenLootBoxSpawns;
                if (lootBoxGroup.Any(lb => lb.daysBetweenLootBoxSpawns != firstInterval))
                {
                    Log.Warning($"[SimpleLootBox] Inconsistent 'daysBetweenLootBoxSpawns' in group '{group.Key}': " +
                                string.Join(", ", lootBoxGroup.Select(lb => $"{lb.thingDef.defName}({lb.daysBetweenLootBoxSpawns})")));
                }

                string groupKey = $"Group_{group.Key}";
                float intervalDays = lootBoxGroup.Max(lb => lb.daysBetweenLootBoxSpawns);
                int intervalTicks = (int)(intervalDays * 60000);
                int lastSpawnTick = (Find.TickManager.TicksGame / intervalTicks) * intervalTicks;

                if (!nextSpawnTick.TryGetValue(groupKey, out int nextTick) || nextTick < lastSpawnTick + intervalTicks)
                {
                    var selected = lootBoxGroup.RandomElementByWeight(lb => lb.weightInGroup);
                    Spawn(selected.thingDef);
                    nextSpawnTick[groupKey] = lastSpawnTick + intervalTicks;
                }
            }
        }

        private void Spawn(ThingDef def)
        {
            Map map = Find.AnyPlayerHomeMap;
            if (map == null) return;
            IntVec3 pos = DropCellFinder.TradeDropSpot(map);
            Thing thing = ThingMaker.MakeThing(def);
            ActiveDropPodInfo podInfo = new ActiveDropPodInfo();
            podInfo.openDelay = 150;
            podInfo.leaveSlag = false;
            podInfo.innerContainer.TryAdd(thing);
            DropPodUtility.MakeDropPodAt(pos, map, podInfo);
            Messages.Message("SimpleLootBox_ThingArrived".Translate(def.label.CapitalizeFirst()),
                new TargetInfo(pos, map), MessageTypeDefOf.PositiveEvent);
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref nextSpawnTick, "SimpleLootBox_nextSpawnTick", LookMode.Value, LookMode.Value);
        }
    }
}

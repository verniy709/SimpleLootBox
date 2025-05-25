using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;

namespace SimpleLootBox
{
    public class LootBoxOpeningWindow : Window
    {
        private static Vector2 scrollPosition = Vector2.zero;
        private readonly CompLootBox compLootBox;
        private readonly LootBoxSpinner lootBoxSpinner;
        private Sustainer backgroundMusicSustainer;
        private Sustainer spinningSustainer;
        private Texture2D backgroundTex;
        private LootBoxSpinner.SpinItem? pendingFinalizingSoundItem = null;
        private LootBoxSpinner.SpinItem? pendingRewardItem = null;

        public LootBoxOpeningWindow(CompLootBox comp)
        {
            this.compLootBox = comp;
            this.lootBoxSpinner = new LootBoxSpinner(comp);
            this.doCloseX = true;
            this.forcePause = true;
        }

        public override void PreOpen()
        {
            base.PreOpen();

            if (compLootBox.Props.lootBoxOpenSound != null)
            {
                compLootBox.Props.lootBoxOpenSound.PlayOneShot(SoundInfo.InMap(new TargetInfo(compLootBox.parent.Position, compLootBox.parent.Map)));
            }

            if (compLootBox.Props.lootBoxBackgroundMusicSound != null)
            {
                SoundInfo info = SoundInfo.InMap(new TargetInfo(compLootBox.parent.Position, compLootBox.parent.Map), MaintenanceType.PerFrame);
                backgroundMusicSustainer = compLootBox.Props.lootBoxBackgroundMusicSound.TrySpawnSustainer(info);
                Find.MusicManagerPlay.ForceFadeoutAndSilenceFor(99999f);
            }

            if (!string.IsNullOrEmpty(compLootBox.Props.lootBoxBackgroundTexturePath))
            {
                backgroundTex = ContentFinder<Texture2D>.Get(compLootBox.Props.lootBoxBackgroundTexturePath, true);
            }

            this.windowRect.width = 700f;
            this.windowRect.height = 800f;
            this.windowRect.x = (UI.screenWidth - this.windowRect.width) / 2f;
            this.windowRect.y = (UI.screenHeight - this.windowRect.height) / 2f;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (compLootBox?.parent == null || compLootBox.Props == null)
            {
                Close();
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
                return;
            }

            if (backgroundTex != null)
            {
                GUI.DrawTexture(inRect, backgroundTex, ScaleMode.StretchToFill);
            }
            GUI.BeginGroup(inRect);
            lootBoxSpinner.Draw(new Rect(0f, 100f, inRect.width, 150f));

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = compLootBox.Props.titleTextColor;
            Widgets.Label(new Rect(0f, 10f, inRect.width, 40f), compLootBox.parent.LabelCap);
            GUI.color = Color.white;

            if (compLootBox.Props.lootBoxOpenCost != null && compLootBox.Props.lootBoxOpenCostCount > 0)
            {
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = compLootBox.Props.titleTextColor;
                Widgets.Label(
                    new Rect(0f, 55f, inRect.width, 20f),
                    "SimpleLootBox_CostLabel".Translate(
                        compLootBox.Props.lootBoxOpenCostCount,
                        compLootBox.Props.lootBoxOpenCost.LabelCap
                    ));
                GUI.color = Color.white;
            }

            backgroundMusicSustainer?.Maintain();
            spinningSustainer?.Maintain();
            if (spinningSustainer != null && !lootBoxSpinner.IsSpinning)
            {
                spinningSustainer.End();
                spinningSustainer = null;
            }

            bool HasEnoughCurrency()
            {
                if (compLootBox.Props.lootBoxOpenCost == null || compLootBox.Props.lootBoxOpenCostCount <= 0)
                    return true;

                Map map = compLootBox.parent?.Map;
                if (map == null) return false;

                ThingDef currencyDef = compLootBox.Props.lootBoxOpenCost;
                int requiredCount = compLootBox.Props.lootBoxOpenCostCount;

                int availableCount = map.listerThings.AllThings
                    .Where(t => t.def == currencyDef && t.IsInAnyStorage() && !t.Position.Fogged(map))
                    .Sum(t => t.stackCount);

                return availableCount >= requiredCount;
            }

            bool ConsumeCurrency(Map map)
            {
                if (map == null || compLootBox.Props.lootBoxOpenCost == null || compLootBox.Props.lootBoxOpenCostCount <= 0)
                    return true;

                ThingDef currencyDef = compLootBox.Props.lootBoxOpenCost;
                int toConsume = compLootBox.Props.lootBoxOpenCostCount;

                foreach (Thing thing in map.listerThings.AllThings
                    .Where(t => t.def == currencyDef && t.IsInAnyStorage() && !t.Position.Fogged(map))
                    .OrderByDescending(t => t.stackCount))
                {
                    int take = Math.Min(toConsume, thing.stackCount);
                    thing.SplitOff(take).Destroy(DestroyMode.Vanish);
                    toConsume -= take;
                    if (toConsume <= 0) return true;
                }
                return false;
            }

            Rect buttonRect = new Rect(255f, 250f, 150f, 50f);
            if (Widgets.ButtonText(buttonRect, "SimpleLootBox_OpenBox".Translate()))
            {
                if (compLootBox.parent.stackCount <= 0)
                {
                    Messages.Message("SimpleLootBox_NoBoxLeft".Translate(), MessageTypeDefOf.RejectInput);
                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.Font = GameFont.Small;
                    return;
                }

                if (!HasEnoughCurrency())
                {
                    string label = compLootBox.Props.lootBoxOpenCost?.LabelCap ?? "unknown";
                    int count = compLootBox.Props.lootBoxOpenCostCount;
                    Messages.Message("SimpleLootBox_NotEnoughCurrency".Translate(label, count), MessageTypeDefOf.RejectInput);
                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.Font = GameFont.Small;
                    return;
                }

                if (!lootBoxSpinner.IsSpinning)
                {
                    var itemWon = lootBoxSpinner.Spin();
                    pendingFinalizingSoundItem = itemWon;
                    pendingRewardItem = itemWon;

                    if (compLootBox.Props.lootBoxSpinningSound != null)
                    {
                        SoundInfo info = SoundInfo.InMap(new TargetInfo(compLootBox.parent.Position, compLootBox.parent.Map), MaintenanceType.PerFrame);
                        spinningSustainer = compLootBox.Props.lootBoxSpinningSound.TrySpawnSustainer(info);
                    }
                }
            }

            if (!lootBoxSpinner.IsSpinning && pendingRewardItem != null)
            {
                var item = pendingRewardItem.Value;
                Map map = compLootBox.parent?.Map;
                IntVec3 pos = compLootBox.parent?.Position ?? IntVec3.Invalid;

                if (item.finalizingSound != null && map != null)
                {
                    item.finalizingSound.PlayOneShot(SoundInfo.InMap(new TargetInfo(pos, map)));
                }

                if (!ConsumeCurrency(map))
                {
                    Messages.Message("SimpleLootBox_NotEnoughCurrency".Translate(
                        compLootBox.Props.lootBoxOpenCost?.LabelCap ?? "unknown",
                        compLootBox.Props.lootBoxOpenCostCount), MessageTypeDefOf.RejectInput);
                    pendingRewardItem = null;
                    pendingFinalizingSoundItem = null;
                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.Font = GameFont.Small;
                    return;
                }

                if (compLootBox.Spawn(item))
                {
                    compLootBox.DeleteBox(1);
                }

                pendingRewardItem = null;
                pendingFinalizingSoundItem = null;
            }

            Rect listRect = new Rect(20f, 300f, inRect.width - 20f, inRect.height - 300f);

            int visibleCount = lootBoxSpinner.PossibleRewards.Count(spinItem =>
            (spinItem.thingDef != null && !(compLootBox.Props.lootBoxThingDef?.Find(t => t.thingDef == spinItem.thingDef)?.isHidden ?? false)) ||
            (spinItem.pawnKindDef != null && !(compLootBox.Props.lootBoxPawnKindDef?.Find(p => p.pawnKindDef == spinItem.pawnKindDef)?.isHidden ?? false)));

            Rect contentRect = new Rect(listRect.x, listRect.y, listRect.width - 20f, 10f + visibleCount * 20f);

            Widgets.BeginScrollView(listRect, ref scrollPosition, contentRect);
            Text.Font = GameFont.Small;

            int rowIndex = 0;
            foreach (var spinItem in lootBoxSpinner.PossibleRewards)
            {
                if (spinItem.thingDef != null)
                {
                    var itemForShow = compLootBox.Props.lootBoxThingDef.Find(t => t.thingDef == spinItem.thingDef);
                    if (itemForShow != null && itemForShow.isHidden)
                        continue;
                }
                else if (spinItem.pawnKindDef != null)
                {
                    var pawnForShow = compLootBox.Props.lootBoxPawnKindDef.Find(p => p.pawnKindDef == spinItem.pawnKindDef);
                    if (pawnForShow != null && pawnForShow.isHidden)
                        continue;
                }

                Rect rowRect = new Rect(listRect.x, listRect.y + rowIndex * 20f, 100f, 20f);
                Widgets.DrawRectFast(rowRect, RarityColors.GetColor(spinItem.rarity));

                Rect labelRect = new Rect(listRect.x + 120f, listRect.y + rowIndex * 20f, listRect.width - 150f, 20f);
                Widgets.DrawRectFast(labelRect, RarityColors.GetColor(spinItem.rarity));

                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = compLootBox.Props.rewardTextColor;
                Widgets.Label(rowRect, spinItem.rarity.TranslateLabel());
                GUI.color = Color.white;

                string rewardName = spinItem.thingDef != null
                    ? spinItem.thingDef.LabelCap
                    : (spinItem.pawnKindDef?.LabelCap ?? "");

                Text.Anchor = TextAnchor.MiddleLeft;
                GUI.color = compLootBox.Props.rewardTextColor;
                Widgets.Label(labelRect, rewardName);
                GUI.color = Color.white;

                if (spinItem.count > 1)
                {
                    Text.Anchor = TextAnchor.MiddleRight;
                    GUI.color = compLootBox.Props.rewardTextColor;
                    Widgets.Label(labelRect, $"x{spinItem.count}");
                    GUI.color = Color.white;
                }

                rowIndex++;
            }

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        public override void PostClose()
        {
            base.PostClose();
            Find.MusicManagerPlay.ScheduleNewSong();
            if (backgroundMusicSustainer != null)
            {
                backgroundMusicSustainer.End();
                backgroundMusicSustainer = null;
            }

            if (spinningSustainer != null)
            {
                spinningSustainer.End();
                spinningSustainer = null;
            }
        }
    }
}


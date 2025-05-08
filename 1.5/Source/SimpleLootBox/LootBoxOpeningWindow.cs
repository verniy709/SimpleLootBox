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
            }

            this.windowRect.width = 700f;
            this.windowRect.height = 800f;
            this.windowRect.x = (UI.screenWidth - this.windowRect.width) / 2f;
            this.windowRect.y = (UI.screenHeight - this.windowRect.height) / 2f;
        }

        public override void DoWindowContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);
            lootBoxSpinner.Draw(new Rect(0f, 100f, inRect.width, 150f));
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(0f, 50f, inRect.width, 40f), compLootBox.parent.LabelCap);

            backgroundMusicSustainer?.Maintain();
            spinningSustainer?.Maintain();

            if (spinningSustainer != null && !lootBoxSpinner.IsSpinning)
            {
                spinningSustainer.End();
                spinningSustainer = null;
            }

            if (Widgets.ButtonText(new Rect(255f, 250f, 150f, 50f), "Open Box", active: compLootBox.parent.stackCount >= 1)
                && compLootBox.parent.stackCount >= 1)
            {
                LootBoxSpinner.SpinItem itemWon = lootBoxSpinner.Spin();
                pendingFinalizingSoundItem = itemWon;
                pendingRewardItem = itemWon;

                if (compLootBox.Props.lootBoxSpinningSound != null)
                {
                    SoundInfo info = SoundInfo.InMap(new TargetInfo(compLootBox.parent.Position, compLootBox.parent.Map), MaintenanceType.PerFrame);
                    spinningSustainer = compLootBox.Props.lootBoxSpinningSound.TrySpawnSustainer(info);
                }
            }

            if (!lootBoxSpinner.IsSpinning && pendingRewardItem != null)
            {
                var item = pendingRewardItem.Value;

                bool success = compLootBox.Spawn(item);
                if (success)
                {
                    compLootBox.DeleteBox(1);
                }

                if (item.finalizingSound != null)
                {
                    item.finalizingSound.PlayOneShot(SoundInfo.InMap(new TargetInfo(compLootBox.parent.Position, compLootBox.parent.Map)));
                }

                pendingRewardItem = null;
                pendingFinalizingSoundItem = null;
            }

            Rect listRect = new Rect(20f, 300f, inRect.width - 20f, inRect.height - 300f);
            Rect contentRect = new Rect(listRect.x, listRect.y, listRect.width - 20f, 10f + lootBoxSpinner.PossibleRewards.Count * 20f);
            Widgets.BeginScrollView(listRect, ref scrollPosition, contentRect);
            Text.Font = GameFont.Small;
            for (int i = 0; i < lootBoxSpinner.PossibleRewards.Count; i++)
            {
                var spinItem = lootBoxSpinner.PossibleRewards[i];

                //Color of the background of rarity next to the available rewards 
                Rect rowRect = new Rect(listRect.x, listRect.y + i * 20f, 100f, 20f);
                if (spinItem.rarity == Rarity.None)
                {
                    Widgets.DrawRectFast(rowRect, RarityColors.GetColor(spinItem.rarity));
                }
                else if(spinItem.rarity == Rarity.Common)
                {
                    Widgets.DrawRectFast(rowRect, RarityColors.GetColor(spinItem.rarity));
                }
                else if (spinItem.rarity == Rarity.Uncommon)
                {
                    Widgets.DrawRectFast(rowRect, RarityColors.GetColor(spinItem.rarity));
                }
                else if (spinItem.rarity == Rarity.Rare)
                {
                    Widgets.DrawRectFast(rowRect, RarityColors.GetColor(spinItem.rarity));
                }
                else if (spinItem.rarity == Rarity.Epic)
                {
                    Widgets.DrawRectFast(rowRect, RarityColors.GetColor(spinItem.rarity));
                }
                else if (spinItem.rarity == Rarity.Legendary)
                {
                    Widgets.DrawRectFast(rowRect, RarityColors.GetColor(spinItem.rarity));
                }

                //Color of the background of available reward labels
                Rect labelRect = new Rect(listRect.x + 120f, listRect.y + i * 20f, listRect.width - 150f, 20f);
                Color rarityColor = RarityColors.GetColor(spinItem.rarity); 
                if (spinItem.rarity == Rarity.Common)
                    rarityColor = RarityColors.GetColor(spinItem.rarity);
                else if (spinItem.rarity == Rarity.Uncommon)
                    rarityColor = RarityColors.GetColor(spinItem.rarity);
                else if (spinItem.rarity == Rarity.Rare)
                    rarityColor = RarityColors.GetColor(spinItem.rarity);
                else if (spinItem.rarity == Rarity.Epic)
                    rarityColor = RarityColors.GetColor(spinItem.rarity);
                else if (spinItem.rarity == Rarity.Legendary)
                    rarityColor = RarityColors.GetColor(spinItem.rarity);
                Widgets.DrawRectFast(labelRect, rarityColor);

                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(new Rect(listRect.x, listRect.y + i * 20f, 100f, 20f), spinItem.rarity.ToString());
                string rewardName = spinItem.thingDef != null ? spinItem.thingDef.LabelCap : (spinItem.pawnKindDef?.LabelCap ?? "");
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect(listRect.x + 120f, listRect.y + i * 20f, listRect.width - 150f, 20f), rewardName);
                if (spinItem.count > 1)
                {
                    Text.Anchor = TextAnchor.MiddleRight;
                    Widgets.Label(new Rect(listRect.x + 120f, listRect.y + i * 20f, listRect.width - 150f, 20f), $"x{spinItem.count}");
                }
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }

        public override void PostClose()
        {
            base.PostClose();
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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using static SimpleLootBox.LootBoxSpinner;

namespace SimpleLootBox
{
    public class LootBoxSpinner
    {
        private const int CellCount = 50;
        private float speed = 0f;
        public bool IsSpinning => speed > 0.1f;
        private float position = 0f;
        private readonly System.Random rng = new System.Random();
        private readonly CompLootBox comp;
        private readonly SpinItem[] spinWheelItems = new SpinItem[CellCount];

        public List<SpinItem> PossibleRewards { get; private set; } = new List<SpinItem>();

        public LootBoxSpinner(CompLootBox comp)
        {
            this.comp = comp;
            LoadPossibleRewards();
            for (int i = 0; i < CellCount; i++)
            {
                spinWheelItems[i] = PickRandomReward();
            }
        }

        private void LoadPossibleRewards()
        {
            PossibleRewards.Clear();
            CompProperties_LootBox props = comp.Props;

            if (props.lootBoxThingDef != null)
            {
                foreach (var thing in props.lootBoxThingDef)
                {
                    if (thing.thingDef == null || thing.weight <= 0) continue;
                    SpinItem item = new SpinItem(thing.thingDef, thing.stuff, thing.quality, thing.count, thing.rarity, thing.weight, thing.effecterDef, thing.lootBoxFinalizingRewardSound, thing.isHostile);
                    PossibleRewards.Add(item);
                }
            }

            if (props.lootBoxPawnKindDef != null)
            {
                foreach (var pawn in props.lootBoxPawnKindDef)
                {
                    if (pawn.pawnKindDef == null || pawn.weight <= 0) continue;
                    SpinItem item = new SpinItem(pawn.pawnKindDef, pawn.count, pawn.rarity, pawn.weight, pawn.effecterDef, pawn.lootBoxFinalizingRewardSound, pawn.isHostile);
                    PossibleRewards.Add(item);
                }
            }

            PossibleRewards.Sort((a, b) =>
            {
                int r = b.rarity.CompareTo(a.rarity);
                if (r != 0) return r;
                string an = a.thingDef != null ? a.thingDef.label : a.pawnKindDef?.label ?? "";
                string bn = b.thingDef != null ? b.thingDef.label : b.pawnKindDef?.label ?? "";
                return string.Compare(an, bn, StringComparison.OrdinalIgnoreCase);
            });
        }

        public void Draw(Rect rect)
        {
            if (spinWheelItems.Length == 0) return;

            speed = Math.Max(speed - 0.2f, 0f);
            position += speed;
            float totalWidth = rect.width * CellCount * 0.2f;

            for (int i = 0; i < CellCount; i++)
            {
                float x = totalWidth - ((position + rect.width * (i * 0.2f)) % totalWidth) + rect.x - rect.width * 0.2f;
                Rect cellRect = new Rect(x, rect.y, rect.width * 0.2f - 5f, rect.height);
                SpinItem item = spinWheelItems[i];
                Widgets.DrawRectFast(cellRect, RarityColors.GetColor(item.rarity));
                Rect iconRect = new Rect(x, rect.y + 5f, rect.width * 0.2f - 5f, rect.width * 0.2f - 5f);
                Texture iconTex = item.thingDef?.uiIcon;
                if (item.pawnKindDef != null)
                {
                    iconTex = item.portrait;
                }

                if (iconTex != null)
                {
                    Widgets.DrawTextureFitted(iconRect, iconTex, 1f);
                }
                else
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Text.Font = GameFont.Tiny;
                    string label = item.thingDef?.LabelCap ?? item.pawnKindDef?.LabelCap ?? "";

                    if (!string.IsNullOrWhiteSpace(label))
                    {
                        Widgets.Label(iconRect, label);
                    }
                }

                if (item.count > 1)
                {
                    Rect countRect = new Rect(x, rect.y + 20f, rect.width * 0.2f - 5f, rect.width * 0.2f - 5f);
                    Text.Anchor = TextAnchor.LowerRight;
                    Text.Font = GameFont.Medium;
                    Widgets.Label(countRect, "x" + item.count);
                }

                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
            }

            Rect bar = new Rect(rect.x + rect.width / 2f - 1f, rect.y, 2f, rect.height);
            Widgets.DrawRectFast(bar, new Color(0.0823f, 0.098f, 0.1137f, 1f));
        }

        public SpinItem Spin()
        {
            speed = 100.3f;
            position = 0f;

            for (int i = 0; i < CellCount; i++)
            {
                spinWheelItems[i] = PickRandomReward();
            }

            return spinWheelItems[8];
        }

        public void Stop()
        {
            speed = 0f;
            position = 0f;
        }

        private SpinItem PickRandomReward()
        {
            if (PossibleRewards.Count == 0) return default;

            double totalWeight = 0;
            foreach (var item in PossibleRewards)
            {
                totalWeight += item.weight;
            }

            double pick = rng.NextDouble() * totalWeight;
            foreach (var item in PossibleRewards)
            {
                pick -= item.weight;
                if (pick <= 0)
                    return item;
            }

            return PossibleRewards[PossibleRewards.Count - 1];
        }

        public struct SpinItem
        {
            public ThingDef thingDef;
            public PawnKindDef pawnKindDef;
            public ThingDef stuff;
            public QualityCategory quality;
            public int count;
            public Rarity rarity;
            public float weight;
            public EffecterDef effecterDef;
            public RenderTexture portrait;
            public SoundDef finalizingSound;
            public bool isHostile;

            public SpinItem(ThingDef thingDef, ThingDef stuff, QualityCategory quality, int count, Rarity rarity, float weight, EffecterDef effecter, SoundDef finalizingSound, bool isHostile)
            {
                this.thingDef = thingDef;
                this.pawnKindDef = null;
                this.stuff = stuff;
                this.quality = quality;
                this.count = count;
                this.rarity = rarity;
                this.weight = weight;
                this.effecterDef = effecter;
                this.portrait = null;
                this.finalizingSound = finalizingSound;
                this.isHostile = false;/*Have to match the struct, always false*/
            }

            public SpinItem(PawnKindDef pawnKindDef, int count, Rarity rarity, float weight, EffecterDef effecter, SoundDef finalizingSound, bool isHostile)
            {
                this.thingDef = null;
                this.pawnKindDef = pawnKindDef;
                this.stuff = null;
                this.quality = QualityCategory.Normal;
                this.count = count;
                this.rarity = rarity;
                this.weight = weight;
                this.effecterDef = effecter;
                this.finalizingSound = finalizingSound;
                this.isHostile = isHostile;

                Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDef);
                this.portrait = PortraitsCache.Get(pawn, new Vector2(128f, 128f), Rot4.South, new Vector3(0f, 0f, 0.1f), 1.25f);
            }
        }
    }
}

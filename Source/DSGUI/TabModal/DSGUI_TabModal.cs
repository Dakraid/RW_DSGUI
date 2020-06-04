using System;
using System.Linq;
using HarmonyLib;
using LWM.DeepStorage;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace DSGUI
{
    [StaticConstructorOnStartup]
    public class DSGUI_TabModal : ITab
    {
        private const float TopPadding = 20f;
        private const float ThingIconSize = 28f;
        private const float ThingRowHeight = 28f;
        private const float ThingLeftX = 36f;
        private const float StandardLineHeight = 22f;

        private static readonly Texture2D Drop;
        public static readonly Color ThingLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        public static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        private Building_Storage buildingStorage;
        private Vector2 scrollPosition = Vector2.zero;
        private float scrollViewHeight = 1000f;

        static DSGUI_TabModal()
        {
            Drop = (Texture2D) AccessTools.Field(AccessTools.TypeByName("Verse.TexButton"), "Drop").GetValue(null);
        }

        public DSGUI_TabModal()
        {
            size = new Vector2(460f, 450f);
            labelKey = "Contents";
        }

        protected override void FillTab()
        {
            buildingStorage = SelThing as Building_Storage; // don't attach this to other things, 'k?
            Text.Font = GameFont.Small;

            var frame = new Rect(10f, 10f, size.x - 10, size.y - 10);
            GUI.BeginGroup(frame);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            var curY = 0f;
            Widgets.ListSeparator(ref curY, frame.width, labelKey.Translate()
            );
            curY += 5f;

            string header, headerTooltip;
            var cds = buildingStorage?.GetComp<CompDeepStorage>();

            var storedItems = cds != null
                ? cds.getContentsHeader(out header, out headerTooltip)
                : CompDeepStorage.genericContentsHeader(buildingStorage, out header, out headerTooltip);
            var tmpRect = new Rect(8f, curY, frame.width - 16, Text.CalcHeight(header, frame.width - 16));
            Widgets.Label(tmpRect, header);

            curY += tmpRect.height;
            storedItems = storedItems.OrderBy(x => x.def.defName).ThenByDescending(x =>
            {
                x.TryGetQuality(out var c);
                return (int) c;
            }).ThenByDescending(x => x.HitPoints / x.MaxHitPoints).ToList();

            var outRect = new Rect(0f, 10f + curY, frame.width, frame.height - curY);
            var viewRect = new Rect(0f, 0f, frame.width - 16f, scrollViewHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            curY = 0f;
            if (storedItems.Count < 1)
            {
                Widgets.Label(viewRect, "NoItemsAreStoredHere".Translate());
                curY += 22;
            }

            foreach (var t in storedItems)
                DrawThingRow(ref curY, viewRect.width, t);

            if (Event.current.type == EventType.Layout) scrollViewHeight = curY + 25f;

            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private static void DrawThingRow(ref float y, float width, Thing thing)
        {
            width -= 24f;
            Widgets.InfoCardButton(width, y, thing);

            width -= 24f;
            var forbidRect = new Rect(width, y, 24f, 24f);
            var allowFlag = !thing.IsForbidden(Faction.OfPlayer);
            var tmpFlag = allowFlag;
            TooltipHandler.TipRegion(forbidRect, allowFlag ? "CommandNotForbiddenDesc".Translate() : "CommandForbiddenDesc".Translate());
            Widgets.Checkbox(forbidRect.x, forbidRect.y, ref allowFlag, 24f, false, true);
            if (allowFlag != tmpFlag)
                thing.SetForbidden(!allowFlag, false);

            if (Settings.useEjectButton)
            {
                width -= 24f;
                var yetAnotherRect = new Rect(width, y, 24f, 24f);
                TooltipHandler.TipRegion(yetAnotherRect, "LWM.ContentsDropDesc".Translate());
                if (Widgets.ButtonImage(yetAnotherRect, Drop, Color.gray, Color.white, false))
                {
                    var loc = thing.Position;
                    var map = thing.Map;
                    thing.DeSpawn();
                    if (!GenPlace.TryPlaceThing(thing, loc, map, ThingPlaceMode.Near, null,
                        newLoc => !map.thingGrid.ThingsListAtFast(newLoc).OfType<Building_Storage>().Any())) GenSpawn.Spawn(thing, loc, map);
                    if (!thing.Spawned || thing.Position == loc)
                        Messages.Message("You have filled the map.",
                            new LookTargets(loc, map), MessageTypeDefOf.NegativeEvent);
                }
            }

            width -= 60f; // Caravans use 100f
            var massRect = new Rect(width, y, 60f, 28f);
            CaravanThingsTabUtility.DrawMass(thing, massRect);
            var cr = thing.TryGetComp<CompRottable>();
            if (cr != null)
            {
                var rotTicks = Math.Min(int.MaxValue, cr.TicksUntilRotAtCurrentTemp);
                if (rotTicks < 36000000)
                {
                    width -= 60f;
                    var rotRect = new Rect(width, y, 60f, 28f);
                    GUI.color = Color.yellow;
                    Widgets.Label(rotRect, (rotTicks / 60000f).ToString("0.#"));
                    GUI.color = Color.white;
                    TooltipHandler.TipRegion(rotRect, "DaysUntilRotTip".Translate());
                }
            }

            var itemRect = new Rect(0f, y, width, 28f);
            if (Mouse.IsOver(itemRect))
            {
                GUI.color = ITab_Pawn_Gear.HighlightColor;
                GUI.DrawTexture(itemRect, TexUI.HighlightTex);
            }

            if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null) Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), thing);
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = ITab_Pawn_Gear.ThingLabelColor;
            var textRect = new Rect(36f, y, itemRect.width - 36f, itemRect.height);
            var text = thing.LabelCap;
            Text.WordWrap = false;
            Widgets.Label(textRect, text.Truncate(textRect.width));
            if (Widgets.ButtonInvisible(itemRect))
            {
                Find.Selector.ClearSelection();
                Find.Selector.Select(thing);
            }

            Text.WordWrap = true;

            var text2 = thing.DescriptionDetailed;
            if (thing.def.useHitPoints)
            {
                var text3 = text2;
                text2 = string.Concat(text3, "\n", thing.HitPoints, " / ", thing.MaxHitPoints);
            }

            TooltipHandler.TipRegion(itemRect, text2);
            y += 28f;
        }
    }
}
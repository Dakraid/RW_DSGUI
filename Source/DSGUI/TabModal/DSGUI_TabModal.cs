using System;
using System.Collections.Generic;
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
        private static readonly Texture2D Drop;
        
        private static Rect mainRect;
        private string searchString = "";

        private Vector2 scrollPosition;
        private float scrollHeight;
        private static float boxHeight = 48f;
        
        private DSGUI_TabItem[] rows;
        private List<Thing> storedItems;
        private Building_Storage buildingStorage;
        private CompDeepStorage deepStorageComp;
        private string curWeight, maxWeight;
        private int curCount, maxCount, minCount;
        
        private static Building_Storage lastStorage;

        static DSGUI_TabModal()
        {
            Drop = (Texture2D) AccessTools.Field(AccessTools.TypeByName("Verse.TexButton"), "Drop").GetValue(null);
        }

        public DSGUI_TabModal()
        {
            size = new Vector2(520f, 460f);
            
            Text.Anchor = TextAnchor.MiddleCenter;
            labelKey = "Contents";
            Text.Anchor = TextAnchor.UpperLeft;
            
            boxHeight = DSGUIMod.settings.DSGUI_List_BoxHeight;
        }

        private void getStorageProperties()
        {
            deepStorageComp = buildingStorage?.GetComp<CompDeepStorage>();

            var slotCells = (deepStorageComp?.parent as Building_Storage)?.AllSlotCells().ToList();
            
            if (slotCells == null)
                return;
            
            float totalWeight = 0;
            foreach (var allSlotCell in slotCells)
            {
                float sum = 0;
                foreach (var thing in deepStorageComp.parent.Map.thingGrid.ThingsListAt(allSlotCell).Where(thing => thing.Spawned && thing.def.EverStorable(false)))
                {
                    storedItems.Add(thing);
                    sum += thing.GetStatValue(deepStorageComp.stat) * thing.stackCount;
                }

                totalWeight += sum;
            }

            storedItems = storedItems.OrderBy(x => x.def.defName).ThenByDescending(x =>
            {
                x.TryGetQuality(out var c);
                return (int) c;
            }).ThenByDescending(x => x.HitPoints / x.MaxHitPoints).ToList();
            
            curCount = storedItems.Count;
            curWeight = totalWeight.ToString("0.##");
            minCount = deepStorageComp.minNumberStacks * slotCells.Count;
            maxCount = deepStorageComp.maxNumberStacks * slotCells.Count;
            var tempMax = deepStorageComp.limitingTotalFactorForCell * slotCells.Count;
            maxWeight = tempMax > 0 ? tempMax.ToString("0.##") : "inf.";
        }

        protected override void FillTab()
        {
            buildingStorage = SelThing as Building_Storage;
            if (buildingStorage != lastStorage)
            {
                storedItems = new List<Thing>();
                getStorageProperties();
                rows = new DSGUI_TabItem[storedItems.Count];

                lastStorage = buildingStorage;
            }
            
            mainRect = new Rect(2f, 2f, size.x - 6f, size.y - 6f);
            
            GUI.BeginGroup(mainRect);
            
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;

            // Make Header
            var headRect = new Rect(mainRect) {height = 36f};
            var headTitle = headRect.LeftPartPixels(62f);
            var headInfo = headRect.RightPartPixels(headRect.width - 69f);
            headInfo.y += 1f;
            headInfo.width -= 26f;
            
            Widgets.Label(headTitle, labelKey.Translate());
            
            DSGUI.Elements.SeparatorVertical(headTitle.width + 2f, headRect.y, headRect.height);

            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;
            
            Widgets.Label(headInfo, $"{curCount} / {maxCount} Stacks (min. {minCount})\n{curWeight} / {maxWeight} kg");
            
            
            DSGUI.Elements.SeparatorHorizontal(headRect.x, headRect.height + 5f, headRect.width);
            
            var headSub = new Rect(mainRect) {height = 18f};
            headSub.y += headRect.height + 8f;
            
            // Scrollable List
            var scrollRect = new Rect(mainRect);
            scrollRect.y += headRect.height + 10f;
            scrollRect.height -= (headRect.height + 10f);

            scrollHeight = storedItems.Count * DSGUIMod.settings.DSGUI_Tab_BoxHeight;
            var viewRect = new Rect(0.0f, 0.0f, scrollRect.width - 16f, scrollHeight);
            
            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);
            GUI.BeginGroup(viewRect);

            if (storedItems.Count < 1) Widgets.Label(viewRect, "NoItemsAreStoredHere".Translate());
            
            for (var i = 0; i < storedItems.Count; i++)
                if (rows[i] == null)
                    try
                    {
                        rows[i] = new DSGUI_TabItem(storedItems[i], Drop, boxHeight);
                        rows[i].DoDraw(viewRect, i);
                    }
                    catch (Exception ex)
                    {
                        var err = scrollRect.ContractedBy(-4f);
                        Widgets.Label(err, "Oops, something went wrong!");
                        Log.Warning(ex.ToString());
                    }
                else
                    rows[i].DoDraw(viewRect, i);

            scrollHeight = boxHeight * storedItems.Count;

            GUI.EndGroup();
            Widgets.EndScrollView();

            /*
            // if (Event.current.type == EventType.Layout) 
            // Search
            var searchRect = new Rect(innerRect);
            searchRect.x += 8f;
            searchRect.height = 28f;
            searchRect.width -= 40f + 16f; // 16f for padding of 8f on each side + 28f for the clear button

            DSGUI.Elements.InputField("Search", searchRect, ref searchString);

            searchRect.x = searchRect.width + 6f + 16f;
            searchRect.width = 28f;
            Text.Anchor = TextAnchor.MiddleLeft;
            if (Widgets.ButtonImageFitted(searchRect, Widgets.CheckboxOffTex))
                searchString = "";
            */
            
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        /*
        private static void DrawThingRow(float y, float width, Thing thing)
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

            width -= 60f;
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
        }
        */
    }
}
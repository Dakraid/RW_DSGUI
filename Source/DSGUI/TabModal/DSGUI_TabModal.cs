using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LWM.DeepStorage;
using RimWorld;
using UnityEngine;
using Verse;

namespace DSGUI
{
    [StaticConstructorOnStartup]
    public class DSGUI_TabModal : ITab
    {
        private static readonly Texture2D Drop;

        private readonly float boxHeight;
        private float scrollHeight;
        private Vector2 scrollPosition;
        private string searchString = "";
        
        private float curWeight, maxWeight;
        private int curCount, maxCount, minCount, slotCount;

        private List<DSGUI_TabItem> rows;
        private List<Thing> storedItems, lastItems;
        private Building_Storage lastStorage;

        static DSGUI_TabModal()
        {
            Drop = (Texture2D) AccessTools.Field(AccessTools.TypeByName("Verse.TexButton"), "Drop").GetValue(null);
        }

        public DSGUI_TabModal()
        {
            size = new Vector2(520f, 460f);
            labelKey = "Contents";
            boxHeight = DSGUIMod.settings.DSGUI_List_BoxHeight;
        }

        private List<Thing> setStoredItems(ISlotGroupParent buildingStorage)
        {
            var slotCells = buildingStorage?.AllSlotCells().ToList();
            var thingGrid = Find.CurrentMap.thingGrid;

            if (slotCells == null || thingGrid == null || slotCells.OptimizedNullOrEmpty())
                return null;
            
            slotCount = slotCells.Count;

            return slotCells.SelectMany(slotCell => thingGrid.ThingsListAt(slotCell).Where(thing => thing.Spawned && thing.def.EverStorable(false))).ToList();
        }

        private void setStorageProperties(CompDeepStorage deepStorageComp)
        {
            if (deepStorageComp == null)
                return;
            
            curCount = storedItems.Count;
            minCount = deepStorageComp.minNumberStacks * slotCount;
            maxCount = deepStorageComp.maxNumberStacks * slotCount;
            curWeight = 0;
            foreach (var thing in storedItems) curWeight += thing.GetStatValue(deepStorageComp.stat) * thing.stackCount;
            maxWeight = deepStorageComp.limitingTotalFactorForCell * slotCount;
        }

        protected override void FillTab()
        {
            var buildingStorage = SelThing as Building_Storage;
            storedItems = setStoredItems(buildingStorage);
            if (storedItems == null)
                return;
            
            if (buildingStorage != lastStorage || !storedItems.Equals(lastItems))
            {
                setStorageProperties(buildingStorage?.GetComp<CompDeepStorage>());

                rows = new List<DSGUI_TabItem>();
                
                lastStorage = buildingStorage;
                lastItems = new List<Thing>(storedItems);
            }
                
            if (storedItems.Count >= 1 && rows.OptimizedNullOrEmpty())
                foreach (var thing in storedItems)
                    rows.Add(new DSGUI_TabItem(thing, Drop));

            var mainRect = new Rect(4f, 2f, size.x - 8f, size.y - 6f);

            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;

            // Make Header
            var headRect = new Rect(mainRect) {height = 36f};
            var headTitle = headRect.LeftPartPixels(62f);
            var headInfo = headRect.RightPartPixels(headRect.width - 69f);
            headInfo.y += 1f;
            headInfo.width -= 26f;

            Widgets.Label(headTitle, labelKey);

            DSGUI.Elements.SeparatorVertical(headTitle.width + 2f, headRect.y, headRect.height);

            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;

            Widgets.Label(headInfo, $"{curCount} / {maxCount} Stacks (min. {minCount})\n{curWeight:0.##} / {maxWeight:0.##} kg");


            DSGUI.Elements.SeparatorHorizontal(headRect.x, headRect.height + 5f, headRect.width);

            var headSub = new Rect(mainRect) {height = 18f};
            headSub.y += headRect.height + 8f;

            // Scrollable List
            var scrollRect = new Rect(mainRect);
            scrollRect.y += headRect.height + 10f;
            scrollRect.height -= headRect.height + 48f;

            scrollHeight = storedItems.Count * boxHeight;
            var viewRect = new Rect(0.0f, 0.0f, scrollRect.width - 16f, scrollHeight);

            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);
            GUI.BeginGroup(viewRect);

            if (DSGUIMod.settings.DSGUI_Tab_SortContent && rows.Count > 1)
                if (DSGUIMod.settings.DSGUI_Tab_AdvSortContent)
                    rows = rows.OrderBy(x => x.label).ThenByDescending(x =>
                    {
                        x.target.TryGetQuality(out var c);
                        return (int) c;
                    }).ThenByDescending(x => x.target.HitPoints / x.target.MaxHitPoints).ToList();
                else
                    rows = rows.OrderBy(x => x.label).ToList();

            if (rows.Count == 0)
            {
                Widgets.Label(viewRect, "NoItemsAreStoredHere".TranslateSimple());
            } 
            else 
            {
                var i = 0;
                if (searchString.NullOrEmpty())
                {
                    foreach (var tabItem in rows)
                    {
                        tabItem.DoDraw(viewRect, i);
                        i++;
                    }
                    
                    scrollHeight = boxHeight * rows.Count;
                }
                else
                {
                    var filteredRows = rows.Where(x => x.label.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                    
                    foreach (var tabItem in filteredRows)
                    {
                        tabItem.DoDraw(viewRect, i);
                        i++;
                    }
                    
                    scrollHeight = boxHeight * filteredRows.Count;
                }
            }


            GUI.EndGroup();
            Widgets.EndScrollView();

            // Search
            var search = new Rect(scrollRect);
            search.x += 3f;
            search.width -= 3f;
            search.y += scrollRect.height + 10f;
            DSGUI.Elements.SearchBar(search, 6f, ref searchString);

            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}
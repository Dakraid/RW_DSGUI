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
            
            // if (Event.current.type == EventType.Layout) 
            mainRect = new Rect(4f, 2f, size.x - 8f, size.y - 6f);

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
            scrollRect.height -= headRect.height + 48f;

            scrollHeight = storedItems.Count * DSGUIMod.settings.DSGUI_Tab_BoxHeight;
            var viewRect = new Rect(0.0f, 0.0f, scrollRect.width - 16f, scrollHeight);
            
            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);
            GUI.BeginGroup(viewRect);

            if (storedItems.Count < 1) Widgets.Label(viewRect, "NoItemsAreStoredHere".Translate());
            
            if (storedItems.Count >= 1 && (rows.NullOrEmpty() || rows[0] == null))
                for (var i = 0; i < storedItems.Count; i++)
                    if (rows[i] == null)
                        try
                        {
                            rows[i] = new DSGUI_TabItem(storedItems[i], Drop);
                        }
                        catch (Exception ex)
                        {
                            var err = scrollRect.ContractedBy(-4f);
                            Widgets.Label(err, "Oops, something went wrong!");
                            Log.Warning(ex.ToString());
                        }

            if (DSGUIMod.settings.DSGUI_Tab_SortContent && rows.Length > 1)
                rows = rows.OrderBy(x => x.label).ToArray();
            
            if (searchString.NullOrEmpty())
            {
                for (var i = 0; i < rows.Length; i++) rows[i].DoDraw(viewRect, i);
            }
            else
            {
                var filteredRows = rows.Where(x => x.label.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0);
                var dsguiTabItems = filteredRows as DSGUI_TabItem[] ?? filteredRows.ToArray();
                
                for (var i = 0; i < dsguiTabItems.Length; i++) dsguiTabItems[i].DoDraw(viewRect, i);
            }
            
            scrollHeight = boxHeight * storedItems.Count;

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
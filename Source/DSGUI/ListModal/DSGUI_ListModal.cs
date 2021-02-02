using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DSGUI
{
    public class DSGUI_ListModal : Window
    {
        private static readonly MethodInfo CAF = AccessTools.Method(typeof(FloatMenuMakerMap), "ChoicesAtFor");
        private static readonly FieldInfo thingListTG = AccessTools.Field(typeof(ThingGrid), "thingGrid");
        
        private const float searchClearPadding = 8f;
        private static float boxHeight = 48f;
        private static readonly Vector2 defaultScreenSize = new Vector2(1920, 1080);
        private static readonly Vector2 modalSize = new Vector2(360, 480);

        private static Vector2 scrollPosition;
        private static float RecipesScrollHeight;
        private static string searchString = "";
        private static Pawn pawn;
        private static Building self;
        private static List<Thing> thingList;

        private readonly Vector3 cpos;
        private readonly DSGUI_ListItem[] rows;
        private readonly List<FloatMenuOption> orders;
        private Rect GizmoListRect;
        
        private static readonly Texture2D menuIcon = ContentFinder<Texture2D>.Get("UI/Buttons/MainButtons/Menu");
        private static readonly Texture2D DragHash = ContentFinder<Texture2D>.Get("UI/Buttons/DragHash");
        protected override float Margin => 0f;

        public DSGUI_ListModal(Pawn p, IEnumerable<Thing> lt, Vector3 pos, Building e, IEnumerable<Thing> ltt)
        {
            onlyOneOfTypeAllowed = true;
            closeOnClickedOutside = true;
            doCloseX = true;
            resizeable = true;
            draggable = true;
            self = e;
            
            if (p == null)
                return;

            cpos = pos;
            pawn = p;

            var tileThingList = new List<Thing>(ltt);
            thingList = new List<Thing>(lt);
            rows = new DSGUI_ListItem[thingList.Count];

            var index = pawn.Map.cellIndices.CellToIndex(cpos.ToIntVec3());
            var listArray = (List<Thing>[]) thingListTG.GetValue(pawn.Map.thingGrid);
            var origList = new List<Thing>(listArray[index]);
            listArray[index] = new List<Thing>(tileThingList);
            orders = (List<FloatMenuOption>) CAF.Invoke(null, new object[] {pos, pawn});
            listArray[index] = origList;
            
            boxHeight = DSGUIMod.settings.DSGUI_List_BoxHeight;
        }

        public override Vector2 InitialSize => new Vector2(modalSize.x * (Screen.width / defaultScreenSize.x), modalSize.y * (Screen.height / defaultScreenSize.y));

        protected override void SetInitialSizeAndPosition()
        {
            if (!DSGUIMod.settings.DSGUI_List_SavePosSize)
            {
                base.SetInitialSizeAndPosition();
                return;
            }

            var windowSize = GlobalStorage.savedSize.Equals(new Vector2(0, 0)) ? InitialSize : GlobalStorage.savedSize;
            var windowPos = new Vector2((float) ((UI.screenWidth - windowSize.x) / 2.0), (float) ((UI.screenHeight - windowSize.y) / 2.0));

            if (!GlobalStorage.savedPos.Equals(new Vector2(0, 0)))
                windowPos = GlobalStorage.savedPos;

            windowRect = new Rect(windowPos.x, windowPos.y, windowSize.x, windowSize.y);
            windowRect = windowRect.Rounded();
        }

        public override void PreClose()
        {
            base.PreClose();
            GlobalStorage.savedSize = windowRect.size;
            GlobalStorage.savedPos = windowRect.position;
        }


        public override void DoWindowContents(Rect inRect)
        {
            var style = new GUIStyle(Text.CurFontStyle)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            
            var moveRect = new Rect(4f, 4f, 18f, 18f);
            DSGUI.Elements.DrawIconFitted(moveRect, DragHash, Color.white, 1.1f);

            var titleX = moveRect.x + moveRect.width + 32f;
            var titleWidth = inRect.width - 60f - titleX;
            var titleRect = new Rect(titleX, 1f, titleWidth, 25f);
            if (DSGUI.Elements.ButtonInvisibleLabeledFree(Color.white, GameFont.Medium, titleRect, self.Label.CapitalizeFirst(), style))
            {
                if (pawn.Map != self.Map)
                    return;

                Find.Selector.ClearSelection();
                Find.Selector.Select(self);
                Find.WindowStack.TryRemove(typeof(DSGUI_ListModal));
            }

            if (Mouse.IsOver(titleRect))
                Widgets.DrawHighlight(titleRect);

            DSGUI.Elements.SeparatorVertical(moveRect.x + moveRect.width + 32f, 0f, titleRect.height + 3f);
            DSGUI.Elements.SeparatorVertical(inRect.width - 28f - 32f, 0f, titleRect.height + 3f);
            
            inRect = inRect.ContractedBy(16f);
            
            var innerRect = inRect;
            innerRect.y += 8f;
            innerRect.height -= 16f;

            GizmoListRect = innerRect.AtZero();
            GizmoListRect.y += scrollPosition.y;

            // Scrollable List
            var scrollRect = new Rect(innerRect);
            scrollRect.y += 3f;
            scrollRect.x += 8f;
            scrollRect.height -= 50f;
            scrollRect.width -= 16f;

            var viewRect = new Rect(0.0f, 0.0f, scrollRect.width, RecipesScrollHeight);

            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);
            GUI.BeginGroup(viewRect);

            for (var i = 0; i < thingList.Count; i++)
            {
                var viewElement = new Rect(0.0f, boxHeight * i, inRect.width, boxHeight);
                if (!viewElement.Overlaps(GizmoListRect)) continue;

                if (rows[i] == null)
                    try
                    {
                        var index = pawn.Map.cellIndices.CellToIndex(cpos.ToIntVec3());
                        var listArray = (List<Thing>[]) thingListTG.GetValue(pawn.Map.thingGrid);
                        var origList = new List<Thing>(listArray[index]);

                        listArray[index] = new List<Thing> {thingList[i]};
                        rows[i] = new DSGUI_ListItem(pawn, thingList[i], cpos, boxHeight);
                        listArray[index] = origList;
                    }
                    catch (Exception ex)
                    {
                        var rect5 = scrollRect.ContractedBy(-4f);
                        Widgets.Label(rect5, "Oops, something went wrong!");
                        Log.Warning(ex.ToString());
                    }


                if (searchString.NullOrEmpty())
                {
                    rows[i].DoDraw(viewRect, i);
                }
                else
                {
                    if (!(rows[i].label.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)) continue;

                    rows[i].DoDraw(viewRect, i);
                }
            }
            
            RecipesScrollHeight = boxHeight * thingList.Count;

            GUI.EndGroup();
            Widgets.EndScrollView();
            Widgets.DrawBox(scrollRect);

            var bottomToolRect = new Rect(scrollRect);
            bottomToolRect.y += scrollRect.height + 16f;
            bottomToolRect.height = 28f;
            
            // Search

            var clearRect = new Rect(bottomToolRect);
            clearRect.width = 28f;
            Text.Anchor = TextAnchor.MiddleLeft;
            if (DSGUI.Elements.ButtonImageFittedScaled(clearRect, Widgets.CheckboxOffTex, 0.9f))
                searchString = "";
            
            var searchFieldRect = new Rect(bottomToolRect);
            searchFieldRect.x += 28f + searchClearPadding;
            searchFieldRect.width -= 56f + searchClearPadding * 2;

            DSGUI.Elements.InputField("Search", searchFieldRect, ref searchString);
            
            var actionRect = new Rect(bottomToolRect);
            actionRect.x = bottomToolRect.x + bottomToolRect.width - 28f;
            actionRect.width = 28f;
            if (orders.Count > 0)
            {
                if (DSGUI.Elements.ButtonImageFittedScaled(actionRect, menuIcon, 1.4f)) DSGUI.Elements.TryMakeFloatMenu(orders, "DSGUI_List_Tile".TranslateSimple());
            }
            else
            {
                DSGUI.Elements.DrawIconFitted(actionRect, menuIcon, Color.gray, 1.4f);
                TooltipHandler.TipRegion(actionRect, "No Orders Available");
            }

            if (Mouse.IsOver(actionRect))
                Widgets.DrawHighlight(actionRect);

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}
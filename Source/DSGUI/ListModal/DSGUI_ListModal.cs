using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DSGUI {
    using System.Linq;

    public class DSGUI_ListModal : Window {
        private const           float      SearchClearPadding = 8f;
        private static readonly MethodInfo CAF                = AccessTools.Method(typeof(FloatMenuMakerMap), "ChoicesAtFor");
        private static readonly FieldInfo  ThingListTG        = AccessTools.Field(typeof(ThingGrid), "thingGrid");
        private static          float      _boxHeight         = 48f;
        private static readonly Vector2    DefaultScreenSize  = new Vector2(1920, 1080);
        private static readonly Vector2    ModalSize          = new Vector2(360, 480);

        private static Vector2     _scrollPosition;
        private static float       _recipesScrollHeight;
        private static string      _searchString = "";
        private static Pawn        _pawn;
        private static Building    _self;
        private static List<Thing> _thingList;

        private static readonly Texture2D MenuIcon = ContentFinder<Texture2D>.Get("UI/Buttons/MainButtons/Menu");
        private static readonly Texture2D DragHash = ContentFinder<Texture2D>.Get("UI/Buttons/DragHash");

        private readonly Vector3               cpos;
        private List<FloatMenuOption> orders;
        private DSGUI_ListItem[]      rows;
        private          Rect                  gizmoListRect;

        public DSGUI_ListModal(Pawn p, IEnumerable<Thing> lt, Vector3 pos, Building e, IEnumerable<Thing> ltt) {
            onlyOneOfTypeAllowed  = true;
            closeOnClickedOutside = true;
            doCloseX              = true;
            resizeable            = true;
            draggable             = true;
            _self                 = e;
            if (p == null)
                return;

            cpos  = pos;
            _pawn = p;
            // TODO: Move the entire ThingList trickery into its own function
            var tileThingList = new List<Thing>(ltt);
            _thingList = new List<Thing>(lt);
            rows       = new DSGUI_ListItem[_thingList.Count];
            var index     = _pawn.Map.cellIndices.CellToIndex(cpos.ToIntVec3());
            var listArray = (List<Thing>[]) ThingListTG.GetValue(_pawn.Map.thingGrid);
            var origList  = new List<Thing>(listArray[index]);
            listArray[index] = new List<Thing>(tileThingList);
            orders           = (List<FloatMenuOption>) CAF.Invoke(null, new object[] {pos, _pawn, false});
            listArray[index] = origList;
            _boxHeight       = DSGUIMod.Settings.DSGUI_List_BoxHeight;
        }

        protected override float Margin => 0f;

        public override Vector2 InitialSize => new Vector2(ModalSize.x * (Screen.width / DefaultScreenSize.x), ModalSize.y * (Screen.height / DefaultScreenSize.y));

        protected override void SetInitialSizeAndPosition() {
            if (!DSGUIMod.Settings.DSGUI_List_SavePosSize) {
                base.SetInitialSizeAndPosition();
                return;
            }

            var windowSize = GlobalStorage.SavedSize.Equals(new Vector2(0, 0)) ? InitialSize : GlobalStorage.SavedSize;
            var windowPos  = new Vector2((float) ((UI.screenWidth - windowSize.x) / 2.0), (float) ((UI.screenHeight - windowSize.y) / 2.0));
            if (!GlobalStorage.SavedPos.Equals(new Vector2(0, 0)))
                windowPos = GlobalStorage.SavedPos;

            windowRect = new Rect(windowPos.x, windowPos.y, windowSize.x, windowSize.y);
            windowRect = windowRect.Rounded();
        }

        public override void PreClose() {
            base.PreClose();
            GlobalStorage.SavedSize = windowRect.size;
            GlobalStorage.SavedPos  = windowRect.position;
        }


        public override void DoWindowContents(Rect inRect) {
            var style = new GUIStyle(Text.CurFontStyle) {
                fontSize  = 16,
                alignment = TextAnchor.MiddleCenter
            };
            
            var c   = cpos.ToIntVec3();
            var lt  = new List<Thing>(c.GetThingList(_pawn.Map));
            var ltt = lt.Where(t => t.def.category != ThingCategory.Item).ToList();
            lt.RemoveAll(t => t.def.category != ThingCategory.Item || t is Mote);

            if (lt.Count != _thingList.Count || !lt.SequenceEqual(_thingList))
            {
                // TODO: Move the entire ThingList trickery into its own function
                var tileThingList = new List<Thing>(ltt);
                _thingList = new List<Thing>(lt);
                rows       = new DSGUI_ListItem[_thingList.Count];
                var index     = _pawn.Map.cellIndices.CellToIndex(cpos.ToIntVec3());
                var listArray = (List<Thing>[]) ThingListTG.GetValue(_pawn.Map.thingGrid);
                var origList  = new List<Thing>(listArray[index]);
                listArray[index] = new List<Thing>(tileThingList);
                orders           = (List<FloatMenuOption>) CAF.Invoke(null, new object[] {cpos, _pawn, false});
                listArray[index] = origList;
                _boxHeight       = DSGUIMod.Settings.DSGUI_List_BoxHeight;
            }

            var moveRect = new Rect(4f, 4f, 18f, 18f);
            DSGUI.Elements.DrawIconFitted(moveRect, DragHash, Color.white, 1.1f);
            var titleX     = moveRect.x + moveRect.width + 32f;
            var titleWidth = inRect.width - 60f - titleX;
            var titleRect  = new Rect(titleX, 1f, titleWidth, 25f);
            if (DSGUI.Elements.ButtonInvisibleLabeledFree(Color.white, GameFont.Medium, titleRect, _self.Label.CapitalizeFirst(), style)) {
                if (_pawn.Map != _self.Map)
                    return;

                Find.Selector.ClearSelection();
                Find.Selector.Select(_self);
                Find.WindowStack.TryRemove(typeof(DSGUI_ListModal));
            }

            if (Mouse.IsOver(titleRect))
                Widgets.DrawHighlight(titleRect);

            DSGUI.Elements.SeparatorVertical(moveRect.x + moveRect.width + 32f, 0f, titleRect.height + 3f);
            DSGUI.Elements.SeparatorVertical(inRect.width - 28f - 32f, 0f, titleRect.height + 3f);
            inRect = inRect.ContractedBy(16f);
            var innerRect = inRect;
            innerRect.y      += 8f;
            innerRect.height -= 16f;
            gizmoListRect    =  innerRect.AtZero();
            gizmoListRect.y  += _scrollPosition.y;

            // Scrollable List
            var scrollRect = new Rect(innerRect);
            scrollRect.y      += 3f;
            scrollRect.x      += 8f;
            scrollRect.height -= 50f;
            scrollRect.width  -= 16f;
            var viewRect = new Rect(0.0f, 0.0f, scrollRect.width, _recipesScrollHeight);
            Widgets.BeginScrollView(scrollRect, ref _scrollPosition, viewRect);
            GUI.BeginGroup(viewRect);

            for (var i = 0; i < _thingList.Count; i++) {
                var viewElement = new Rect(0.0f, _boxHeight * i, inRect.width, _boxHeight);
                if (!viewElement.Overlaps(gizmoListRect)) continue;

                if (rows[i] == null) {
                    try {
                        // TODO: Move the entire ThingList trickery into its own function
                        var index     = _pawn.Map.cellIndices.CellToIndex(cpos.ToIntVec3());
                        var listArray = (List<Thing>[]) ThingListTG.GetValue(_pawn.Map.thingGrid);
                        var origList  = new List<Thing>(listArray[index]);
                        listArray[index] = new List<Thing> {_thingList[i]};
                        rows[i]          = new DSGUI_ListItem(_pawn, _thingList[i], cpos, _boxHeight);
                        listArray[index] = origList;
                    }
                    catch (Exception ex) {
                        var rect5 = scrollRect.ContractedBy(-4f);
                        Widgets.Label(rect5, "Failed to generate thing entry!");
                        Log.Warning(ex.ToString());
                    }
                }

                try
                {
                    if (_searchString.NullOrEmpty())
                    {
                        rows[i].DoDraw(viewRect, i);
                    }
                    else
                    {
                        if (!(rows[i].Label.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0)) continue;

                        rows[i].DoDraw(viewRect, i);
                    }
                }
                catch (Exception ex)
                {
                    var rect5 = scrollRect.ContractedBy(-4f);
                    Widgets.Label(rect5, "Failed to draw thing entry!");
                    Log.Warning(ex.ToString());
                }
            }

            _recipesScrollHeight = _boxHeight * _thingList.Count;
            GUI.EndGroup();
            Widgets.EndScrollView();
            Widgets.DrawBox(scrollRect);
            var bottomToolRect = new Rect(scrollRect);
            bottomToolRect.y      += scrollRect.height + 16f;
            bottomToolRect.height =  28f;

            // Search
            var clearRect = new Rect(bottomToolRect) {width = 28f};
            Text.Anchor = TextAnchor.MiddleLeft;
            if (DSGUI.Elements.ButtonImageFittedScaled(clearRect, Widgets.CheckboxOffTex, 0.9f))
                _searchString = "";

            var searchFieldRect = new Rect(bottomToolRect);
            searchFieldRect.x     += 28f + SearchClearPadding;
            searchFieldRect.width -= 56f + SearchClearPadding * 2;
            DSGUI.Elements.InputField("Search", searchFieldRect, ref _searchString);
            var actionRect = new Rect(bottomToolRect) {x = bottomToolRect.x + bottomToolRect.width - 28f, width = 28f};
            if (orders.Count > 0) {
                if (DSGUI.Elements.ButtonImageFittedScaled(actionRect, MenuIcon, 1.4f)) DSGUI.Elements.TryMakeFloatMenu(orders, "DSGUI_List_Tile".TranslateSimple());
            }
            else {
                DSGUI.Elements.DrawIconFitted(actionRect, MenuIcon, Color.gray, 1.4f);
                TooltipHandler.TipRegion(actionRect, "No Orders Available");
            }

            if (Mouse.IsOver(actionRect))
                Widgets.DrawHighlight(actionRect);

            Text.Font   = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}
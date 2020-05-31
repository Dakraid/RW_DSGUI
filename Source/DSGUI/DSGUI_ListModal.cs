using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace DSGUI
{
    public class DSGUI_ListModal : Window
    {
        private const float searchClearPadding = 16f;
        private static float boxHeight = 48f;
        private static readonly Vector2 defaultScreenSize = new Vector2(1920, 1080);
        private static readonly Vector2 modalSize = new Vector2(360, 480);

        private static Vector2 scrollPosition;
        private static float RecipesScrollHeight;
        private static string searchString = "";
        private static Pawn pawn;
        private static List<Thing> thingList;

        private readonly Vector3 cpos;
        private readonly DSGUI_ListItem[] rows;
        private Rect GizmoListRect;

        public DSGUI_ListModal(Pawn p, List<Thing> lt, Vector3 pos)
        {
            onlyOneOfTypeAllowed = true;
            closeOnClickedOutside = true;
            doCloseX = true;
            resizeable = true;
            draggable = true;

            if (p == null)
                return;

            cpos = pos;
            pawn = p;

            lt.RemoveAll(t => t.def.category != ThingCategory.Item || t is Mote);
            thingList = new List<Thing>(lt);

            rows = new DSGUI_ListItem[thingList.Count];

            boxHeight = DSGUIMod.settings.DSGUI_BoxHeight;
        }

        public override Vector2 InitialSize => new Vector2(modalSize.x * (Screen.width / defaultScreenSize.x), modalSize.y * (Screen.height / defaultScreenSize.y));
        
        protected override void SetInitialSizeAndPosition()
        {
            if (!DSGUIMod.settings.DSGUI_SavePosSize)
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
            var innerRect = inRect;
            innerRect.y += 8f;
            innerRect.height -= 16f;

            GizmoListRect = innerRect.AtZero();
            GizmoListRect.y += scrollPosition.y;

            // Scrollable List
            var scrollRect = new Rect(innerRect);
            scrollRect.y += 3f;
            scrollRect.x += 8f;
            scrollRect.height -= 49f;
            scrollRect.width -= 16f;

            var listRect = new Rect(0.0f, 0.0f, scrollRect.width, RecipesScrollHeight);

            Widgets.BeginScrollView(scrollRect, ref scrollPosition, listRect);
            GUI.BeginGroup(listRect);

            var j = 0;
            for (var i = 0; i < thingList.Count; i++)
            {
                ++j;
                var viewElement = new Rect(0.0f, boxHeight * i, inRect.width, boxHeight);
                if (!viewElement.Overlaps(GizmoListRect)) continue;

                if (rows[i] == null)
                    try
                    {
                        GlobalStorage.currThing = thingList[i].GetInnerIfMinified();
                        rows[i] = new DSGUI_ListItem(pawn, thingList[i], cpos, boxHeight);
                        GlobalStorage.currThing = null;
                    }
                    catch (Exception ex)
                    {
                        var rect5 = scrollRect.ContractedBy(-4f);
                        Widgets.Label(rect5, "Oops, something went wrong!");
                        Log.Warning(ex.ToString());
                    }


                if (searchString.NullOrEmpty())
                {
                    rows[i].DoDraw(listRect, i);
                }
                else
                {
                    if (!(rows[i].label.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)) continue;

                    rows[i].DoDraw(listRect, i);
                }
            }


            RecipesScrollHeight = boxHeight * thingList.Count;

            GUI.EndGroup();
            Widgets.EndScrollView();
            Widgets.DrawBox(scrollRect);

            // Search
            var searchRect = new Rect(innerRect);
            searchRect.y += scrollRect.height + 16f;
            searchRect.x += 8f;
            searchRect.height = 28f;
            searchRect.width -= 40f + searchClearPadding; // 16f for padding of 8f on each side + 28f for the clear button

            DSGUI.Elements.InputField("Search", searchRect, ref searchString);

            searchRect.x = searchRect.width + 6f + searchClearPadding;
            searchRect.width = 28f;
            Text.Anchor = TextAnchor.MiddleLeft;
            if (Widgets.ButtonImageFitted(searchRect, Widgets.CheckboxOffTex))
                searchString = "";

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}
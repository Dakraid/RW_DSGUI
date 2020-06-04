using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DSGUI
{
    public class DSGUI_ListItem
    {
        // Allow calling AddHumanlikeOrders
        private readonly MethodInfo AHlO = AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders");

        private readonly float height;
        private readonly float iconScale;
        public readonly string label;
        private readonly Texture2D menuIcon = ContentFinder<Texture2D>.Get("UI/Buttons/MainButtons/Menu");

        private readonly List<FloatMenuOption> orders = new List<FloatMenuOption>();
        private readonly Thing origTarget;
        private readonly Pawn pawn;
        private readonly GUIStyle style;
        private readonly Thing target;
        private readonly Color thingColor = Color.white;
        private readonly Texture2D thingIcon;

        public DSGUI_ListItem(
            Pawn p,
            Thing t,
            Vector3 clickPos,
            float boxHeight)
        {
            iconScale = DSGUIMod.settings.DSGUI_List_IconScaling;
            height = boxHeight;
            origTarget = t;
            target = t.GetInnerIfMinified();
            label = t.Label;
            pawn = p;

            try
            {
                thingIcon = target.def.uiIcon;
                thingColor = target.def.uiIconColor;
            }
            catch
            {
                Log.Warning($"[DSGUI] Thing {t.def.defName} has no UI icon.");
                thingIcon = Texture2D.blackTexture;
            }

            AHlO.Invoke(null, new object[] {clickPos, pawn, orders});

            style = new GUIStyle(Text.CurFontStyle)
            {
                fontSize = DSGUIMod.settings.DSGUI_List_FontSize,
                alignment = TextAnchor.MiddleCenter
            };
        }

        public void DoDraw(Rect inRect, float y, bool altBG = false)
        {
            var listRect = new Rect(0.0f, height * y, inRect.width, height);

            //if (altBG)
            //    DSGUI.Elements.SolidColorBG(listRect, new Color(1f, 1f, 1f, 0.075f));

            var graphicRect = listRect.LeftPart(0.9f);
            graphicRect.width -= 16;
            var actionRect = listRect.RightPart(0.1f);
            actionRect.x -= 16;

            GUI.color = thingColor;
            Widgets.DrawTextureFitted(graphicRect.LeftPart(0.15f).ContractedBy(2f), thingIcon, iconScale);
            TooltipHandler.TipRegion(graphicRect.RightPart(0.85f), (TipSignal) target.def.description);
            GUI.color = Color.white;

            if (DSGUI.Elements.ButtonInvisibleLabeledFree(Color.white, GameFont.Small, graphicRect.RightPart(0.85f), label.CapitalizeFirst(), style))
            {
                if (pawn.Map != origTarget.Map)
                    return;

                Find.Selector.ClearSelection();
                Find.Selector.Select(origTarget);
                Find.WindowStack.TryRemove(typeof(DSGUI_ListModal));
            }

            if (Mouse.IsOver(graphicRect))
                Widgets.DrawHighlight(graphicRect);

            if (orders.Count > 0)
            {
                if (DSGUI.Elements.ButtonImageFittedScaled(actionRect, menuIcon, iconScale)) DSGUI.Elements.TryMakeFloatMenu(pawn, orders, target.LabelCapNoCount);
            }
            else
            {
                GUI.color = Color.gray;
                Widgets.DrawTextureFitted(actionRect, menuIcon, iconScale);
                GUI.color = Color.white;
                TooltipHandler.TipRegion(actionRect, "No orders available");
            }

            if (Mouse.IsOver(actionRect))
                Widgets.DrawHighlight(actionRect);

            if (DSGUIMod.settings.DSGUI_List_DrawDividersColumns)
                DSGUI.Elements.SeparatorVertical(graphicRect.xMax, height * y, height);

            if (y != 0 && DSGUIMod.settings.DSGUI_List_DrawDividersRows)
                DSGUI.Elements.SeparatorHorizontal(0f, height * y, listRect.width);
        }
    }
}
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
            //if (altBG)
            //    DSGUI.Elements.SolidColorBG(listRect, new Color(1f, 1f, 1f, 0.075f));
            
            var listRect = new Rect(0.0f, height * y, inRect.width, height);
            var itemRect = listRect.LeftPart(0.9f);
            itemRect.width -= 16;
            var actionRect = listRect.RightPart(0.1f);
            actionRect.x -= 16;
            var iconRect = itemRect.LeftPart(0.15f).ContractedBy(2f);
            var labelRect = itemRect.RightPart(0.85f);

            DSGUI.Elements.DrawIconFitted(iconRect, thingIcon, thingColor, iconScale);
            TooltipHandler.TipRegion(labelRect, (TipSignal) target.def.description);

            if (DSGUI.Elements.ButtonInvisibleLabeledFree(Color.white, GameFont.Small, itemRect.RightPart(0.85f), label.CapitalizeFirst(), style))
            {
                if (pawn.Map != origTarget.Map)
                    return;

                Find.Selector.ClearSelection();
                Find.Selector.Select(origTarget);
                Find.WindowStack.TryRemove(typeof(DSGUI_ListModal));
            }

            if (Mouse.IsOver(itemRect))
                Widgets.DrawHighlight(itemRect);

            if (orders.Count > 0)
            {
                if (DSGUI.Elements.ButtonImageFittedScaled(actionRect, menuIcon, iconScale)) DSGUI.Elements.TryMakeFloatMenu(pawn, orders, target.LabelCapNoCount);
            }
            else
            {
                DSGUI.Elements.DrawIconFitted(actionRect, menuIcon, Color.gray, iconScale);
                TooltipHandler.TipRegion(actionRect, "No orders available");
            }

            if (Mouse.IsOver(actionRect))
                Widgets.DrawHighlight(actionRect);

            if (DSGUIMod.settings.DSGUI_List_DrawDividersColumns)
                DSGUI.Elements.SeparatorVertical(itemRect.xMax, height * y, height);

            if (y != 0 && DSGUIMod.settings.DSGUI_List_DrawDividersRows)
                DSGUI.Elements.SeparatorHorizontal(0f, height * y, listRect.width);
        }
    }
}
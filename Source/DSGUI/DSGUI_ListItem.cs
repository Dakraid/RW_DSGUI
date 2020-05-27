using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Object = System.Object;

namespace DSGUI
{
    public class DSGUI_ListItem
    {
        public readonly string label;
        
        // Allow calling AddHumanlikeOrders
        private readonly MethodInfo AHlO = AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders");
        
        private readonly float height;
        private readonly float iconScale;

        private readonly Texture2D menuIcon = ContentFinder<Texture2D>.Get("UI/Buttons/MainButtons/Menu");
        private readonly Vector3 cpos;
        private readonly Pawn pawn;
        private readonly Thing target;
        private readonly Color thingColor = Color.white;
        private readonly Texture2D thingIcon;
        private readonly List<FloatMenuOption> orders = new List<FloatMenuOption>();

        public DSGUI_ListItem(
            Pawn p,
            Thing t,
            Vector3 clickPos,
            float boxHeight)
        {
            iconScale = DSGUIMod.settings.DSGUI_IconScaling;
            height = boxHeight;
            target = t.GetInnerIfMinified();
            label = t.Label;
            pawn = p;
            cpos = clickPos;

            try
            {
                thingIcon = target.def.uiIcon;
                thingColor = target.def.uiIconColor;
            }
            catch
            {
                Log.Warning($"[LWM] Thing {t.def.defName} has no UI icon.");
                thingIcon = Texture2D.blackTexture;
            }
            
            GlobalStorage.currThing = target;
            AHlO.Invoke(null, new object[] {clickPos, pawn, orders});
            GlobalStorage.currThing = null;
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
            
            if (DSGUI.Elements.ButtonInvisibleLabeled(Color.white, GameFont.Small, graphicRect.RightPart(0.85f), label.CapitalizeFirst()))
            {
                if (pawn.Map != target.Map)
                    return;
                
                Find.Selector.ClearSelection();
                Find.Selector.Select(target);
                Find.WindowStack.TryRemove(typeof(DSGUI_ListModal));
            }

            if (Mouse.IsOver(graphicRect))
                Widgets.DrawHighlight(graphicRect);

            if (orders.Count > 0)
            {
                if (DSGUI.Elements.ButtonImageFittedScaled(actionRect, menuIcon, iconScale))
                {
                    orders.Clear();
                
                    GlobalStorage.currThing = target;
                    AHlO.Invoke(null, new object[] {cpos, pawn, orders});
                    GlobalStorage.currThing = null;
                    
                    DSGUI.Elements.TryMakeFloatMenu(pawn, orders, target.LabelCapNoCount);
                }
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

            DSGUI.Elements.SeparatorVertical(graphicRect.xMax, height * y, height);
            
            if (y != 0)
                DSGUI.Elements.SeparatorHorizontal(0f, height * y, listRect.width);
        }
    }
}
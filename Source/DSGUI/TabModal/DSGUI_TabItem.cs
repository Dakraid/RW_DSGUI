using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using LWM.DeepStorage;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace DSGUI
{
    public class DSGUI_TabItem
    {
        private readonly float height;
        private readonly float iconScale;
        private readonly string label;
        
        private readonly GUIStyle style;
        private readonly Thing target;
        private readonly Color thingColor = Color.white;
        private readonly Texture2D thingIcon;
        private readonly Texture2D dropIcon;

        public DSGUI_TabItem(
            Thing t,
            Texture2D icon,
            float boxHeight)
        {
            iconScale = DSGUIMod.settings.DSGUI_Tab_IconScaling;
            height = boxHeight;
            target = t;
            label = t.Label;
            dropIcon = icon;

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

            style = new GUIStyle(Text.CurFontStyle)
            {
                fontSize = DSGUIMod.settings.DSGUI_Tab_FontSize,
                alignment = TextAnchor.MiddleCenter
            };
        }

        public void DoDraw(Rect inRect, float y, bool altBG = false)
        {
            //if (altBG)
            //    DSGUI.Elements.SolidColorBG(listRect, new Color(1f, 1f, 1f, 0.075f));
            
            // Define all the rectangles used by the GUI
            var listRect = new Rect(0.0f, height * y, inRect.width, height);
            var thingRect = listRect.LeftPart(0.8f);
            thingRect.width -= 16;
            var itemRect = thingRect.LeftPartPixels(thingRect.width - 72f);
            var massRect = thingRect.RightPartPixels(72f);
            massRect.x += 6f;
            var actionRect = listRect.RightPart(0.2f);
            actionRect.x -= 16;
            var iconRect = itemRect.LeftPart(0.15f).ContractedBy(2f);
            var labelRect = itemRect.RightPart(0.85f);

            // Draw the thing icon
            DSGUI.Elements.DrawIconFitted(iconRect, thingIcon, thingColor, iconScale);

            var toolTip = target.DescriptionDetailed;
            if (target.def.useHitPoints)
            {
                var temp = toolTip;
                toolTip = string.Concat(temp, "\nHP:", target.HitPoints, " / ", target.MaxHitPoints);
            }

            var cr = target.TryGetComp<CompRottable>();
            if (cr != null)
            {
                const float rotLabelWidth = 60f;
                var rotTicks = Math.Min(int.MaxValue, cr.TicksUntilRotAtCurrentTemp);
                if (rotTicks < 36000000)
                {
                    Text.Font = GameFont.Small;
                    GUI.color = Color.yellow;
                    DSGUI.Elements.LabelAnchored(labelRect.RightPartPixels(rotLabelWidth), (rotTicks / 60000f).ToString("0.#") + " days", TextAnchor.MiddleCenter);
                    GUI.color = Color.white;
                    TooltipHandler.TipRegion(labelRect.RightPartPixels(rotLabelWidth), "DaysUntilRotTip".Translate());
                }

                labelRect = itemRect.LeftPartPixels(itemRect.width - rotLabelWidth);
                TooltipHandler.TipRegion(labelRect, (TipSignal) toolTip);
            }
            else
            {
                TooltipHandler.TipRegion(itemRect, (TipSignal) toolTip);
            }

            Text.Anchor = TextAnchor.MiddleCenter;
            if (DSGUI.Elements.ButtonInvisibleLabeled(Color.white, GameFont.Small, labelRect, label.CapitalizeFirst()))
            {
                Find.Selector.ClearSelection();
                Find.Selector.Select(target);
            }

            Text.Anchor = TextAnchor.UpperLeft;

            if (Mouse.IsOver(itemRect))
                Widgets.DrawHighlight(itemRect);

            DSGUI.Elements.LabelAnchored(massRect, target.def.BaseMass.ToString("0.## kg"), TextAnchor.MiddleCenter);
            
            actionRect.width -= 24f;
            Widgets.InfoCardButton(actionRect.x + actionRect.width, height * y, target);
            
            var forbidRect = new Rect(width, y, 24f, 24f);
            var allowFlag = !target.IsForbidden(Faction.OfPlayer);
            var tmpFlag = allowFlag;
            TooltipHandler.TipRegion(forbidRect, allowFlag ? "CommandNotForbiddenDesc".Translate() : "CommandForbiddenDesc".Translate());
            Widgets.Checkbox(forbidRect.x, forbidRect.y, ref allowFlag, 24f, false, true);
            if (allowFlag != tmpFlag)
                target.SetForbidden(!allowFlag, false);

            if (Settings.useEjectButton)
            {
                var yetAnotherRect = new Rect(width, y, 24f, 24f);
                TooltipHandler.TipRegion(yetAnotherRect, "LWM.ContentsDropDesc".Translate());
                if (Widgets.ButtonImage(yetAnotherRect, dropIcon, Color.gray, Color.white, false))
                {
                    var loc = target.Position;
                    var map = target.Map;
                    target.DeSpawn();
                    if (!GenPlace.TryPlaceThing(target, loc, map, ThingPlaceMode.Near, null,
                        newLoc => !map.thingGrid.ThingsListAtFast(newLoc).OfType<Building_Storage>().Any())) GenSpawn.Spawn(target, loc, map);
                    if (!target.Spawned || target.Position == loc)
                        Messages.Message("You have filled the map.",
                            new LookTargets(loc, map), MessageTypeDefOf.NegativeEvent);
                }
            }

            if (DSGUIMod.settings.DSGUI_Tab_DrawDividersColumns) 
            {
                DSGUI.Elements.SeparatorVertical(itemRect.xMax, height * y, height);
                DSGUI.Elements.SeparatorVertical(massRect.xMax, height * y, height);
            }

            if (y != 0 && DSGUIMod.settings.DSGUI_Tab_DrawDividersRows)
                DSGUI.Elements.SeparatorHorizontal(0f, height * y, listRect.width);
            
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}
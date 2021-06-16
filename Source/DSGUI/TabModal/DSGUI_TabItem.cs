using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace DSGUI {
    public class DSGUI_TabItem {
        private readonly Texture2D dropIcon;
        private readonly float     height;
        private readonly float     iconScale;
        public readonly  string    Label;
        private readonly GUIStyle  style;

        public readonly  Thing     Target;
        private readonly Color     thingColor = Color.white;
        private readonly Texture2D thingIcon;

        public DSGUI_TabItem(
            Thing     t,
            Texture2D icon) {
            iconScale = DSGUIMod.Settings.DSGUI_Tab_IconScaling;
            height    = DSGUIMod.Settings.DSGUI_Tab_BoxHeight;
            Target    = t;
            Label     = t.Label;
            dropIcon  = icon;
            try {
                if (Target.GetInnerIfMinified() != Target) {
                    thingIcon  = Target.GetInnerIfMinified().def.uiIcon;
                    thingColor = Target.GetInnerIfMinified().def.uiIconColor;
                }
                else {
                    thingIcon  = Target.def.uiIcon;
                    thingColor = Target.def.uiIconColor;
                }
            }
            catch {
                Log.Warning($"[DSGUI] Thing {t.def.defName} has no UI icon.");
                thingIcon = Texture2D.blackTexture;
            }

            style = new GUIStyle(Text.CurFontStyle) {
                fontSize  = DSGUIMod.Settings.DSGUI_Tab_FontSize,
                alignment = TextAnchor.MiddleCenter
            };
        }

        public void DoDraw(Rect inRect, float y, bool altBG = false) {
            //if (altBG)
            //    DSGUI.Elements.SolidColorBG(listRect, new Color(1f, 1f, 1f, 0.075f));

            // Define all the rectangles used by the GUI
            var listRect  = new Rect(0.0f, height * y, inRect.width, height);
            var thingRect = listRect.LeftPart(0.8f);
            var itemRect  = thingRect.LeftPartPixels(thingRect.width - 72f);
            var massRect  = thingRect.RightPartPixels(72f);
            massRect.x += 6f;
            var actionRect = listRect.RightPart(0.2f);
            actionRect.x     += 6f;
            actionRect.width -= 6f;
            var iconRect  = itemRect.LeftPart(0.15f).ContractedBy(2f);
            var labelRect = itemRect.RightPart(0.85f);

            // Draw the thing icon
            // DSGUI.Elements.DrawIconFitted(iconRect, thingIcon, thingColor, iconScale);
            DSGUI.Elements.DrawThingIcon(iconRect, Target, iconScale);
            var toolTip = Target.DescriptionDetailed;
            if (Target.def.useHitPoints) {
                var temp = toolTip;
                toolTip = string.Concat(temp, "\nHP: ", Target.HitPoints, " / ", Target.MaxHitPoints);
            }

            var cr = Target.TryGetComp<CompRottable>();
            if (cr != null) {
                const float rotLabelWidth = 60f;
                var         rotTicks      = Math.Min(int.MaxValue, cr.TicksUntilRotAtCurrentTemp);
                if (rotTicks < 36000000) {
                    Text.Font = GameFont.Small;
                    GUI.color = Color.yellow;
                    DSGUI.Elements.LabelAnchored(labelRect.RightPartPixels(rotLabelWidth), (rotTicks / 60000f).ToString("0.#") + " days", TextAnchor.MiddleCenter);
                    GUI.color = Color.white;
                    TooltipHandler.TipRegion(labelRect.RightPartPixels(rotLabelWidth), "DaysUntilRotTip".TranslateSimple());
                }

                labelRect = labelRect.LeftPartPixels(labelRect.width - rotLabelWidth);
                TooltipHandler.TipRegion(labelRect, (TipSignal) toolTip);
            }
            else {
                TooltipHandler.TipRegion(itemRect, (TipSignal) toolTip);
            }

            if (DSGUI.Elements.ButtonInvisibleLabeled(Color.white, GameFont.Small, labelRect, Label.CapitalizeFirst(), TextAnchor.MiddleLeft)) {
                if (Target.Map != Find.CurrentMap)
                    return;

                Find.Selector.ClearSelection();
                Find.Selector.Select(Target);
            }

            if (Mouse.IsOver(itemRect))
                Widgets.DrawHighlight(itemRect);

            DSGUI.Elements.LabelAnchored(massRect, Target.def.BaseMass.ToString("0.## kg"), TextAnchor.MiddleCenter);
            var spacing   = (actionRect.width - 24 * 3) / 4;
            var xPos      = actionRect.x + spacing;
            var heightPos = height * y + (height - 24f) / 2;
            var ejectRect = new Rect(xPos, heightPos, 24f, 24f);
            TooltipHandler.TipRegion(ejectRect, "LWM.ContentsDropDesc".TranslateSimple());
            if (Widgets.ButtonImage(ejectRect, dropIcon, Color.gray, Color.white, false))
                EjectTarget(Target);

            xPos += spacing + 24f;
            var forbidRect = new Rect(xPos, heightPos, 24f, 24f);
            var allowFlag  = !Target.IsForbidden(Faction.OfPlayer);
            var tmpFlag    = allowFlag;
            TooltipHandler.TipRegion(forbidRect, allowFlag ? "CommandNotForbiddenDesc".TranslateSimple() : "CommandForbiddenDesc".TranslateSimple());
            Widgets.Checkbox(forbidRect.x, forbidRect.y, ref allowFlag, 24f, false, true);
            if (allowFlag != tmpFlag)
                Target.SetForbidden(!allowFlag, false);

            xPos += spacing + 24f;
            Widgets.InfoCardButton(xPos, heightPos, Target);
            if (DSGUIMod.Settings.DSGUI_Tab_DrawDividersColumns) {
                DSGUI.Elements.SeparatorVertical(itemRect.xMax, height * y, height);
                DSGUI.Elements.SeparatorVertical(massRect.xMax, height * y, height);
            }

            if (y != 0 && DSGUIMod.Settings.DSGUI_Tab_DrawDividersRows)
                DSGUI.Elements.SeparatorHorizontal(0f, height * y, listRect.width);

            Text.Anchor = TextAnchor.UpperLeft;
        }

        private static void EjectTarget(Thing target) {
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
}
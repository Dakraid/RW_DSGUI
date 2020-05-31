using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DSGUI
{
    public partial class DSGUI
    {
        public static class StaticHelper
        {
            public static List<Building> GetBuildings(IntVec3 c, Map map)
            {
                var returnList = new List<Building>();
                var thingList = map.thingGrid.ThingsListAt(c);
                foreach (var t in thingList)
                    if (t is Building building)
                        returnList.Add(building);

                return returnList;
            }
        }

        // TODO: Possibly verify the options based on the functionality as implemented in FloatMenuMap
        public class Helper
        {
            private static bool OptionsMatch(FloatMenuOption a, FloatMenuOption b)
            {
                return a.Label == b.Label;
            }

            public static bool StillValid(
                FloatMenuOption opt,
                IEnumerable<FloatMenuOption> curOpts,
                Pawn forPawn)
            {
                var cachedChoices = (List<FloatMenuOption>) null;
                var cachedChoicesForPos = new Vector3(-9999f, -9999f, -9999f);
                return StillValid(opt, curOpts, forPawn, ref cachedChoices, ref cachedChoicesForPos);
            }

            private static bool StillValid(
                FloatMenuOption opt,
                IEnumerable<FloatMenuOption> curOpts,
                Pawn forPawn,
                ref List<FloatMenuOption> cachedChoices,
                ref Vector3 cachedChoicesForPos)
            {
                if (opt.revalidateClickTarget == null) return curOpts.Any(t => OptionsMatch(opt, t));

                {
                    if (!opt.revalidateClickTarget.Spawned)
                        return false;

                    var vector3Shifted = opt.revalidateClickTarget.Position.ToVector3Shifted();
                    List<FloatMenuOption> floatMenuOptionList;
                    if (vector3Shifted == cachedChoicesForPos)
                    {
                        floatMenuOptionList = cachedChoices;
                    }
                    else
                    {
                        cachedChoices = FloatMenuMakerMap.ChoicesAtFor(vector3Shifted, forPawn);
                        cachedChoicesForPos = vector3Shifted;
                        floatMenuOptionList = cachedChoices;
                    }

                    return (from t in floatMenuOptionList where OptionsMatch(opt, t) select !t.Disabled).FirstOrDefault();
                }
            }
        }

        public class Listing_Extended : Listing_Standard
        {
            public void CheckboxNonLabeled(ref bool checkOn, string tooltip = null, bool leftAligned = false)
            {
                var rect = GetRect(Text.LineHeight);
                if (!tooltip.NullOrEmpty())
                {
                    if (Mouse.IsOver(rect))
                        Widgets.DrawHighlight(rect);
                    TooltipHandler.TipRegion(rect, (TipSignal) tooltip);
                }

                float x;

                if (leftAligned)
                    x = rect.x;
                else
                    x = rect.x + rect.width - 24f;

                Widgets.Checkbox(x, rect.y, ref checkOn);

                Gap(verticalSpacing);
            }

            public int SliderInt(int val, int min, int max)
            {
                var num = (int) Widgets.HorizontalSlider(GetRect(22f), val, min, max, roundTo: 1f);
                if (num != val)
                    SoundDefOf.DragSlider.PlayOneShotOnCamera();
                Gap(verticalSpacing);
                return num;
            }
        }

        public class Elements
        {
            // Credits to Dubwise for this awesome function
            public static void InputField(
                string name,
                Rect rect,
                ref string buff,
                Texture icon = null,
                int max = 999,
                bool readOnly = false,
                bool forceFocus = false,
                bool ShowName = false)
            {
                if (buff == null)
                    buff = "";

                if (icon != null)
                {
                    var outerRect = rect;
                    outerRect.width = outerRect.height;
                    Widgets.DrawTextureFitted(outerRect, icon, 1f);
                    rect.width -= outerRect.width;
                    rect.x += outerRect.width;
                }

                if (ShowName)
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(rect.LeftPart(0.2f), name);
                    Text.Anchor = TextAnchor.UpperLeft;
                    rect = rect.RightPart(0.8f);
                }

                GUI.SetNextControlName(name);
                buff = GUI.TextField(rect, buff, max, Text.CurTextAreaStyle);
                var flag = GUI.GetNameOfFocusedControl() == name;
                if (!flag & forceFocus)
                    GUI.FocusControl(name);
                if (((!Input.GetMouseButtonDown(0) ? 0 : !Mouse.IsOver(rect) ? 1 : 0) & (flag ? 1 : 0)) != 0)
                    GUI.FocusControl(null);
            }

            public static bool ButtonInvisibleLabeled(Color textColor, GameFont textSize, Rect inRect, string label)
            {
                GUI.color = textColor;
                Text.Font = textSize;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(inRect, label);
                Text.Anchor = TextAnchor.UpperLeft;
                return Widgets.ButtonInvisible(inRect.ContractedBy(2f));
            }

            public static bool ButtonInvisibleLabeledFree(Color textColor, GameFont textSize, Rect inRect, string label, GUIStyle style)
            {
                GUI.color = textColor;
                Text.Font = textSize;
                Text.Anchor = TextAnchor.MiddleCenter;
                LabelFree(inRect, label, style);
                Text.Anchor = TextAnchor.UpperLeft;
                return Widgets.ButtonInvisible(inRect.ContractedBy(2f));
            }

            public static void SolidColorBG(Rect inRect, Color inColor)
            {
                GUI.DrawTexture(inRect, SolidColorMaterials.NewSolidColorTexture(inColor));
            }

            public static void SeparatorHorizontal(float x, float y, float len)
            {
                GUI.color = Color.grey;
                Widgets.DrawLineHorizontal(x, y, len);
                GUI.color = Color.white;
            }

            public static void SeparatorVertical(float x, float y, float len)
            {
                GUI.color = Color.grey;
                Widgets.DrawLineVertical(x, y, len);
                GUI.color = Color.white;
            }

            public static bool ButtonImageFittedScaled(Rect butRect, Texture2D tex, float scale)
            {
                return ButtonImageFittedScaled(butRect, tex, Color.white, scale);
            }

            public static bool ButtonImageFittedScaled(Rect butRect, Texture2D tex, Color baseColor, float scale)
            {
                return ButtonImageFittedScaled(butRect, tex, baseColor, GenUI.MouseoverColor, scale);
            }

            public static bool ButtonImageFittedScaled(
                Rect butRect,
                Texture2D tex,
                Color baseColor,
                Color mouseoverColor,
                float scale)
            {
                GUI.color = !Mouse.IsOver(butRect) ? baseColor : mouseoverColor;
                Widgets.DrawTextureFitted(butRect, tex, scale);
                GUI.color = baseColor;
                return Widgets.ButtonInvisible(butRect);
            }
            
            public static void LabelFree(Rect rect, string label, GUIStyle style)
            {
                var position = rect;
                var f = Prefs.UIScale / 2f;
                if ((double) Prefs.UIScale > 1.0 && (double) Math.Abs(f - Mathf.Floor(f)) > 1.40129846432482E-45)
                {
                    position.xMin = Widgets.AdjustCoordToUIScalingFloor(rect.xMin);
                    position.yMin = Widgets.AdjustCoordToUIScalingFloor(rect.yMin);
                    position.xMax = Widgets.AdjustCoordToUIScalingCeil(rect.xMax + 1E-05f);
                    position.yMax = Widgets.AdjustCoordToUIScalingCeil(rect.yMax + 1E-05f);
                }
                
                GUI.Label(position, label, style);
            }

            public static void TryMakeFloatMenu(Pawn pawn, List<FloatMenuOption> options, string title)
            {
                if (options.Count == 0)
                    return;

                var flag = true;

                var floatMenuOption = (FloatMenuOption) null;
                foreach (var option in options)
                {
                    if (option.Disabled || !option.autoTakeable)
                    {
                        flag = false;
                        break;
                    }

                    if (floatMenuOption == null || option.autoTakeablePriority > floatMenuOption.autoTakeablePriority)
                        floatMenuOption = option;
                }

                if (flag && floatMenuOption != null)
                {
                    floatMenuOption.Chosen(true, null);
                }
                else
                {
                    if (DSGUIMod.settings.DSGUI_SortOrders && options.Count > 1)
                        options = options.OrderBy(x => x.Label).ToList();
                    
                    var floatMenuMap = new FloatMenu(options, title) {givesColonistOrders = true};
                    Find.WindowStack.Add(floatMenuMap);
                }
            }
        }
    }
}
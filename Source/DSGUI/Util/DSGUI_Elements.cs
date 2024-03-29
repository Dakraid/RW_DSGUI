﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DSGUI {
    public partial class DSGUI {
        public static class StaticHelper {
            public static IEnumerable<Building> GetBuildings(IntVec3 c, Map map) {
                var returnList = new List<Building>();
                var thingList  = map.thingGrid.ThingsListAt(c);
                foreach (var t in thingList)
                    if (t is Building building)
                        returnList.Add(building);

                return returnList;
            }
        }

        public class Listing_Extended : Listing_Standard {
            public void CheckboxNonLabeled(ref bool checkOn, string tooltip = null, bool leftAligned = false) {
                var rect = GetRect(Text.LineHeight);
                if (!tooltip.OptimizedNullOrEmpty()) {
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

            public int SliderInt(int val, int min, int max) {
                var num = (int) Widgets.HorizontalSlider(GetRect(22f), val, min, max, roundTo: 1f);
                if (num != val)
                    SoundDefOf.DragSlider.PlayOneShotOnCamera();

                Gap(verticalSpacing);
                return num;
            }

            public void BeginScrollView(Rect rect, ref Vector2 scrollPosition, ref Rect viewRect)
            {
                Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
                rect.height =  100000f;
                rect.width  -= 20f;
                this.Begin(rect.AtZero());
            }

            public void EndScrollView(ref Rect viewRect)
            {
                viewRect = new Rect(0.0f, 0.0f, this.listingRect.width, this.curY);
                Widgets.EndScrollView();
                this.End();
            }
        }

        [UsedImplicitly]
        public class Elements
        {
            private static readonly GUIContent _tempGuiContent = new GUIContent();
            private static readonly MethodInfo _doTextFieldMethod = AccessTools.Method(typeof(GUI), "DoTextField", new[] {typeof(Rect), typeof(int), typeof(GUIContent), typeof(bool), typeof(int), typeof(GUIStyle)});

            // Credits to Dubwise for this awesome function
            public static void InputField(
                string     name,
                Rect       rect,
                ref string buff,
                Texture    icon       = null,
                int        max        = 999,
                bool       readOnly   = false,
                bool       forceFocus = false,
                bool       showName   = false) {
                Text.Font   =   GameFont.Small;
                Text.Anchor =   TextAnchor.MiddleCenter;
                buff        ??= "";

                if (icon != null) {
                    var outerRect = rect;
                    outerRect.width = outerRect.height;
                    Widgets.DrawTextureFitted(outerRect, icon, 1f);
                    rect.width -= outerRect.width;
                    rect.x     += outerRect.width;
                }

                if (showName) {
                    Widgets.Label(rect.LeftPart(0.2f), name);
                    rect = rect.RightPart(0.8f);
                }

                GUI.SetNextControlName(name);

                _tempGuiContent.text = buff; 
                _doTextFieldMethod.Invoke(null, new object[]
                {
                    rect, 80000 + name.GetHashCode(), _tempGuiContent, false, max, Text.CurTextFieldStyle
                });
			    buff = _tempGuiContent.text;

                var flag = GUI.GetNameOfFocusedControl() == name;
                if (!flag & forceFocus)
                    GUI.FocusControl(name);
                if (Input.GetMouseButtonDown(0) && !Mouse.IsOver(rect) && flag)
                    GUI.FocusControl(null);

                Text.Anchor = TextAnchor.UpperLeft;
            }

            public static void SearchBar(Rect rect, float gap, ref string input) {
                var searchRect  = new Rect(rect) {height = 28f};
                var searchField = searchRect.LeftPartPixels(searchRect.width - 28f - 1f - gap);
                var clearBtn    = searchRect.RightPartPixels(28f + 1f);
                InputField("Search", searchField, ref input);
                Text.Anchor = TextAnchor.MiddleLeft;
                if (Widgets.ButtonImageFitted(clearBtn, Widgets.CheckboxOffTex))
                    input = "";
            }

            public static void LabelAnchored(Rect rect, string label, TextAnchor textAnchor) {
                Text.Anchor = textAnchor;
                Widgets.Label(rect, label);
                Text.Anchor = TextAnchor.UpperLeft;
            }

            public static void DrawTextureFittedSized(Rect outerRect, Texture tex, float scale, float width, float height) {
                Widgets.DrawTextureFitted(outerRect, tex, scale, new Vector2(width, height), new Rect(0.0f, 0.0f, 1f, 1f));
            }

            public static void DrawIconFittedSized(Rect iconRect, Texture thingIcon, Color thingColor, float iconScale, float width, float height) {
                GUI.color = thingColor;
                DrawTextureFittedSized(iconRect, thingIcon, iconScale, width, height);
                GUI.color = Color.white;
            }

            public static void DrawIconFitted(Rect iconRect, Texture thingIcon, Color thingColor, float iconScale) {
                GUI.color = thingColor;
                Widgets.DrawTextureFitted(iconRect, thingIcon, iconScale);
                GUI.color = Color.white;
            }

            public static bool ButtonInvisibleLabeled(Color textColor, GameFont textSize, Rect inRect, string label, TextAnchor anchor = TextAnchor.MiddleCenter) {
                GUI.color   = textColor;
                Text.Font   = textSize;
                Text.Anchor = anchor;
                Widgets.Label(inRect, label);
                Text.Anchor = TextAnchor.UpperLeft;
                return Widgets.ButtonInvisible(inRect.ContractedBy(2f));
            }

            public static bool ButtonInvisibleLabeledFree(Color textColor, GameFont textSize, Rect inRect, string label, GUIStyle style) {
                GUI.color   = textColor;
                Text.Font   = textSize;
                Text.Anchor = TextAnchor.MiddleCenter;
                LabelFree(inRect, label, style);
                Text.Anchor = TextAnchor.UpperLeft;
                return Widgets.ButtonInvisible(inRect.ContractedBy(2f));
            }

            public static void SolidColorBG(Rect inRect, Color inColor) {
                GUI.DrawTexture(inRect, SolidColorMaterials.NewSolidColorTexture(inColor));
            }

            public static void SeparatorHorizontal(float x, float y, float len) {
                GUI.color = Color.grey;
                Widgets.DrawLineHorizontal(x, y, len);
                GUI.color = Color.white;
            }

            public static void SeparatorVertical(float x, float y, float len) {
                GUI.color = Color.grey;
                Widgets.DrawLineVertical(x, y, len);
                GUI.color = Color.white;
            }

            public static bool ButtonImageFittedScaled(Rect butRect, Texture2D tex, float scale) {
                return ButtonImageFittedScaled(butRect, tex, Color.white, scale);
            }

            public static bool ButtonImageFittedScaled(Rect butRect, Texture2D tex, Color baseColor, float scale) {
                return ButtonImageFittedScaled(butRect, tex, baseColor, GenUI.MouseoverColor, scale);
            }

            public static bool ButtonImageFittedScaled(
                Rect      butRect,
                Texture2D tex,
                Color     baseColor,
                Color     mouseoverColor,
                float     scale) {
                GUI.color = !Mouse.IsOver(butRect) ? baseColor : mouseoverColor;
                Widgets.DrawTextureFitted(butRect, tex, scale);
                GUI.color = baseColor;
                return Widgets.ButtonInvisible(butRect);
            }

            public static void LabelFree(Rect rect, string label, GUIStyle style) {
                var position = rect;
                var f        = Prefs.UIScale / 2f;
                if (Prefs.UIScale > 1.0 && Math.Abs(f - Mathf.Floor(f)) > 1.40129846432482E-45) {
                    position.xMin = Widgets.AdjustCoordToUIScalingFloor(rect.xMin);
                    position.yMin = Widgets.AdjustCoordToUIScalingFloor(rect.yMin);
                    position.xMax = Widgets.AdjustCoordToUIScalingCeil(rect.xMax + 1E-05f);
                    position.yMax = Widgets.AdjustCoordToUIScalingCeil(rect.yMax + 1E-05f);
                }

                GUI.Label(position, label, style);
            }

            public static void DrawThingIcon(Rect rect, Thing thing, float scale = 1f) {
                thing     = thing.GetInnerIfMinified();
                GUI.color = thing.DrawColor;
                var     resolvedIconAngle = 0.0f;
                Texture resolvedIcon;
                if (!thing.def.uiIconPath.OptimizedNullOrEmpty()) {
                    resolvedIcon      =  thing.def.uiIcon;
                    resolvedIconAngle =  thing.def.uiIconAngle;
                    rect.position     += new Vector2(thing.def.uiIconOffset.x * rect.size.x, thing.def.uiIconOffset.y * rect.size.y);
                }
                else {
                    switch (thing) {
                        case Pawn _:
                        case Corpse _:
                            if (!(thing is Pawn pawn))
                                pawn = ((Corpse) thing).InnerPawn;

                            if (!pawn.RaceProps.Humanlike) {
                                if (!pawn.Drawer.renderer.graphics.AllResolved)
                                    pawn.Drawer.renderer.graphics.ResolveAllGraphics();

                                var material = pawn.Drawer.renderer.graphics.nakedGraphic.MatAt(Rot4.East);
                                resolvedIcon = material.mainTexture;
                                GUI.color    = material.color;
                                break;
                            }

                            rect         =  rect.ScaledBy(1.8f);
                            rect.y       += 3f;
                            rect         =  rect.Rounded();
                            resolvedIcon =  PortraitsCache.Get(pawn, new Vector2(rect.width, rect.height), pawn.Rotation);
                            break;

                        default:
                            resolvedIcon = thing.Graphic.ExtractInnerGraphicFor(thing).MatAt(thing.def.defaultPlacingRot).mainTexture;
                            break;
                    }
                }

                ThingIconWorker(rect, thing.def, resolvedIcon, resolvedIconAngle, scale);
                GUI.color = Color.white;
            }

            private static void ThingIconWorker(
                Rect     rect,
                ThingDef thingDef,
                Texture  resolvedIcon,
                float    resolvedIconAngle,
                float    scale = 1f) {
                var texProportions = new Vector2(resolvedIcon.width, resolvedIcon.height);
                var texCoords      = new Rect(0.0f, 0.0f, 1f, 1f);
                if (thingDef.graphicData != null) {
                    texProportions = thingDef.graphicData.drawSize.RotatedBy(thingDef.defaultPlacingRot);
                    if (thingDef.uiIconPath.OptimizedNullOrEmpty() && thingDef.graphicData.linkFlags != LinkFlags.None)
                        texCoords = new Rect(0.0f, 0.5f, 0.25f, 0.25f);
                }

                Widgets.DrawTextureFitted(rect, resolvedIcon, GenUI.IconDrawScale(thingDef) * scale, texProportions, texCoords, resolvedIconAngle);
            }

            public static void TryMakeFloatMenu(List<FloatMenuOption> options, string title) {
                if (options.Count == 0)
                    return;

                var flag            = true;
                var floatMenuOption = (FloatMenuOption) null;
                foreach (var option in options) {
                    if (option.Disabled || !option.autoTakeable) {
                        flag = false;
                        break;
                    }

                    if (floatMenuOption == null || option.autoTakeablePriority > floatMenuOption.autoTakeablePriority)
                        floatMenuOption = option;
                }

                if (flag && floatMenuOption != null) {
                    floatMenuOption.Chosen(true, null);
                }
                else {
                    if (DSGUIMod.Settings.DSGUI_List_SortOrders && options.Count > 1)
                        options = options.OrderBy(x => x.Label).ToList();

                    var floatMenuMap = new FloatMenu(options, title) {givesColonistOrders = true};
                    Find.WindowStack.Add(floatMenuMap);
                }
            }
        }
    }
}
using System.Globalization;
using UnityEngine;
using Verse;

namespace DSGUI {
    public class DSGUISettings : ModSettings {
        public int   DSGUI_List_BoxHeight           = 32;
        public bool  DSGUI_List_DrawDividersColumns = true;
        public bool  DSGUI_List_DrawDividersRows    = true;
        public int   DSGUI_List_FontSize            = 14;
        public float DSGUI_List_IconScaling         = 1f;
        public bool  DSGUI_List_SavePosSize         = true;
        public bool  DSGUI_List_SortOrders          = true;
        public bool  DSGUI_Tab_AdvSortContent;
        public int   DSGUI_Tab_BoxHeight           = 32;
        public bool  DSGUI_Tab_DrawDividersColumns = true;
        public bool  DSGUI_Tab_DrawDividersRows    = true;

        public bool  DSGUI_Tab_EnableTab   = true;
        public int   DSGUI_Tab_FontSize    = 14;
        public float DSGUI_Tab_IconScaling = 1f;
        public bool  DSGUI_Tab_SortContent = true;

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref DSGUI_List_IconScaling, "DSGUI_IconScalingLabel");
            Scribe_Values.Look(ref DSGUI_List_BoxHeight, "DSGUI_BoxHeightLabel");
            Scribe_Values.Look(ref DSGUI_List_FontSize, "DSGUI_FontScalingLabel");
            Scribe_Values.Look(ref DSGUI_List_SortOrders, "DSGUI_SortOrdersLabel");
            Scribe_Values.Look(ref DSGUI_List_SavePosSize, "DSGUI_SavePosSizeLabel");
            Scribe_Values.Look(ref DSGUI_List_DrawDividersRows, "DSGUI_DrawDividersRowsLabel");
            Scribe_Values.Look(ref DSGUI_List_DrawDividersColumns, "DSGUI_DrawDividersColumnsLabel");
            Scribe_Values.Look(ref DSGUI_Tab_EnableTab, "DSGUI_Tab_DrawDividersColumnsLabel");
            Scribe_Values.Look(ref DSGUI_Tab_IconScaling, "DSGUI_Tab_IconScalingLabel");
            Scribe_Values.Look(ref DSGUI_Tab_BoxHeight, "DSGUI_Tab_BoxHeightLabel");
            Scribe_Values.Look(ref DSGUI_Tab_FontSize, "DSGUI_Tab_FontScalingLabel");
            Scribe_Values.Look(ref DSGUI_Tab_SortContent, "DSGUI_Tab_SortOrdersLabel");
            Scribe_Values.Look(ref DSGUI_Tab_DrawDividersRows, "DSGUI_Tab_DrawDividersRowsLabel");
            Scribe_Values.Look(ref DSGUI_Tab_DrawDividersColumns, "DSGUI_Tab_DrawDividersColumnsLabel");
        }
    }

    internal class DSGUIMod : Mod {
        public static DSGUISettings Settings;
        private       float         scrollHeight;
        private       Vector2       scrollPosition;

        public DSGUIMod(ModContentPack content) : base(content) {
            Settings = GetSettings<DSGUISettings>();
        }

        public override string SettingsCategory() {
            return "DSGUI_Label".TranslateSimple();
        }

        private static void ResetSettings() {
            Settings.DSGUI_List_BoxHeight           = 32;
            Settings.DSGUI_List_IconScaling         = 1f;
            Settings.DSGUI_List_FontSize            = 14;
            Settings.DSGUI_List_SortOrders          = true;
            Settings.DSGUI_List_SavePosSize         = true;
            Settings.DSGUI_List_DrawDividersRows    = true;
            Settings.DSGUI_List_DrawDividersColumns = true;
            Settings.DSGUI_Tab_EnableTab            = true;
            Settings.DSGUI_Tab_BoxHeight            = 32;
            Settings.DSGUI_Tab_IconScaling          = 1f;
            Settings.DSGUI_Tab_FontSize             = 14;
            Settings.DSGUI_Tab_SortContent          = true;
            Settings.DSGUI_Tab_AdvSortContent       = false;
            Settings.DSGUI_Tab_DrawDividersRows     = true;
            Settings.DSGUI_Tab_DrawDividersColumns  = true;
        }

        public override void DoSettingsWindowContents(Rect inRect) {
            var ls       = new DSGUI.Listing_Extended {verticalSpacing = 8f};
            var viewRect = new Rect(0, 0, inRect.width - 16f, scrollHeight);
            ls.BeginScrollView(inRect, ref scrollPosition, ref viewRect);
            GUI.BeginGroup(viewRect);
            ls.GapLine();
            ls.Label("DSGUI_Warn".TranslateSimple());
            ls.GapLine();
            Text.Anchor = TextAnchor.MiddleCenter;
            ls.Label("DSGUI_List_Label".TranslateSimple());
            Text.Anchor = TextAnchor.UpperLeft;
            ls.GapLine();
            ls.Label("DSGUI_List_SortOrders".TranslateSimple());
            ls.CheckboxNonLabeled(ref Settings.DSGUI_List_SortOrders);
            ls.Label("DSGUI_List_SavePosSize".TranslateSimple());
            ls.CheckboxNonLabeled(ref Settings.DSGUI_List_SavePosSize);
            ls.Label("DSGUI_List_DrawDividersRows".TranslateSimple());
            ls.CheckboxNonLabeled(ref Settings.DSGUI_List_DrawDividersRows);
            ls.Label("DSGUI_List_DrawDividersColumns".TranslateSimple());
            ls.CheckboxNonLabeled(ref Settings.DSGUI_List_DrawDividersColumns);
            ls.LabelDouble("DSGUI_List_IconScaling".TranslateSimple(), Settings.DSGUI_List_IconScaling.ToString(CultureInfo.CurrentCulture));
            Settings.DSGUI_List_IconScaling = ls.Slider(Settings.DSGUI_List_IconScaling, 0f, 2f);
            ls.LabelDouble("DSGUI_List_FontSize".TranslateSimple(), Settings.DSGUI_List_FontSize.ToString(CultureInfo.CurrentCulture));
            Settings.DSGUI_List_FontSize = ls.SliderInt(Settings.DSGUI_List_FontSize, 8, 32);
            ls.LabelDouble("DSGUI_List_BoxHeight".TranslateSimple(), Settings.DSGUI_List_BoxHeight.ToString(CultureInfo.CurrentCulture));
            Settings.DSGUI_List_BoxHeight = ls.SliderInt(Settings.DSGUI_List_BoxHeight, 4, 64);
            ls.GapLine();
            ls.GapLine();
            Text.Anchor = TextAnchor.MiddleCenter;
            ls.Label("DSGUI_Tab_Label".TranslateSimple());
            Text.Anchor = TextAnchor.UpperLeft;
            ls.GapLine();
            ls.Label("DSGUI_Tab_EnableTab".TranslateSimple());
            ls.CheckboxNonLabeled(ref Settings.DSGUI_Tab_EnableTab);
            ls.Label("DSGUI_Tab_SortContent".TranslateSimple());
            ls.CheckboxNonLabeled(ref Settings.DSGUI_Tab_SortContent);
            ls.Label("DSGUI_Tab_AdvSortContent".TranslateSimple());
            ls.CheckboxNonLabeled(ref Settings.DSGUI_Tab_AdvSortContent);
            ls.Label("DSGUI_Tab_DrawDividersRows".TranslateSimple());
            ls.CheckboxNonLabeled(ref Settings.DSGUI_Tab_DrawDividersRows);
            ls.Label("DSGUI_Tab_DrawDividersColumns".TranslateSimple());
            ls.CheckboxNonLabeled(ref Settings.DSGUI_Tab_DrawDividersColumns);
            ls.LabelDouble("DSGUI_Tab_IconScaling".TranslateSimple(), Settings.DSGUI_Tab_IconScaling.ToString(CultureInfo.CurrentCulture));
            Settings.DSGUI_Tab_IconScaling = ls.Slider(Settings.DSGUI_Tab_IconScaling, 0f, 2f);
            ls.LabelDouble("DSGUI_Tab_FontSize".TranslateSimple(), Settings.DSGUI_Tab_FontSize.ToString(CultureInfo.CurrentCulture));
            Settings.DSGUI_Tab_FontSize = ls.SliderInt(Settings.DSGUI_Tab_FontSize, 8, 32);
            ls.LabelDouble("DSGUI_Tab_BoxHeight".TranslateSimple(), Settings.DSGUI_Tab_BoxHeight.ToString(CultureInfo.CurrentCulture));
            Settings.DSGUI_Tab_BoxHeight = ls.SliderInt(Settings.DSGUI_Tab_BoxHeight, 4, 64);
            ls.GapLine();
            if (ls.ButtonText("DSGUI_ResetBtn".TranslateSimple())) ResetSettings();
            ls.GapLine();
            scrollHeight = ls.CurHeight;
            GUI.EndGroup();
            ls.EndScrollView(ref viewRect);
            Settings.Write();
        }
    }
}
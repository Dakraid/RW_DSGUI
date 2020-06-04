using System.Globalization;
using UnityEngine;
using Verse;

namespace DSGUI
{
    public class DSGUISettings : ModSettings
    {
        public int DSGUI_List_BoxHeight = 32;
        public bool DSGUI_List_DrawDividersColumns = true;
        public bool DSGUI_List_DrawDividersRows = true;
        public int DSGUI_List_FontSize = 1;
        public float DSGUI_List_IconScaling = 1f;
        public bool DSGUI_List_SavePosSize = true;
        public bool DSGUI_List_SortOrders = true;
        public int DSGUI_Tab_BoxHeight = 32;
        public bool DSGUI_Tab_DrawDividersColumns = true;
        public bool DSGUI_Tab_DrawDividersRows = true;

        public bool DSGUI_Tab_EnableTab = true;
        public int DSGUI_Tab_FontSize = 1;
        public float DSGUI_Tab_IconScaling = 1f;
        public bool DSGUI_Tab_SortContent = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref DSGUI_List_IconScaling, "DSGUI_IconScalingLabel");
            Scribe_Values.Look(ref DSGUI_List_BoxHeight, "DSGUI_BoxHeightLabel");
            Scribe_Values.Look(ref DSGUI_List_FontSize, "DSGUI_FontScalingLabel");
            Scribe_Values.Look(ref DSGUI_List_SortOrders, "DSGUI_SortOrdersLabel");
            Scribe_Values.Look(ref DSGUI_List_SavePosSize, "DSGUI_SavePosSizeLabel");
            Scribe_Values.Look(ref DSGUI_List_DrawDividersRows, "DSGUI_DrawDividersRowsLabel");
            Scribe_Values.Look(ref DSGUI_List_DrawDividersColumns, "DSGUI_DrawDividersColumnsLabel");
            Scribe_Values.Look(ref DSGUI_Tab_EnableTab, "DSGUI_DrawDividersColumnsLabel");
        }
    }

    internal class DSGUIMod : Mod
    {
        public static DSGUISettings settings;
        private float scrollHeight;
        private Vector2 scrollPosition;

        public DSGUIMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<DSGUISettings>();
        }

        public override string SettingsCategory()
        {
            return "DSGUI_Label".Translate();
        }

        private static void ResetSettings()
        {
            settings.DSGUI_List_BoxHeight = 32;
            settings.DSGUI_List_IconScaling = 1f;
            settings.DSGUI_List_FontSize = 14;
            settings.DSGUI_List_SortOrders = true;
            settings.DSGUI_List_SavePosSize = true;
            settings.DSGUI_List_DrawDividersRows = true;
            settings.DSGUI_List_DrawDividersColumns = true;

            settings.DSGUI_Tab_EnableTab = true;
            settings.DSGUI_Tab_BoxHeight = 32;
            settings.DSGUI_Tab_IconScaling = 1f;
            settings.DSGUI_Tab_FontSize = 14;
            settings.DSGUI_Tab_SortContent = true;
            settings.DSGUI_Tab_DrawDividersRows = true;
            settings.DSGUI_Tab_DrawDividersColumns = true;
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var ls = new DSGUI.Listing_Extended {verticalSpacing = 8f};
            var viewRect = new Rect(0, 0, inRect.width - 16f, scrollHeight);

            ls.BeginScrollView(inRect, ref scrollPosition, ref viewRect);
            GUI.BeginGroup(viewRect);
            ls.GapLine();

            ls.Label("DSGUI_Warn".Translate());

            ls.GapLine();
            Text.Anchor = TextAnchor.MiddleCenter;
            ls.Label("DSGUI_List_Label".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            ls.GapLine();

            ls.Label("DSGUI_List_SortOrders".Translate());
            ls.CheckboxNonLabeled(ref settings.DSGUI_List_SortOrders);

            ls.Label("DSGUI_List_SavePosSize".Translate());
            ls.CheckboxNonLabeled(ref settings.DSGUI_List_SavePosSize);

            ls.Label("DSGUI_List_DrawDividersRows".Translate());
            ls.CheckboxNonLabeled(ref settings.DSGUI_List_DrawDividersRows);

            ls.Label("DSGUI_List_DrawDividersColumns".Translate());
            ls.CheckboxNonLabeled(ref settings.DSGUI_List_DrawDividersColumns);

            ls.LabelDouble("DSGUI_List_IconScaling".Translate(), settings.DSGUI_List_IconScaling.ToString(CultureInfo.CurrentCulture));
            settings.DSGUI_List_IconScaling = ls.Slider(settings.DSGUI_List_IconScaling, 0f, 2f);

            ls.LabelDouble("DSGUI_List_FontSize".Translate(), settings.DSGUI_List_FontSize.ToString(CultureInfo.CurrentCulture));
            settings.DSGUI_List_FontSize = ls.SliderInt(settings.DSGUI_List_FontSize, 8, 32);

            ls.LabelDouble("DSGUI_List_BoxHeight".Translate(), settings.DSGUI_List_BoxHeight.ToString(CultureInfo.CurrentCulture));
            settings.DSGUI_List_BoxHeight = ls.SliderInt(settings.DSGUI_List_BoxHeight, 4, 64);

            ls.GapLine();

            ls.GapLine();
            Text.Anchor = TextAnchor.MiddleCenter;
            ls.Label("DSGUI_Tab_Label".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            ls.GapLine();

            ls.Label("DSGUI_Tab_EnableTab".Translate());
            ls.CheckboxNonLabeled(ref settings.DSGUI_Tab_EnableTab);

            ls.Label("DSGUI_Tab_SortContent".Translate());
            ls.CheckboxNonLabeled(ref settings.DSGUI_Tab_SortContent);

            ls.Label("DSGUI_Tab_DrawDividersRows".Translate());
            ls.CheckboxNonLabeled(ref settings.DSGUI_Tab_DrawDividersRows);

            ls.Label("DSGUI_Tab_DrawDividersColumns".Translate());
            ls.CheckboxNonLabeled(ref settings.DSGUI_Tab_DrawDividersColumns);

            ls.LabelDouble("DSGUI_Tab_IconScaling".Translate(), settings.DSGUI_Tab_IconScaling.ToString(CultureInfo.CurrentCulture));
            settings.DSGUI_Tab_IconScaling = ls.Slider(settings.DSGUI_Tab_IconScaling, 0f, 2f);

            ls.LabelDouble("DSGUI_Tab_FontSize".Translate(), settings.DSGUI_Tab_FontSize.ToString(CultureInfo.CurrentCulture));
            settings.DSGUI_Tab_FontSize = ls.SliderInt(settings.DSGUI_Tab_FontSize, 8, 32);

            ls.LabelDouble("DSGUI_Tab_BoxHeight".Translate(), settings.DSGUI_Tab_BoxHeight.ToString(CultureInfo.CurrentCulture));
            settings.DSGUI_Tab_BoxHeight = ls.SliderInt(settings.DSGUI_Tab_BoxHeight, 4, 64);

            ls.GapLine();

            if (ls.ButtonText("DSGUI_ResetBtn".Translate())) ResetSettings();

            ls.GapLine();

            scrollHeight = ls.CurHeight;

            GUI.EndGroup();
            ls.EndScrollView(ref viewRect);

            settings.Write();
        }
    }
}
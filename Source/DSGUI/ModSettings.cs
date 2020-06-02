using System.Globalization;
using UnityEngine;
using Verse;

namespace DSGUI
{
    public class DSGUISettings : ModSettings
    {
        public float DSGUI_IconScaling = 1f;
        public int DSGUI_BoxHeight = 32;
        public int DSGUI_FontSize = 1;
        public bool DSGUI_DrawDividersRows = true;
        public bool DSGUI_DrawDividersColumns = true;
        public bool DSGUI_SavePosSize = true;
        public bool DSGUI_SortOrders = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref DSGUI_IconScaling, "DSGUI_IconScalingLabel");
            Scribe_Values.Look(ref DSGUI_BoxHeight, "DSGUI_BoxHeightLabel");
            Scribe_Values.Look(ref DSGUI_FontSize, "DSGUI_FontScalingLabel");
            Scribe_Values.Look(ref DSGUI_SortOrders, "DSGUI_SortOrdersLabel");
            Scribe_Values.Look(ref DSGUI_SavePosSize, "DSGUI_SavePosSizeLabel");
            Scribe_Values.Look(ref DSGUI_DrawDividersRows, "DSGUI_DrawDividersRowsLabel");
            Scribe_Values.Look(ref DSGUI_DrawDividersColumns, "DSGUI_DrawDividersColumnsLabel");
        }
    }

    internal class DSGUIMod : Mod
    {
        public static DSGUISettings settings;
        private Vector2 scrollPosition;
        private float scrollHeight;

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
            settings.DSGUI_BoxHeight = 32;
            settings.DSGUI_IconScaling = 1f;
            settings.DSGUI_FontSize = 14;
            settings.DSGUI_SortOrders = true;
            settings.DSGUI_SavePosSize = true;
            settings.DSGUI_DrawDividersRows = true;
            settings.DSGUI_DrawDividersColumns = true;
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

            ls.Label("DSGUI_SortOrders".Translate());
            ls.CheckboxNonLabeled(ref settings.DSGUI_SortOrders);
            
            ls.Label("DSGUI_SavePosSize".Translate());
            ls.CheckboxNonLabeled(ref settings.DSGUI_SavePosSize);

            ls.Label("DSGUI_DrawDividersRows".Translate());
            ls.CheckboxNonLabeled(ref settings.DSGUI_DrawDividersRows);

            ls.Label("DSGUI_DrawDividersColumns".Translate());
            ls.CheckboxNonLabeled(ref settings.DSGUI_DrawDividersColumns);

            ls.LabelDouble("DSGUI_IconScaling".Translate(), settings.DSGUI_IconScaling.ToString(CultureInfo.CurrentCulture));
            settings.DSGUI_IconScaling = ls.Slider(settings.DSGUI_IconScaling, 0f, 2f);

            ls.LabelDouble("DSGUI_FontSize".Translate(), settings.DSGUI_FontSize.ToString(CultureInfo.CurrentCulture));
            settings.DSGUI_FontSize = ls.SliderInt(settings.DSGUI_FontSize, 8, 32);

            ls.LabelDouble("DSGUI_BoxHeight".Translate(), settings.DSGUI_BoxHeight.ToString(CultureInfo.CurrentCulture));
            settings.DSGUI_BoxHeight = ls.SliderInt(settings.DSGUI_BoxHeight, 4, 64);

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
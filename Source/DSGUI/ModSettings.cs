using System.Globalization;
using Verse;
using UnityEngine;

namespace DSGUI
{
    public class DSGUISettings : ModSettings
    {
        public float DSGUI_IconScaling = 1f;
        public float DSGUI_BoxHeight = 32f;
        public int DSGUI_FontSize = 1;
        public bool DSGUI_SortOrders = true;
        public bool DSGUI_DrawDividers = true;
        public bool DSGUI_UseTranspiler = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref DSGUI_IconScaling, "DSGUI_IconScalingLabel");
            Scribe_Values.Look(ref DSGUI_BoxHeight, "DSGUI_BoxHeightLabel");
            Scribe_Values.Look(ref DSGUI_FontSize, "DSGUI_FontSizeLabel");
            Scribe_Values.Look(ref DSGUI_SortOrders, "DSGUI_SortOrdersLabel");
            Scribe_Values.Look(ref DSGUI_DrawDividers, "DSGUI_DrawDividersLabel");
            Scribe_Values.Look(ref DSGUI_UseTranspiler, "DSGUI_UseTranspilerLabel");
        }
    }
    
    class DSGUIMod : Mod
    {
        public static DSGUISettings settings;

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
            settings.DSGUI_BoxHeight = 32f;
            settings.DSGUI_IconScaling = 1f;
            settings.DSGUI_SortOrders = true;
            settings.DSGUI_DrawDividers = true;
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var ls = new DSGUI.Listing_Extended();
            ls.Begin(inRect);
            ls.verticalSpacing = 8f;
            ls.Label("DSGUI_Warn".Translate());
            
            ls.GapLine();
            ls.Label("DSGUI_SortOrders".Translate());
            ls.CheckboxNonLabeled(ref settings.DSGUI_SortOrders);
            
            ls.Label("DSGUI_DrawDividers".Translate());
            ls.CheckboxNonLabeled(ref settings.DSGUI_DrawDividers);
            
            ls.LabelDouble("DSGUI_IconScaling".Translate(), settings.DSGUI_IconScaling.ToString(CultureInfo.CurrentCulture));
            settings.DSGUI_IconScaling = ls.Slider(settings.DSGUI_IconScaling, 0f, 2f);
            
            ls.LabelDouble("DSGUI_BoxHeight".Translate(), settings.DSGUI_BoxHeight.ToString(CultureInfo.CurrentCulture));
            settings.DSGUI_BoxHeight = ls.Slider(settings.DSGUI_BoxHeight, 4f, 64f);
            
            ls.Label("DSGUI_FontSize".Translate());
            // ls.RadioButton_NewTemp("DSGUI_FontTiny".Translate(),)
            
            ls.GapLine();
            if (ls.ButtonText("DSGUI_ResetBtn".Translate())) ResetSettings();
            
            ls.GapLine();
            ls.Label("DSGUI_AdvWarn".Translate());
            
            ls.Label("DSGUI_UseTranspiler".Translate());
            ls.CheckboxNonLabeled(ref settings.DSGUI_UseTranspiler);
            
            ls.GapLine();
            ls.End();
            settings.Write();
        }
    }
}
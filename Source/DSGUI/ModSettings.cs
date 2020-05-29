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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref DSGUI_IconScaling, "DSGUI_IconScalingLabel");
            Scribe_Values.Look(ref DSGUI_BoxHeight, "DSGUI_BoxHeightLabel");
            Scribe_Values.Look(ref DSGUI_FontSize, "DSGUI_FontSizeLabel");
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
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var ls = new Listing_Standard();
            ls.Begin(inRect);
            ls.verticalSpacing = 8f;
            ls.Label("DSGUI_Warn".Translate());
            ls.GapLine();
            
            ls.LabelDouble("DSGUI_IconScaling".Translate(), settings.DSGUI_IconScaling.ToString(CultureInfo.CurrentCulture));
            settings.DSGUI_IconScaling = ls.Slider(settings.DSGUI_IconScaling, 0f, 2f);
            
            ls.LabelDouble("DSGUI_BoxHeight".Translate(), settings.DSGUI_BoxHeight.ToString(CultureInfo.CurrentCulture));
            settings.DSGUI_BoxHeight = ls.Slider(settings.DSGUI_BoxHeight, 4f, 64f);
            
            ls.Label("DSGUI_FontSize".Translate());
            // ls.RadioButton_NewTemp("DSGUI_FontTiny".Translate(),)
            
            ls.GapLine();
            if (ls.ButtonText("DSGUI_ResetBtn".Translate())) ResetSettings();
            ls.End();
            settings.Write();
        }
    }
}
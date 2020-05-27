using Verse;
using UnityEngine;

namespace DSGUI
{
    public class DSGUISettings : ModSettings
    {
        public float DSGUI_IconScaling = 1f;
        public float DSGUI_BoxHeight = 32f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref DSGUI_IconScaling, "DSGUI_IconScalingLabel");
            Scribe_Values.Look(ref DSGUI_BoxHeight, "DSGUI_BoxHeightLabel");
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

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.verticalSpacing = 8f;
            listing_Standard.Label("DSGUI_Warn".Translate());
            listing_Standard.GapLine();
            listing_Standard.Label("DSGUI_IconScaling".Translate());
            settings.DSGUI_IconScaling = listing_Standard.Slider(settings.DSGUI_IconScaling, 0f, 1f);
            listing_Standard.Label("DSGUI_BoxHeight".Translate());
            settings.DSGUI_BoxHeight = listing_Standard.Slider(settings.DSGUI_BoxHeight, 4f, 64f);
            listing_Standard.End();
            settings.Write();
        }
    }
}
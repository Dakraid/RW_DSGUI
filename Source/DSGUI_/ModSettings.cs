using Verse;
using UnityEngine;

namespace DSGUI
{
    public class DSGUISettings : ModSettings
    {
        public static float DSGUI_IconScaling = 1f;
        public static float DSGUI_BoxHeight = 32f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref DSGUI_IconScaling, "DSGUI_IconScalingLabel");
            Scribe_Values.Look(ref DSGUI_BoxHeight, "DSGUI_BoxHeightLabel");
        }
    }
    
    class HuntersUseMeleeMod : Mod
    {
        public static HuntersUseMeleeSettings settings;

        public HuntersUseMeleeMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<HuntersUseMeleeSettings>();
        }

        public override string SettingsCategory() => "DSGUISettingsLabel".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.verticalSpacing = 8f;
            listing_Standard.Label("HuntersUseMeleeFistFightDesc".Translate());
            listing_Standard.CheckboxLabeled("HuntersUseMeleeFistFightingLabel".Translate() + ": ", ref settings.enableFistFighting);
            listing_Standard.Label("HuntersUseMeleeSidearmsDesc".Translate());
            listing_Standard.CheckboxLabeled("HuntersUseMeleeSimpleSidearmsLabel".Translate() + ": ", ref settings.enableSimpleSidearms);
            listing_Standard.End();
            settings.Write();
        }
    }
}
using System.Linq;
using HarmonyLib;
using LWM.DeepStorage;
using RimWorld;
using Verse;

namespace DSGUI
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static Selector selectInst;

        static HarmonyPatches()
        {
            var harmony = new Harmony("net.netrve.dsgui");

            // We patch all as we use annotations
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(FloatMenuMakerMap), "TryMakeFloatMenu")]
        private static class Patch_TryMakeFloatMenu
        {
            [HarmonyPriority(Priority.First)]
            private static bool Prefix(Pawn pawn)
            {
                return DSGUI.Create(UI.MouseMapPosition(), pawn);
            }
        }

        [HarmonyPatch(typeof(Selector), "Select")]
        public static class Patch_Select
        {
            [HarmonyPriority(Priority.First)]
            private static void Postfix(Selector __instance)
            {
                selectInst = __instance;
            }
        }

        [HarmonyPatch(typeof(Open_DS_Tab_On_Select), "Postfix")]
        public static class Patch_DeepStorage
        {
            [HarmonyPriority(Priority.First)]
            private static bool Prefix()
            {
                if (!DSGUIMod.settings.DSGUI_Tab_EnableTab)
                    return true;

                if (selectInst.NumSelected != 1)
                    return false;

                var t = selectInst.SingleSelectedThing;
                if (t == null)
                    return false;

                if (!(t is ThingWithComps))
                    return false;

                var cds = t.TryGetComp<CompDeepStorage>();
                if (cds == null)
                    return false;

                var pane = (MainTabWindow_Inspect) MainButtonDefOf.Inspect.TabWindow;
                var alreadyOpenTabType = pane.OpenTabType;
                if (alreadyOpenTabType != null)
                {
                    var listOfTabs = t.GetInspectTabs();
                    if (listOfTabs.Any(x => x.GetType() == alreadyOpenTabType)) return false;
                }

                ITab tab = null;

                if (t.Spawned && t is IStoreSettingsParent && t is ISlotGroupParent parent)
                {
                    foreach (var l in from c in parent.GetSlotGroup().CellsList
                        select t.Map.thingGrid.ThingsListAt(c)
                        into l
                        from tmp in l.Where(tmp => tmp.def.EverStorable(false))
                        select l) goto EndLoop;

                    tab = t.GetInspectTabs().OfType<ITab_Storage>().First();
                }

                EndLoop:
                if (tab == null && DSGUIMod.settings.DSGUI_Tab_EnableTab)
                    tab = t.GetInspectTabs().OfType<DSGUI_TabModal>().First();
                
                if (tab == null)
                    tab = t.GetInspectTabs().OfType<ITab_DeepStorage_Inventory>().First();
                
                if (tab == null)
                {
                    Log.Warning("[LWM] Deep Storage object " + t + " does not have an inventory tab?");
                    return false;
                }

                tab.OnOpen();
                if (tab is DSGUI_TabModal)
                    pane.OpenTabType = typeof(DSGUI_TabModal);
                else if (tab is ITab_DeepStorage_Inventory)
                    pane.OpenTabType = typeof(ITab_DeepStorage_Inventory);
                else
                    pane.OpenTabType = typeof(ITab_Storage);

                return false;
            }
        }
    }
}
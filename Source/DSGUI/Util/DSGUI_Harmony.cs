using System.Linq;
using AchtungMod;
using HarmonyLib;
using LWM.DeepStorage;
using RimWorld;
using UnityEngine;
using UnityEngine.UI;
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

            if (!DSGUIMain.modAchtungLoaded)
                return;
            
            var methodInfoAchtung = AccessTools.Method(typeof(Controller), "MouseDown");
            var prefixAchtung = typeof(Patch_Achtung_Controller).GetMethod("Prefix");

            harmony.Patch(methodInfoAchtung, new HarmonyMethod(prefixAchtung));
        }
        
        static class Patch_Achtung_Controller
        {
            private static Controller controller = Controller.GetInstance();
            
            public static bool Prefix(Vector3 pos)
            {
                var colonist = Find.Selector.SelectedObjects.OfType<Pawn>()
                    .Where(pawn => pawn.IsColonistPlayerControlled && pawn.Downed == false).ToList();
                
                if (colonist.EnumerableNullOrEmpty() || colonist.Count > 1)
                    return true;
                
                return DSGUI.Create(pos, colonist.First());
            }
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
                pane.OpenTabType = tab switch
                {
                    DSGUI_TabModal _ => typeof(DSGUI_TabModal),
                    ITab_DeepStorage_Inventory _ => typeof(ITab_DeepStorage_Inventory),
                    _ => typeof(ITab_Storage)
                };

                return false;
            }
        }
    }
}
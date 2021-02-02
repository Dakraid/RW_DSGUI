using System;
using System.Linq;
using HarmonyLib;
using LWM.DeepStorage;
using RimWorld;
using UnityEngine;
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
        
        [HarmonyBefore("net.pardeike.rimworld.mods.achtung")]
        [HarmonyPatch(typeof(Selector), "HandleMapClicks")]
        public static class Selector_HandleMapClicks_Patch
        {
            public static bool Prefix()
            {
                var result = true;
                var pos = UI.MouseMapPosition();
                var target = Find.Selector.SelectedObjects.OfType<Pawn>()
                    .Where(pawn => pawn.IsColonistPlayerControlled && pawn.Downed == false).ToList();
    
                if (target.OptimizedNullOrEmpty() || target.Count > 1)
                    return true;

                if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
                    result = DSGUI.Create(pos, target.First());

                return result;
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
                    foreach (var _ in from c in parent.GetSlotGroup().CellsList
                        select t.Map.thingGrid.ThingsListAt(c)
                        into l
                        from tmp in l.Where(tmp => tmp.def.EverStorable(false))
                        select l) goto EndLoop;

                    tab = t.GetInspectTabs().OfType<ITab_Storage>().First();
                }

                EndLoop:
                if (tab == null && DSGUIMod.settings.DSGUI_Tab_EnableTab)
                    try
                    {
                        tab = t.GetInspectTabs().OfType<DSGUI_TabModal>().First();
                    }
                    catch (Exception e)
                    {
                        Log.Warning("[DSGUI] Could not get DSGUI_TabModel, trying default. (" + e + ")");
                    }
                
                if (tab == null)
                    try
                    {
                        tab = t.GetInspectTabs().OfType<ITab_DeepStorage_Inventory>().First();
                    }
                    catch (Exception e)
                    {
                        Log.Warning("[DSGUI] Could not get ITab_DeepStorage_Inventory, trying default. (" + e + ")");
                    }

                if (tab == null)
                {
                    Log.Error("[DSGUI] Deep Storage object " + t + " does not have an inventory tab?");
                    return false;
                }

                tab.OnOpen();
                pane.OpenTabType = tab switch
                {
                    ITab_DeepStorage_Inventory _ => typeof(ITab_DeepStorage_Inventory),
                    DSGUI_TabModal _ => typeof(DSGUI_TabModal),
                    _ => typeof(DSGUI_TabModal)
                };

                return false;
            }
        }
    }
}
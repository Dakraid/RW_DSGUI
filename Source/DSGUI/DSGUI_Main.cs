using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using LWM.DeepStorage;
using RimWorld;
using UnityEngine;
using Verse;

// TODO: Add comments and explanations to the code
// TODO: Overwrite the Verse.Log.Message to allow toggling logging

namespace DSGUI {
    public static class GlobalStorage {
        public static Vector2 SavedSize = new Vector2(0, 0);
        public static Vector2 SavedPos  = new Vector2(0, 0);
    }

    [StaticConstructorOnStartup]
    public static class DSGUIMain {
        public const string PidSimpleStorage    = "jangodsoul.simplestorage";
        public const string PidSimpleRefStorage = "jangodsoul.simplestorage.ref";

        // public static bool ModRimFridgeLoaded => ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "[KV] RimFridge" || m.PackageId == PidRimFridge);

        static DSGUIMain() {
            Log.Message("[DSGUI] Ready.");
        }

        // public const string PidRimFridge        = "rimfridge.kv.rw";

        public static bool ModSimpleLoaded    => ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "[JDS] Simple Storage" || m.PackageId == PidSimpleStorage);
        public static bool ModSimpleRefLoaded => ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "[JDS] Simple Storage - Refrigeration" || m.PackageId == PidSimpleRefStorage);
    }

    [UsedImplicitly]
    public partial class DSGUI {
        private static readonly MethodInfo CAF         = AccessTools.Method(typeof(FloatMenuMakerMap), "ChoicesAtFor");
        private static readonly FieldInfo  ThingListTG = AccessTools.Field(typeof(ThingGrid), "thingGrid");
        
        public static bool Create(Vector3 clickPosition, Pawn pawn, bool ordersOnly = false) {
            List<Thing> thingList, tileThingList;
            var c = IntVec3.FromVector3(clickPosition);
            if (!pawn.IsColonistPlayerControlled || pawn.Downed || pawn.Map != Find.CurrentMap) {
                Log.Message("[DSGUI] Pawn is not player controlled, downed, or on the current map. Handing execution to vanilla again.");
                return true;
            }
            
            var buildingList = StaticHelper.GetBuildings(c, pawn.Map).ToList();
            if (buildingList.OptimizedNullOrEmpty()) {
                Log.Message("[DSGUI] Building List is empty. Handing execution to vanilla again.");
                return true;
            }

            var storageUnit = buildingList.Find(building => building.AllComps.Find(x => x is IHoldMultipleThings.IHoldMultipleThings) != null);
            if (storageUnit == null || storageUnit.DestroyedOrNull()) {
                Log.Message("[DSGUI] Found no valid target. Handing execution to vanilla again.");
                return true;
            }

            if (DSGUIMain.ModSimpleLoaded && storageUnit.def.modContentPack.PackageId == DSGUIMain.PidSimpleStorage ||
                DSGUIMain.ModSimpleRefLoaded && storageUnit.def.modContentPack.PackageId == DSGUIMain.PidSimpleRefStorage) {
                var storageComp = (CompDeepStorage) storageUnit.AllComps.Find(x => x is CompDeepStorage);
                thingList = new List<Thing>(storageComp.getContentsHeader(out _, out _));
            }
            else {
                thingList = new List<Thing>(c.GetThingList(pawn.Map));
            }

            if (thingList.OptimizedNullOrEmpty()) {
                Log.Message("[DSGUI] Thing List is empty. Handing execution to vanilla again.");
                return true;
            }

            if (ordersOnly) {
                thingList     = new List<Thing>(c.GetThingList(pawn.Map));
                tileThingList = thingList.Where(t => t.def.category != ThingCategory.Item).ToList();
                // TODO: Move the entire ThingList trickery into its own function
                var index     = pawn.Map.cellIndices.CellToIndex(c);
                var listArray = (List<Thing>[]) ThingListTG.GetValue(pawn.Map.thingGrid);
                var origList  = new List<Thing>(listArray[index]);
                listArray[index] = new List<Thing>(tileThingList);
                var orders = (List<FloatMenuOption>) CAF.Invoke(null, new object[] {clickPosition, pawn, false});
                listArray[index] = origList;
                if (orders.Count <= 0) return true;

                Elements.TryMakeFloatMenu(orders, "DSGUI_List_Tile".TranslateSimple());
                return false;
            }

            tileThingList = thingList.Where(t => t.def.category != ThingCategory.Item).ToList();
            thingList.RemoveAll(t => t.def.category != ThingCategory.Item || t is Mote);
            Find.WindowStack.Add(new DSGUI_ListModal(pawn, thingList, clickPosition, storageUnit, tileThingList));
            return false;
        }
    }
}
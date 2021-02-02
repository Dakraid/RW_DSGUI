using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LWM.DeepStorage;
using UnityEngine;
using Verse;

// TODO: Add comments and explanations to the code

namespace DSGUI
{
    public static class GlobalStorage
    {
        public static Vector2 savedSize = new Vector2(0, 0);
        public static Vector2 savedPos = new Vector2(0, 0);
    }

    [StaticConstructorOnStartup]
    public static class DSGUIMain
    {
        public const string pidSimpleStorage = "jangodsoul.simplestorage";
        public const string pidSimpleRefStorage = "jangodsoul.simplestorage.ref";
        public const string pidRimFridge = "rimfridge.kv.rw";
        public static bool modSimpleLoaded => ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "[JDS] Simple Storage" || m.PackageId == pidSimpleStorage);
        public static bool modSimpleRefLoaded => ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "[JDS] Simple Storage - Refrigeration" || m.PackageId == pidSimpleRefStorage);
        public static bool modRimFridgeLoaded => ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "[KV] RimFridge" || m.PackageId == pidRimFridge);
        
        static DSGUIMain()
        {
            Log.Message("[DSGUI] Ready.");
        }
    }

    public partial class DSGUI
    {
        public static bool Create(Vector3 clickPosition, Pawn pawn)
        {
            var c = IntVec3.FromVector3(clickPosition);

            if (!pawn.IsColonistPlayerControlled || pawn.Downed || pawn.Map != Find.CurrentMap)
            {
                Log.Message("[DSGUI] Pawn is not player controlled, downed, or on the current map.");
                return true;
            }

            var buildingList = StaticHelper.GetBuildings(c, pawn.Map).ToList();

            if (buildingList.OptimizedNullOrEmpty())
            {
                Log.Message("[DSGUI] Building List is empty.");
                return true;
            }

            var storageUnit = buildingList.Find(building => building.AllComps.Find(x => x is IHoldMultipleThings.IHoldMultipleThings) != null);

            if (storageUnit == null || storageUnit.DestroyedOrNull())
            {
                Log.Message("[DSGUI] Found no valid target.");
                return true;
            }
            
            List<Thing> thingList;
            
            if (DSGUIMain.modSimpleLoaded && storageUnit.def.modContentPack.PackageId == DSGUIMain.pidSimpleStorage ||
                DSGUIMain.modSimpleRefLoaded && storageUnit.def.modContentPack.PackageId == DSGUIMain.pidSimpleRefStorage)
            {
                var storageComp = (CompDeepStorage) storageUnit.AllComps.Find(x => x is CompDeepStorage);
                thingList = new List<Thing>(storageComp.getContentsHeader(out _, out _));
            } 
            else 
            {
                thingList = new List<Thing>(c.GetThingList(pawn.Map));
            }
            
            if (thingList.OptimizedNullOrEmpty())
            {
                Log.Message("[DSGUI] Thing List is empty.");
                return true;
            }
            
            var tileThingList = thingList.Where(t => t.def.category != ThingCategory.Item);
            
            thingList.RemoveAll(t => t.def.category != ThingCategory.Item || t is Mote);

            Find.WindowStack.Add(new DSGUI_ListModal(pawn, thingList, clickPosition, storageUnit, tileThingList));
            return false;
        }
    }
}
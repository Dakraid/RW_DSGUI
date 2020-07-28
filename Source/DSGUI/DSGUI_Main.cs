using System;
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
        public static bool modSimplyLoaded => ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "[JDS] Simply Storage" || m.PackageId == "jangodsoul.simplystorage");
        
        static DSGUIMain()
        {
            // Placeholder
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
            // var building = c.GetFirstBuilding(pawn.Map);

            if (buildingList.EnumerableNullOrEmpty())
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
            
            // var thingList = new List<Thing>(c.GetThingList(pawn.Map));
            List<Thing> thingList;

            if (DSGUIMain.modSimplyLoaded && storageUnit.def.modContentPack.PackageId == "jangodsoul.simplystorage")
            {
                var storageComp = (CompDeepStorage) storageUnit.AllComps.Find(x => x is CompDeepStorage);
                thingList = new List<Thing>(storageComp.getContentsHeader(out _, out _));
            }
            else
            {
                thingList = new List<Thing>(c.GetThingList(pawn.Map));
            }
            
            thingList.RemoveAll(t => t.def.category != ThingCategory.Item || t is Mote);

            if (thingList.EnumerableNullOrEmpty())
            {
                Log.Message("[DSGUI] Thing List is empty.");
                return true;
            }

            Find.WindowStack.Add(new DSGUI_ListModal(pawn, thingList, clickPosition));
            return false;
        }
    }
}
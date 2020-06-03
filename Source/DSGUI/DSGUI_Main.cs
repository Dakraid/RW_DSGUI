using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AwesomeInventory.Common.HarmonyPatches;
using HarmonyLib;
using LWM.DeepStorage;
using RimWorld;
using UnityEngine;
using Verse;

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
        static DSGUIMain()
        {
            Settings.useDeepStorageRightClickLogic = false;
        }
    }

    public partial class DSGUI
    {
        public static bool Create(Vector3 clickPosition, Pawn pawn)
        {
            var c = IntVec3.FromVector3(clickPosition);

            if (!pawn.IsColonistPlayerControlled || pawn.Downed || pawn.Map != Find.CurrentMap)
                return true;

            var buildingList = StaticHelper.GetBuildings(c, pawn.Map).ToList();
            // var building = c.GetFirstBuilding(pawn.Map);
            
            if (buildingList.EnumerableNullOrEmpty())
                return true;

            var target = buildingList.Any(building => building.AllComps.Find(x => x is IHoldMultipleThings.IHoldMultipleThings) != null);

            if (!target)
                return true;

            var thingList = new List<Thing>(c.GetThingList(pawn.Map));

            if (thingList.EnumerableNullOrEmpty())
                return true;

            Find.WindowStack.Add(new DSGUI_ListModal(pawn, thingList, clickPosition));
            return false;
        }
    }
}
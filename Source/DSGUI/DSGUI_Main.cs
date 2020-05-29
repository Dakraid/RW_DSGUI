using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DSGUI
{
    public static class GlobalStorage
    {
        public static Thing currThing = null;
        public static Thing lastThing = null;
        public static bool pickUploaded;
    }
    
    [StaticConstructorOnStartup]
    public static class DSGUIMain
    {
        static DSGUIMain() 
        {
            LWM.DeepStorage.Settings.useDeepStorageRightClickLogic = false;
            GlobalStorage.pickUploaded = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Pick Up And Haul" || m.PackageId == "mehni.pickupandhaul");
            
            if (GlobalStorage.pickUploaded)
                Log.Warning("[DSGUI] WARNING: Pick Up and Haul is currently not compatible with DSGUI!");

            /*
            if (!GlobalStorage.pickUploaded) return;
            
            try
            {
                var pickUpTrans = AccessTools.Method(typeof(PickUpAndHaul.HarmonyPatches), "FloatMenuMakerMad_AddHumanlikeOrders_Transpiler");
                var ahloInfos = Harmony.GetPatchInfo(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"));
                var target = ahloInfos.Transpilers.First(x => x.PatchMethod == pickUpTrans)?.PatchMethod;

                HarmonyPatches.harmony.Unpatch(target, pickUpTrans);
                Log.Message("[DSGUI] Unpatched Pick Up and Haul Transpiler");
            }
            catch (Exception e)
            {
                Log.Warning("[DSGUI] Could not unpatch Pick Up and Haul Transpiler. Exception:\n" + e);
            }
            */
        }
    }
    
    public partial class DSGUI
    {
        public static bool Create(Vector3 clickPosition, Pawn pawn)
        {
            var c = IntVec3.FromVector3(clickPosition);

            if (!pawn.IsColonistPlayerControlled)
                return false;

            if (pawn.Downed)
            {
                Messages.Message("IsIncapped".Translate((NamedArgument)pawn.LabelCap, (NamedArgument)pawn), pawn, MessageTypeDefOf.RejectInput, false);
            }
            else
            {
                if (pawn.Map != Find.CurrentMap)
                    return false;
            }

            var buildingList = StaticHelper.GetBuildings(c, pawn.Map);
            if (buildingList.NullOrEmpty())
                return true;

            ThingComp target = null;
            foreach (var building in buildingList) target = building.AllComps.Find(x => x is IHoldMultipleThings.IHoldMultipleThings);

            if (target == null)
                return true;

            var thingList = new List<Thing>(c.GetThingList(pawn.Map));

            if (thingList.NullOrEmpty())
            {
                var cells = target.parent.GetSlotGroup().CellsList;

                foreach (var cell in cells) thingList.AddRange(cell.GetThingList(pawn.Map));
            }

            Find.WindowStack.Add(new DSGUI_ListModal(pawn, thingList, clickPosition));
            return false;
        }
    }
}
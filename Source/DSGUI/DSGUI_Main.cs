using System.Collections.Generic;
using LWM.DeepStorage;
using RimWorld;
using UnityEngine;
using Verse;

namespace DSGUI
{
    public static class GlobalStorage
    {
        public static Thing currThing = null;

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

            if (!pawn.IsColonistPlayerControlled)
                return false;

            if (pawn.Downed)
            {
                Messages.Message("IsIncapped".Translate((NamedArgument) pawn.LabelCap, (NamedArgument) pawn), pawn, MessageTypeDefOf.RejectInput, false);
            }
            else
            {
                if (pawn.Map != Find.CurrentMap)
                    return false;
            }

            // var buildingList = StaticHelper.GetBuildings(c, pawn.Map);
            var building = c.GetFirstBuilding(pawn.Map);

            var target = building?.AllComps.Find(x => x is IHoldMultipleThings.IHoldMultipleThings);

            if (target == null)
                return true;

            var thingList = new List<Thing>(c.GetThingList(pawn.Map));

            if (thingList.Count == 0)
            {
                var cells = building.GetSlotGroup().CellsList;

                foreach (var cell in cells) thingList.AddRange(cell.GetThingList(pawn.Map));
            }

            Find.WindowStack.Add(new DSGUI_ListModal(pawn, thingList, clickPosition));
            return false;
        }
    }
}
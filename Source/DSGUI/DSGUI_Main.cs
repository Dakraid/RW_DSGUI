using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace DSGUI
{
    public static class GlobalStorage
    {
        public static Thing currThing = null;
        public static Thing lastThing = null;
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
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DSGUI
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("net.netrve.dsgui");
            
            // We patch all as we use annotations
            harmony.PatchAll();
        }
        
        [HarmonyPatch(typeof(FloatMenuMakerMap), "TryMakeFloatMenu")]
        internal static class Patch_TryMakeFloatMenu
        {
            [HarmonyPriority(Priority.First)]
            public static bool Prefix(Pawn pawn)
            {
                return DSGUI.Create(UI.MouseMapPosition(), pawn);
            }
        }

        [HarmonyPatch(typeof(GridsUtility), "GetThingList")]
        internal static class Patch_GetThingList
        {
            [HarmonyPriority(Priority.First)]
            public static bool Prefix(ref List<Thing> __result)
            {
                if (GlobalStorage.currThing == null)
                    return true;

                GlobalStorage.lastThing = GlobalStorage.currThing;
                __result = new List<Thing> {GlobalStorage.currThing};
            
                return false;
            }
        }

        [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
        internal static class Patch_AddHumanlikeOrders
        {
            [HarmonyPriority(Priority.First)]
            public static bool Prefix()
            {
                if (GlobalStorage.currThing == null)
                    return true;

                return GlobalStorage.currThing != GlobalStorage.lastThing;
            }
        }
    }
}
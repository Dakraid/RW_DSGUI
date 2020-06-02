using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DSGUI
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("net.netrve.dsgui");

            // We patch all as we use annotations
            harmony.PatchAll();
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
        
        // Thanks to Garthor (who came up with this specific solution), Krafs, and Mehni for helping with this
        [HarmonyPatch]
        private static class Patch_ThingAt
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(typeof(ThingGrid), "ThingAt", new[] {typeof(IntVec3)}, new[] {typeof(Apparel)});
            }

            [HarmonyPriority(Priority.First)]
            public static bool Prefix(ref Thing __result, IntVec3 c)
            {
                if (GlobalStorage.currThing == null || !(GlobalStorage.currThing is Apparel))
                    return true;
            
                __result = null;
                
                if (GlobalStorage.currThing is Apparel)
                    __result = GlobalStorage.currThing;

                return false;
            }
        }

        [HarmonyPatch(typeof(GridsUtility), "GetThingList")]
        private static class Patch_GetThingList
        {
            [HarmonyPriority(Priority.First)]
            public static bool Prefix(ref List<Thing> __result)
            {
                if (GlobalStorage.currThing == null)
                    return true;

                __result = new List<Thing> {GlobalStorage.currThing};

                return false;
            }
        }

        [HarmonyPatch(typeof(GridsUtility), "GetFirstItem")]
        private static class Patch_GetFirstItem
        {
            [HarmonyPriority(Priority.First)]
            public static bool Prefix(ref Thing __result)
            {
                if (GlobalStorage.currThing == null)
                    return true;

                __result = GlobalStorage.currThing;

                return false;
            }
        }

        /*
        public static Thing GetThingAt<T>(IntVec3 c, Map map) where T : Thing
        {
            if (GlobalStorage.currThing == null)
                return map.thingGrid.ThingAt<T>(c);
            
            if (GlobalStorage.currThing is T)
                return GlobalStorage.currThing as T;
            
            return null;
        }
        
        [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
        private static class Patch_AddHumanlikeOrders
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return instructions.MethodReplacer();
            }
        }
        */
    }
}
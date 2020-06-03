using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using PickUpAndHaul;
using RimWorld;
using UnityEngine;
using Verse;

namespace DSGUI
{
    public static class HarmonyHelper
    {
        public static T ThingAt<T>(ThingGrid thingGrid, IntVec3 c) where T : Thing
        {
            if (GlobalStorage.currThing != null && GlobalStorage.currThing is T)
                return GlobalStorage.currThing as T;
            
            return thingGrid.ThingAt<T>(c);
        }
    }
    
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

        [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
        private static class Patch_AddHumanlikeOrders
        {
            private static readonly MethodInfo thingAt = AccessTools.Method(typeof(ThingGrid), "ThingAt", new[] {typeof(IntVec3)}).MakeGenericMethod(typeof(Apparel));
            private static readonly MethodInfo dsguiThingAt = AccessTools.Method(typeof(HarmonyHelper), "ThingAt", new[] {typeof(ThingGrid), typeof(IntVec3)}).MakeGenericMethod(typeof(Apparel));
            
            [HarmonyDebug]
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return instructions.MethodReplacer(thingAt, dsguiThingAt);
            }
        }
    }
}
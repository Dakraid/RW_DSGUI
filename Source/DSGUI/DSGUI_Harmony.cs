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
    public class HarmonyHelper
    {
        public T ThingAt<T>(Map map, IntVec3 c) where T : Thing
        {
            if (GlobalStorage.currThing != null && GlobalStorage.currThing is T)
                return GlobalStorage.currThing as T;
            
            return map.thingGrid.ThingAt<T>(c);
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
        
        /*
        // Thanks to Garthor, Krafs, and Mehni for helping with this
        [HarmonyPatch]
        private static class Patch_ThingAt
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(typeof(ThingGrid), "ThingAt", new[] {typeof(IntVec3)}).MakeGenericMethod(typeof(Apparel));
            }

            [HarmonyPriority(Priority.First)]
            public static bool Prefix(ref Apparel __result, IntVec3 c)
            {
                if (GlobalStorage.currThing == null || !(GlobalStorage.currThing is Apparel apparel)) return true;
                
                __result = apparel; 
                
                return false;
            }
        }
        */

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
            private static readonly FieldInfo thingGrid = AccessTools.Field(typeof(Map), "thingGrid");
            private static readonly MethodInfo thingAt = AccessTools.Method(typeof(ThingGrid), "ThingAt", new[] {typeof(IntVec3)}).MakeGenericMethod(typeof(Apparel));
            private static readonly MethodInfo dsguiThingAt = AccessTools.Method(typeof(HarmonyHelper), "ThingAt").MakeGenericMethod(typeof(Apparel));
            
            [HarmonyDebug]
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                foreach (var instruction in instructionList)
                {
                    if (instruction.LoadsField(thingGrid))
                    {
                        instruction.opcode = OpCodes.Nop;
                        instruction.operand = null;
                        yield return instruction;
                        
                        continue;
                    }
                    
                    if (instruction.Calls(thingAt))
                    {
                        instruction.opcode = OpCodes.Call;
                        instruction.operand = dsguiThingAt;
                        yield return instruction;
                        
                        continue;
                    }

                    yield return instruction;
                }
            }
        }
    }
}
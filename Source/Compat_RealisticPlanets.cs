using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PollutionTweaks;
public static class Compat_RealisticPlanets {
    public static readonly bool Active =
        LoadedModManager.RunningMods.Any(x => x.PackageId == "windowsxp.realisticplanets");
}

[HarmonyPatch]
public static class Compat_RealisticPlanets_DoWindowContents {
    [HarmonyPrepare]
    public static bool ShouldPatch() 
        => Compat_RealisticPlanets.Active;

    [HarmonyTargetMethod]
    public static MethodBase Method() 
        => AccessTools.Method("Planets_Code.Planets_CreateWorldParams:DoWindowContents");

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> orig) {
        var pollution = AccessTools.Method("Planets_Code.Planets_CreateWorldParams:DoPollutionSlider");
        foreach (var instr in orig) {
            yield return instr;
            if (instr.Calls(pollution)) {
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Ldloca_S, 1);
                yield return new CodeInstruction(OpCodes.Ldc_R4, 400f);
                yield return CodeInstruction.Call(typeof(Patch_Page_CreateWorldParams),
                                                  nameof(Patch_Page_CreateWorldParams.WindowContentAdditions));
            }
        }
    }
}

[HarmonyPatch]
public static class Compat_RealisticPlanets_CanDoNext {
    [HarmonyPrepare]
    public static bool ShouldPatch() 
        => Compat_RealisticPlanets.Active;

    [HarmonyTargetMethod]
    public static MethodBase Method() 
        => AccessTools.Method("Planets_Code.Planets_CreateWorldParams:CanDoNext");

    [HarmonyPostfix]
    public static void CanDoNext() 
        => Patch_Page_CreateWorldParams.CanDoNext();
}

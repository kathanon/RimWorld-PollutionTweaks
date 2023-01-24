using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PollutionTweaks;
[HarmonyPatch]
public static class Patch_WorldGenStep_Pollution {
    public static float min    = 0.25f;
    public static float max    = 1f;
    public static bool  adjust = false;

    private static float minNoise;
    private static float noiseScale;

    public static MethodInfo round        = AccessTools.Method(typeof(Mathf), "RoundToInt");
    public static MethodInfo lerp         = AccessTools.Method(typeof(Mathf), "Lerp");
    public static MethodInfo replacement  = AccessTools.Method(typeof(Patch_WorldGenStep_Pollution), "Pollution");
    public static MethodInfo readNoise    = AccessTools.Method(typeof(Patch_WorldGenStep_Pollution), "ReadNoise");
    public static FieldInfo  tmpTiles     = AccessTools.Field (typeof(WorldGenStep_Pollution), "tmpTiles");
    public static FieldInfo  tmpTileNoise = AccessTools.Field (typeof(WorldGenStep_Pollution), "tmpTileNoise");

    [HarmonyPatch(typeof(WorldGenStep_Pollution), nameof(WorldGenStep_Pollution.GenerateFresh))]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instrs) {
        foreach (var instr in instrs) {
            if (instr.Calls(lerp)) {
                instr.operand = replacement;
                yield return instr;
            } else if (instr.Calls(round)) {
                yield return instr;
                yield return new CodeInstruction(OpCodes.Dup);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, tmpTiles);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, tmpTileNoise);
                yield return new CodeInstruction(OpCodes.Call, readNoise);
            } else {
                yield return instr;
            }
        }
    }

    public static void ReadNoise(int n, List<int> tmpTiles, Dictionary<int, float> tmpTileNoise) {
        minNoise   = tmpTileNoise[tmpTiles[n - 1]];
        noiseScale = tmpTileNoise[tmpTiles[0]] - minNoise;
    }

    public static float Pollution(float vanillaMin, float vanillaMax, float noise) {
        if (adjust) {
            noise = (noise - minNoise) / noiseScale;
        }
        float vanilla = Mathf.Lerp(vanillaMin, vanillaMax, noise);
        float vanillaScale = vanillaMax - vanillaMin;
        float unscaled = (vanilla - vanillaMin) / vanillaScale;
        float scale = max - min;
        return unscaled * scale + min;
    }
}

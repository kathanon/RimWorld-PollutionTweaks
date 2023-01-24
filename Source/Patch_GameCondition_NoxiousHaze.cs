using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PollutionTweaks;

[HarmonyPatch]
public static class Patch_GameCondition_NoxiousHaze {
    private static int lastSpread = 0;
    private static int nextSpread = 0;

    public static int Interval => Settings.RandomSmogInterval * 1000;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameCondition_NoxiousHaze), nameof(GameCondition_NoxiousHaze.GameConditionTick))]
    public static void GameConditionTick(GameCondition_NoxiousHaze __instance) {
        int ticks = __instance.TicksPassed;
        if (lastSpread > ticks) {
            lastSpread = 0;
            nextSpread = Interval;
        }
        if (nextSpread < ticks) {
            int amount = Settings.RandomSmogPollution;
            lastSpread = ticks;
            nextSpread += Interval;
            foreach (var map in __instance.AffectedMaps) {
                for (int i = 0; i < amount; i++) {
                    PollutionUtility.GrowPollutionAt(map.RandomPollutableEdgeCell(), map, 1, silent: true);
                }
            }
        }
    }

    public static IntVec3 RandomPollutableEdgeCell(this Map map) {
        var size = map.Size;
        int sum = size.x + size.z - 2;
        var val = IntVec3.Zero;
        for (int j = 0; j < 10; j++) {
            int i = Rand.Range(0, 2 * sum);
            if (i > sum) {
                val.x = size.x - 1;
                val.z = size.z - 1;
            }
            i %= sum;
            if (i < size.x) {
                val.x = i;
            } else {
                val.z = i - size.x + 1;
            }
            if (map.pollutionGrid.EverPollutable(val)) return val;
        }
        return map.EdgeCells().Where(map.pollutionGrid.EverPollutable).RandomElement();
    }

    public static IEnumerable<IntVec3> EdgeCells(this Map map) {
        var size = map.Size;
        int xmax = size.x - 1;
        int zmax = size.z - 1;
        var val = IntVec3.Zero;
        for (val.x = 0; val.x < xmax; val.x++) {
            val.z = 0;
            yield return val;
            val.z = zmax;
            yield return val;
        }
        for (val.z = 0; val.z < zmax; val.z++) {
            val.x = 0;
            yield return val;
            val.x = xmax;
            yield return val;
        }
    }
}

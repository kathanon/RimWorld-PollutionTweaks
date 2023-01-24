using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PollutionTweaks;
[HarmonyPatch]
public static class Patch_Page_CreateWorldParams {
    public const float VanillaMin =   0.25f;
    public const float VanillaMax =   1f;
    public const float Margin     =  18f;
    public const float YStep      =  40f;
    public const float YSkip      =  45f + 6 * YStep;
    public const float Height     =  30f;
    public const float LabelWidth = 200f;
    public const float CheckWidth = LabelWidth + Widgets.CheckboxSize;
    public const int   SliderId   = Settings.LastSliderId + 1;

    public static FloatRange range  = new(VanillaMin, VanillaMax);
    public static bool       adjust = false;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Page_CreateWorldParams), nameof(Page_CreateWorldParams.Reset))]
    public static void Reset() {
        range.min = VanillaMin;
        range.max = VanillaMax;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Page_CreateWorldParams), nameof(Page_CreateWorldParams.DoWindowContents))]
    public static void DoWindowContents(Rect rect) {
        if (!ModsConfig.BiotechActive) return;
        Rect area = new Rect(rect.x, rect.y + YSkip, (rect.width - Margin) * 0.5f, Height);
        TooltipHandler.TipRegion(area, Strings.PollutionTip);
        area.y += YStep;

        TooltipHandler.TipRegion(area, Strings.PollutionRangeTip);
        Widgets.Label(area.LeftPartPixels(LabelWidth), Strings.PollutionRange);
        var sliderRect = area;
        sliderRect.xMin += LabelWidth;
        Widgets.FloatRange(sliderRect, SliderId, ref range, valueStyle: ToStringStyle.PercentZero);
        area.y += YStep;

        TooltipHandler.TipRegion(area, Strings.PollutionSpreadTip);
        Widgets.Label(area.LeftPartPixels(LabelWidth), Strings.PollutionSpread);
        Widgets.Checkbox(new(area.x + LabelWidth, area.y), ref adjust);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Page_CreateWorldParams), "CanDoNext")]
    public static void CanDoNext() {
        Patch_WorldGenStep_Pollution.min    = range.min;
        Patch_WorldGenStep_Pollution.max    = range.max;
        Patch_WorldGenStep_Pollution.adjust = adjust;
    }
}

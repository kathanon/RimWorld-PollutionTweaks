using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PollutionTweaks;

[StaticConstructorOnStartup]
public class Settings : ModSettings {
    public const float Margin      =   6f;
    public const float LabelWidth  = 100f;
    public const float SliderWidth = 300f;
    public const float RowHeight   =  30f;
    public const float ButtonWidth = 100f;
    public const float GraphHeight = 400f;
    public const float GraphWidth  = 600f;
    public const float GraphMargin =  36f;
    public const int MaxRange     = 4;
    public const int PollutionId  = 56419167;
    public const int IntervalId   = PollutionId + 1;
    public const int LastSliderId = IntervalId;
    public static readonly Color GraphGridColor = new(.2f, .85f, 1f);
    public static readonly Color GraphLineColor = Color.white;

    private static readonly SimpleCurve curve = WorldPollutionUtility.NearbyPollutionOverDistanceCurve;
    private static readonly List<CurvePoint> defaultCurve = curve.Points.ToList();

    private int dragging = -1;
    private int selected = -1;

    private IntRange smogPollution = new();
    private IntRange smogInterval = new(6, 10);
    private int maxWeight = 2;
    private List<float> curveListX = new();
    private List<float> curveListY = new();

    private static Settings instance;

    public Settings() {
        instance = this;
    }

    public static int RandomSmogInterval => instance.smogInterval.RandomInRange;

    public static int RandomSmogPollution => instance.smogPollution.RandomInRange;

    public void DoWindowContents(Rect rect) {
        Text.Font = GameFont.Medium;
        float titleStep = Text.LineHeight + Margin;
        Widgets.Label(rect, Strings.SmogTitle);
        rect.yMin += titleStep;

        Text.Font = GameFont.Small;
        Widgets.Label(rect, Strings.SpreadPollution);
        rect.yMin += Text.CalcHeight(Strings.SpreadPollution, rect.width) + Margin;

        var row = new Rect(rect.x, rect.y, Mathf.Min(LabelWidth + SliderWidth, rect.width), RowHeight);
        var label = row.LeftPartPixels(LabelWidth);
        var slider = row.RightPartPixels(row.width - LabelWidth);

        TooltipHandler.TipRegion(row, Strings.AmountTip);
        Widgets.Label(label, Strings.Amount);
        Widgets.IntRange(slider, PollutionId, ref smogPollution, 0, 20);
        label.y = slider.y = row.y += RowHeight + Margin;

        TooltipHandler.TipRegion(row, Strings.IntervalTip);
        Widgets.Label(label, Strings.Interval);
        Widgets.IntRange(slider, IntervalId, ref smogInterval, 1, 24);
        rect.yMin = label.yMax + RowHeight + Margin;

        Text.Font = GameFont.Medium;
        Widgets.Label(rect, Strings.NearbyPollutionTitle);
        rect.yMin += titleStep;

        Text.Font = GameFont.Small;
        Widgets.Label(rect, Strings.NearbyPollutionWeighing);
        rect.yMin += Text.CalcHeight(Strings.NearbyPollutionWeighing, rect.width) + 2 * Margin;

        label.y = slider.y = row.y = rect.y;
        Widgets.Label(label, Strings.MaxWeight);
        float floatValue = Widgets.HorizontalSlider(slider, maxWeight, 1, 10, true, maxWeight.ToStringCached(), roundTo: 1f);
        maxWeight = Mathf.RoundToInt(floatValue);
        rect.yMin = label.yMax + RowHeight + Margin;

        DrawGraph(rect);

        GenUI.ResetLabelAlign();
    }

    private void DrawGraph(Rect rect) {
        float width = Mathf.Min(GraphWidth, rect.width - 8f - ButtonWidth - Margin);
        float height = Mathf.Min(GraphHeight, rect.height - 8f);
        var graph = new Rect(rect.x, rect.y, width, height);

        var legend = new Rect(graph.x, graph.y, Text.CalcSize(Strings.WeightVertical).x, graph.height - GraphMargin);
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(legend, Strings.WeightVertical);
        legend = new Rect(graph.x + GraphMargin, graph.y + 2f, graph.width - GraphMargin, graph.height);
        Text.Anchor = TextAnchor.LowerCenter;
        Widgets.Label(legend, Strings.Distance);
        graph.xMin += GraphMargin;
        graph.yMax -= GraphMargin;

        float xScale = graph.width / MaxRange;
        float yScale = graph.height / maxWeight;
        Rect number = new Rect(0f, 0f, Text.LineHeight, Text.LineHeight);
        var upperRight = new Vector2(graph.xMax, graph.yMin);

        var start = graph.min;
        var end   = upperRight;
        start.x -= 4f;
        end.x   += 4f;
        number.xMax = start.x - 4f;
        Text.Anchor = TextAnchor.MiddleRight;
        for (int i = maxWeight; i >= 0; i--) {
            Widgets.DrawLine(start, end, GraphGridColor, (i == 0) ? 2f : 1f);
            number.y = start.y - number.height / 2;
            Widgets.Label(number, i.ToStringCached());
            if (i > 0) start.y = end.y += yScale;
        }

        start = graph.max;
        end = upperRight;
        start.y += 6f;
        end.y   -= 2f;
        number.y = start.y - 4f;
        Text.Anchor = TextAnchor.UpperCenter;
        for (int i = MaxRange; i >= 0; i--) {
            Widgets.DrawLine(start, end, GraphGridColor, (i == 0) ? 2f : 1f);
            number.x = start.x - number.width / 2;
            Widgets.Label(number, i.ToStringCached());
            if (i > 0) start.x = end.x -= xScale;
        }

        Vector2 prev = default;
        for (int i = 0; i < curve.PointsCount; i++) {
            var p = curve.Points[i];
            float x = graph.x    + p.x * xScale;
            float y = graph.yMax - p.y * yScale;
            var point = new Vector2(x, y);
            if (i > 0) {
                Widgets.DrawLine(prev, point, GraphLineColor, 1f);
            }
            prev = point;
            var handle = new Rect(x - 3f, y - 3f, 6f, 6f);
            Widgets.DrawBoxSolid(handle, GraphLineColor);
            if (selected == i) {
                GUI.color = GraphLineColor;
                Widgets.DrawBox(new Rect(x - 6f, y - 5f, 11f, 11f));
                GUI.color = Color.white;
            }
            if (Event.current.type == EventType.MouseDown && Mouse.IsOver(handle)) {
                dragging = selected = i;
                Event.current.Use();
            }
        }

        if (Event.current.type == EventType.MouseUp) {
            dragging = -1;
        } else if (Event.current.type == EventType.MouseDrag && dragging >= 0) {
            var pos = Event.current.mousePosition;
            var points = curve.Points;
            float xMin = (dragging == 0) ? 0f : points[dragging - 1].x;
            float xMax = (dragging == points.Count - 1) ? MaxRange : points[dragging + 1].x;
            float x = Mathf.Clamp((pos.x - graph.x) / xScale, xMin, xMax);
            float y = Mathf.Clamp((graph.yMax - pos.y) / yScale, 0f, maxWeight);
            x = Mathf.Round(x * 10f) / 10f;
            y = Mathf.Round(y * 10f) / 10f;
            points[dragging] = new(x, y);
            Event.current.Use();
        }

        var button = new Rect(graph.xMax + Margin, graph.y, ButtonWidth, RowHeight);
        if (Widgets.ButtonText(button, Strings.Reset)) {
            curve.SetPoints(defaultCurve);
            selected = -1;
        }
        button.y += RowHeight + Margin;
        if (Widgets.ButtonText(button, Strings.AddPoint)) {
            var points = curve.Points;
            int n = points.Count;
            float max = MaxRange;
            curve.Add(new(max, 0f), false);
            if (points[n - 1].y < 0.1f) max -= 0.1f;
            for (int i = n - 1; i >= 0 && points[i].x > max; i--) {
                points[i] = new(max, points[i].y);
                if (Mathf.Abs(points[i - 1].y - points[i].y) < 0.1f) max -= 0.1f;
            }
        }
        button.y += RowHeight + Margin;
        if (selected < 0) {
            GUI.color = Color.grey;
        }
        if (Widgets.ButtonText(button, Strings.RemovePoint, active: selected >= 0)) {
            curve.Points.RemoveAt(selected);
            selected = -1;
        }
        GUI.color = Color.white;
    }

    public override void ExposeData() {
        if (Scribe.mode == LoadSaveMode.Saving) {
            curveListX.Clear();
            curveListY.Clear();
            curveListX.AddRange(curve.Select(p => p.x));
            curveListY.AddRange(curve.Select(p => p.y));
        }

        Scribe_Collections.Look(ref curveListX, "curveX", LookMode.Value);
        Scribe_Collections.Look(ref curveListY, "curveY", LookMode.Value);
        if (curveListX == null) curveListX = new();
        if (curveListY == null) curveListY = new();

        if (Scribe.mode == LoadSaveMode.PostLoadInit 
                && curveListX.Count > 0 
                && curveListX.Count == curveListY.Count) {
            curve.SetPoints(curveListX.Zip(curveListY, (x, y) => new CurvePoint(x, y)));
        }

        Scribe_Values.Look(ref smogPollution, "smogPollution", new());
        Scribe_Values.Look(ref smogInterval, "smogInterval", new(6, 10));
    }
}

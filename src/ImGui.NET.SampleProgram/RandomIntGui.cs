using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGui.NET.SampleProgram;

namespace ImGuiNET;

public class RandomIntGui : Program
{
    static         bool   withSeed        = false;
    static         int    seed            = 0;
    private static string randomMaxInt    = "";
    private static string randomMinMaxInt = "";
    private static string randomInt       = "";
    private static string randomIntExt    = "";


    private static int     minIntExt            = -100;
    private static int     maxIntExt            = 100;
    private static int     minMaxIntValuesCount = 100;
    private static int     minMaxHighestCount   = 0;
    private static int     minMaxLowestCount    = 0;
    private static float[] ValuesMinMax         = new float[1];


    private static int                    maxInt            = 100;
    private static int                    maxIntValuesCount = 100;
    private static int                    maxHighestCount   = 0;
    private static float[]                ValuesMax         = new float[1];
    private static Dictionary<int, float> dict              = new Dictionary<int, float>();


    public static void RandomInt()
    {
        ImGui.TextColored(new Vector4(1, 1, 0, 1), "Random Integer Functions");

        if (ImGui.Checkbox("With Seed", ref withSeed))
        {
        }

        ImGui.SameLine();
        var val = RandomFunctions.GetSeed();
        if (withSeed)
        {
            if (ImGui.Button("R"))
            {
                seed = RandomFunctions.NewSeed();
            }

            ImGui.SameLine();
            ImGui.InputInt("##Seed", ref seed, 0, 0);
        }
        else
        {
            ImGui.InputInt("##Seed", ref val, 0, 0, ImGuiInputTextFlags.ReadOnly);
        }

        if (ImGui.BeginTable("Table01", 5))
        {
            ImGui.TableSetupColumn("Column 1", ImGuiTableColumnFlags.WidthFixed, 200);
            ImGui.TableSetupColumn("Column 2", ImGuiTableColumnFlags.WidthFixed, 200);
            ImGui.TableSetupColumn("Column 3", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableSetupColumn("Column 4", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("Column 5");
            ImGui.TableNextRow();

            RandomIntRow("Random Integer Max",    ref randomMaxInt,    "Int32.(0-MaxValue)");
            RandomIntRow("Random Integer MinMax", ref randomMinMaxInt, "Int32.(MinValue-MaxValue)");
            RandomMaxIntRow("Random Integer (0-Max)", ref randomInt, ref maxInt);
            RandomMinMaxIntRow("Random Integer (Min-Max)", ref randomIntExt, ref minIntExt, ref maxIntExt);

            ImGui.EndTable();
        }

        PlotMaxValues();

        if (dict?.Count < 0) return;
        PlotMinMaxValues();
    }

    static void RandomIntRow(string text, ref string value, string tooltip = "")
    {
        ImGui.TableNextColumn();
        ImGui.Text(text);

        ImGui.TableNextColumn();
        ImGui.Text(tooltip);

        ImGui.TableNextColumn();
        ImGui.Text(value);

        ImGui.TableNextColumn();
        if (ImGui.Button($"Get##{text}"))
        {
            RandomFunctions.SetSeed(seed);
            value = RandomFunctions.NextInt().ToString();
        }

        ImGui.TableNextRow();
    }

    static void RandomMaxIntRow(string text, ref string value, ref int max)
    {
        ImGui.TableNextColumn();
        ImGui.Text(text);

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(198);
        ImGui.InputInt($"##max{text}", ref max);

        ImGui.TableNextColumn();
        ImGui.Text(value);

        ImGui.TableNextColumn();
        if (ImGui.Button($"Get##{text}0"))
        {
            if (withSeed)
            {
                RandomFunctions.SetSeed(seed);
            }

            value = RandomFunctions.NextInt(0, max).ToString();
        }

        ImGui.TableNextColumn();
        ImGui.InputInt($"##max{text}1", ref maxIntValuesCount, 0, 0);
        ImGui.SameLine();
        if (ImGui.Button($"Get {maxIntValuesCount}##{text}1"))
        {
            if (withSeed)
            {
                RandomFunctions.SetSeed(seed);
            }

            Dictionary<int, int> dict1 = new Dictionary<int, int>();

            ValuesMax = new float[max];

            for (int i = 0; i < max; i++)
            {
                dict1.Add(i, 0);
            }

            for (int i = 0; i < maxIntValuesCount; i++)
            {
                var val = RandomFunctions.NextInt(0, max);
                dict1[val]++;
            }

            for (int i = 0; i < ValuesMax.Length; i++)
            {
                ValuesMax[i] = dict1[i];
            }

            maxHighestCount = dict1.Values.Max();
        }


        ImGui.TableNextRow();
    }

    static void RandomMinMaxIntRow(string text, ref string value, ref int min, ref int max)
    {
        ImGui.TableNextColumn();
        ImGui.Text(text);

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(95);
        ImGui.InputInt($"##min{text}", ref min);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(95);
        ImGui.InputInt($"##max{text}", ref max);

        ImGui.TableNextColumn();
        ImGui.Text(value);

        ImGui.TableNextColumn();
        if (ImGui.Button($"Get##{text}"))
        {
            if (withSeed)
            {
                RandomFunctions.SetSeed(seed);
            }

            value = RandomFunctions.NextInt(min, max).ToString();
        }

        ImGui.TableNextColumn();
        ImGui.InputInt($"##max{text}2", ref minMaxIntValuesCount, 0, 0);
        ImGui.SameLine();
        if (ImGui.Button($"Get {minMaxIntValuesCount}##{text}2"))
        {
            if (withSeed)
            {
                RandomFunctions.SetSeed(seed);
            }

            dict.Clear();

            ValuesMinMax = new float[max - min];

            for (int i = min; i < max; i++)
            {
                dict.Add(i, 0);
            }

            for (int i = 0; i < minMaxIntValuesCount; i++)
            {
                var val = RandomFunctions.NextInt(min, max);
                dict[val] += 1;
            }

            var _min = min;

            for (int i = 0; i < ValuesMinMax.Length; i++)
            {
                ValuesMinMax[i] = dict[_min++];
            }

            minMaxHighestCount = (int)dict.Values.Max();
            minMaxLowestCount  = (int)dict.Values.Min();
        }


        ImGui.TableNextRow();
    }


    static void PlotMaxValues()
    {
        ImGui.Text($"Highest Count: {maxHighestCount} ({ValuesMax.Count(x => x.Equals(maxHighestCount))})");
        ImGui.Text($"Values with zero: {ValuesMax.Count(x => x == 0)}");
        ImGui.PlotHistogram("##PlotMaxValues", ref ValuesMax[0], ValuesMax.Length, 0, "", 0.0f, maxHighestCount,
                            new Vector2(_window.Width - 16, 80));
    }

    static void PlotMinMaxValues()
    {
        ImGui.Text($"Highest Count: {minMaxHighestCount} ({ValuesMinMax.Count(x => x.Equals(minMaxHighestCount))})");
        ImGui.Text($"Values with zero: {ValuesMinMax.Count(x => x == 0)}");
        ImGui.PlotHistogram("##PlotMinMaxValues", ref ValuesMinMax[0], dict.Count, 0, "", minMaxLowestCount, minMaxHighestCount,
                            new Vector2(_window.Width - 16, 80));

        // ImGui.PlotLines("##PlotMinMaxValues1", ref ValuesMinMax[0], dict.Count, 0, "", minMaxLowestCount, minMaxHighestCount,
        // new Vector2(_window.Width - 16, 80));
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGui.NET.SampleProgram;

namespace ImGuiNET;

public class RandomFloatGui : Program
{
    static         bool   withSeed          = false;
    static         int    seed              = 0;
    private static string randomMaxFloat    = "";
    private static string randomMinMaxFloat = "";
    private static string randomFloat       = "";
    private static string randomFloatExt    = "";

    private static float   minFloatExt            = -100.0f;
    private static float   maxFloatExt            = 100.0f;
    private static int     minMaxFloatValuesCount = 100;
    private static int     minMaxHighestCount     = 0;
    private static int     minMaxLowestCount      = 0;
    private static float[] ValuesMinMax           = new float[1];


    private static float                  maxFloat            = 100.0f;
    private static int                    maxFloatValuesCount = 100;
    private static int                    maxHighestCount     = 0;
    private static float[]                ValuesMax           = new float[1];
    private static Dictionary<int, float> dict                = new Dictionary<int, float>();
}

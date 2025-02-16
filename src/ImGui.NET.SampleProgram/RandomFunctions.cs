using System;
using System.Collections.Generic;
using System.Linq;

namespace ImGui.NET.SampleProgram;

public static class RandomFunctions
{
    // Speichert den aktuell verwendeten Seed
    private static int _seed;
    // Statische Instanz von Random (Achtung: Random ist nicht thread-sicher)
    private static Random _random;

    // Statischer Konstruktor: Initialisiert mit einem Standardseed (hier Environment.TickCount)
    static RandomFunctions()
    {
        _seed   = Environment.TickCount;
        _random = new Random(_seed);
    }

    public static int NewSeed()
    {
        _seed = DateTime.Now.Ticks.GetHashCode();
        _random = new Random(_seed);
        return _seed;
    }

    // Setzt den Seed für den Zufallsgenerator und speichert diesen ab
    public static void SetSeed(int seed)
    {
        _seed   = seed;
        _random = new Random(seed);
    }

    // Gibt den aktuell verwendeten Seed zurück
    public static int GetSeed() => _seed;

    // Gibt einen zufälligen Integer ohne Grenzen zurück
    public static int NextInt() => _random.Next();

    // Gibt einen zufälligen Integer zwischen 0 (inklusive) und maxValue (exklusive) zurück
    public static int NextInt(int maxValue) => _random.Next(maxValue);

    // Gibt einen zufälligen Integer zwischen minValue (inklusive) und maxValue (exklusive) zurück
    public static int NextInt(int minValue, int maxValue) => _random.Next(minValue, maxValue);

    // Gibt einen zufälligen Double zwischen 0.0 (inklusive) und 1.0 (exklusive) zurück
    public static double NextDouble() => _random.NextDouble();

    // Erzeugt einen normalverteilten Zufallswert.
    // Standardmäßig: Mittelwert 0 und Standardabweichung 1 (N(0,1))
    public static double NextGaussian(double mean = 0.0, double standardDeviation = 1.0)
    {
        // Box-Muller-Transformation
        double u1            = 1.0 - _random.NextDouble(); // Vermeidet 0
        double u2            = 1.0 - _random.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return mean + standardDeviation * randStdNormal;
    }

    // Gibt einen zufälligen Wert basierend auf einer gewichteten Verteilung zurück.
    // Das Dictionary enthält die möglichen Werte als Schlüssel und ihre Gewichte als Werte.
    public static T NextFromWeightedDistribution<T>(Dictionary<T, double> items)
    {
        double totalWeight = items.Values.Sum();
        double roll        = _random.NextDouble() * totalWeight;
        foreach (var item in items)
        {
            roll -= item.Value;
            if (roll <= 0)
            {
                return item.Key;
            }
        }
        // Sollte eigentlich nie eintreten, aber als Fallback:
        return items.Last().Key;
    }

    // Erzeugt einen Zufallswert aus der Exponentialverteilung.
    // Der Parameter lambda muss größer als 0 sein.
    public static double NextExponential(double lambda)
    {
        if (lambda <= 0)
            throw new ArgumentException("Lambda muss größer als 0 sein.", nameof(lambda));

        double u = 1.0 - _random.NextDouble();
        return -Math.Log(u) / lambda;
    }
}

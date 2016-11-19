using System;
using System.Collections.Generic;
using motto_cgss_core;
using motto_cgss_core.Utility;
using UnityEngine;

public static class Precomputed
{
    public const int QuantizeFactor = 120;
    public static List<Vector3> PathPoints { get; set; }

    public static int GetQuantizedIndex(int from, int to, double t)
    {
        var qf = QuantizeFactor;
        var qt = MathHelper.Clamp((int)Math.Round(t * qf), 0, qf);
        return from * CurrentGame.NumberOfButtons * (qf + 1) + to * (qf + 1) + qt;
    }

    public static Vector3[] GetLinePath(int from, int to, double s, double t)
    {
        // When skipped we see some t=0...
        if (s > t)
            return new Vector3[0];

        var qf = QuantizeFactor;
        var qs = MathHelper.Clamp((int)Math.Round(s * qf), 0, qf);
        var qt = MathHelper.Clamp((int)Math.Round(t * qf), 0, qf);
        var ba = from * CurrentGame.NumberOfButtons * (qf + 1) + to * (qf + 1);
        return PathPoints.GetRange(ba + qs, qt - qs + 1).ToArray();
    }
}
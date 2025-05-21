using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace Bezier_beadando.Calculations
{
    public class BezierComparisonResult
    {
        public float MaxError { get; set; }             // A legnagyobb eltérés a két módszer között
        public float AverageError { get; set; }         // Az átlagos eltérés a két módszer között
        public long CasteljauTimeMs { get; set; }       // de Casteljau algoritmus számítási ideje (ms)
        public long BernsteinTimeMs { get; set; }       // Bernstein algoritmus számítási ideje (ms)
    }

    public static class BezierComparison
    {
        // A két algoritmus összehasonlítását végző függvény
        public static BezierComparisonResult Compare(List<PointF> controlPoints, float step = 0.01f)
        {
            var stopwatch = new Stopwatch();          // Időmérés
            float totalError = 0f;                    // Teljes hibák összege az átlagos hiba számításához
            float maxError = 0f;                      // Legnagyobb eltérés a két módszer között
            int count = 0;

            //de Casteljau algoritmus
            stopwatch.Restart();
            List<PointF> casteljauPoints = new List<PointF>();
            for (float t = 0; t <= 1.0001f; t += step)
            {
                casteljauPoints.Add(BezierCalculator.CalculatePointCasteljau(controlPoints, t));
            }
            stopwatch.Stop();
            long casteljauTime = stopwatch.ElapsedMilliseconds;

            //Bernstein algoritmus
            stopwatch.Restart();
            List<PointF> bernsteinPoints = new List<PointF>();
            for (float t = 0; t <= 1.0001f; t += step)
            {
                bernsteinPoints.Add(BezierCalculator.CalculatePointBernstein(controlPoints, t));
            }
            stopwatch.Stop();
            long bernsteinTime = stopwatch.ElapsedMilliseconds;

            //Eltérések kiszámítása
            for (int i = 0; i < casteljauPoints.Count; i++)
            {
                float dx = casteljauPoints[i].X - bernsteinPoints[i].X;
                float dy = casteljauPoints[i].Y - bernsteinPoints[i].Y;
                float error = (float)Math.Sqrt(dx * dx + dy * dy); // Euklideszi távolság
                totalError += error;
                if (error > maxError) maxError = error;
                count++;
            }

            //Eredmény
            return new BezierComparisonResult
            {
                MaxError = maxError,
                AverageError = totalError / count,
                CasteljauTimeMs = casteljauTime,
                BernsteinTimeMs = bernsteinTime
            };
        }
    }
}

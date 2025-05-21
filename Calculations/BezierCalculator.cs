using System;
using System.Collections.Generic;
using System.Drawing;

namespace Bezier_beadando.Calculations
{
    public static class BezierCalculator
    {
        // Bézier-görbe pontjának kiszámítása de Casteljau algoritmussal.
        // A módszer rekurzív interpolációval számítja ki az adott 't' paraméterhez tartozó pontot.
        public static PointF CalculatePointCasteljau(List<PointF> controlPoints, double t)
        {
            List<PointF> points = new List<PointF>(controlPoints);

            // Addig ismételjük, amíg egyetlen pont marad
            while (points.Count > 1)
            {
                List<PointF> next = new List<PointF>();
                // Minden szomszédos pontból egy új pontot számolunk, ami arányosan interpolált
                for (int i = 0; i < points.Count - 1; i++)
                {
                    double x = (1 - t) * points[i].X + t * points[i + 1].X;
                    double y = (1 - t) * points[i].Y + t * points[i + 1].Y;
                    next.Add(new PointF((float)x, (float)y));
                }
                // Az újonnan kapott pontok lesznek a következő iteráció kiindulópontjai
                points = next;
            }
            // Az utolsó megmaradt pont a Bézier-görbe adott pontja
            return points[0];
        }

        // Bézier-görbe pontjának kiszámítása Bernstein-polinomok segítségével.
        public static PointF CalculatePointBernstein(List<PointF> controlPoints, double t)
        {
            int n = controlPoints.Count - 1;
            double x = 0, y = 0;

            // Minden vezérlőpontra kiszámítjuk a hozzá tartozó Bernstein-értéket
            for (int i = 0; i <= n; i++)
            {
                double bern = Bernstein(n, i, t); // Bernstein-polinom értéke
                x += bern * controlPoints[i].X;
                y += bern * controlPoints[i].Y;
            }
            return new PointF((float)x, (float)y);
        }

        // Binomiális együttható (n alatt a k) kiszámítása.
        public static double BinomialCoefficient(int n, int k)
        {
            // Szélső esetek
            if (k < 0 || k > n) return 0;
            if (k == 0 || k == n) return 1;
            // Szimmetria kihasználása a számítás egyszerűsítésére
            if (k > n - k) k = n - k;
            double result = 1.0;
            for (int i = 1; i <= k; i++)
            {
                result *= (n - (k - i));
                result /= i;
            }
            return result;
        }
        // Bernstein-polinom értékének kiszámítása.
        public static double Bernstein(int n, int i, double t)
        {
            double binCoeff = BinomialCoefficient(n, i);
            double val = binCoeff * Math.Pow(t, i) * Math.Pow(1 - t, n - i);
            return val;
        }
    }
}

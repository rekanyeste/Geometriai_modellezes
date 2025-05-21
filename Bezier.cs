using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Bezier_beadando.Calculations;

namespace Bezier_beadando
{
    public partial class Bezier : Form
    {
        private enum CurveMode
        {
            Both,
            CasteljauOnly,
            BernsteinOnly
        }

        private double currentT = 0.3f; // Aktuális t paraméter (a görbén való hely)
        private List<PointF> cachedCasteljauCurve = new List<PointF>();
        private List<PointF> cachedBernsteinCurve = new List<PointF>();
        private double lastComputedT = -1f; // A legutóbbi számítás t értéke és kontrollpont hash-e
        private string lastControlPointsHash = "";
        private TextBox calculationsBox;
        private CurveMode currentMode = CurveMode.Both;
        private List<PointF> controlPoints = new List<PointF>(); // Kontrollpontokat tároló lista
        private int selectedPointIndex = -1; // Az éppen mozgatott pont indexe és a mozgatás állapota
        private bool isDragging = false;
        private PointF dragOffset;
        private Label degreeLabel;

        public Bezier()
        {
            Button casteljauButton = new Button
            {
                Text = "de Casteljau",
                Location = new Point(10, 40),
                AutoSize = true
            };
            casteljauButton.Click += (s, e) =>
            {
                currentMode = CurveMode.CasteljauOnly;
                Result(currentT, currentMode);
                this.Invalidate();
            };
            this.Controls.Add(casteljauButton);

            Button bernsteinButton = new Button
            {
                Text = "Bernstein",
                Location = new Point(150, 40),
                AutoSize = true
            };
            bernsteinButton.Click += (s, e) =>
            {
                currentMode = CurveMode.BernsteinOnly;
                Result(currentT, currentMode);
                this.Invalidate();
            };
            this.Controls.Add(bernsteinButton);

            Button bothButton = new Button
            {
                Text = "Összehasonlítás",
                Location = new Point(280, 40),
                AutoSize = true
            };
            bothButton.Click += (s, e) =>
            {
                currentMode = CurveMode.Both;
                if (controlPoints.Count > 1)
                {
                    var result = BezierComparison.Compare(controlPoints);
                    calculationsBox.Text =
                        "Összehasonlítás:\r\n" +
                        $"Maximális hiba: {result.MaxError:F6}\r\n" +
                        $"Átlagos hiba: {result.AverageError:F6}\r\n" +
                        $"Casteljau számítási idő: {result.CasteljauTimeMs} ms\r\n" +
                        $"Bernstein számítási idő: {result.BernsteinTimeMs} ms\r\n" +
                        $"Gyorsabb: {(result.CasteljauTimeMs < result.BernsteinTimeMs ? "Casteljau" : "Bernstein")}";
                }
                else
                {
                    calculationsBox.Text = "Adj hozzá legalább 2 pontot az összehasonlításhoz.";
                }
                this.Invalidate();
            };
            this.Controls.Add(bothButton);

            // 3 kontrollpontos görbe
            Button degree3Button = new Button
            {
                Text = "Fokszám 3",
                Location = new Point(10, 80),
                AutoSize = true
            };
            degree3Button.Click += (s, e) =>
            {
                SetControlPoints(4);
            };
            this.Controls.Add(degree3Button);

            // 20 kontrollpontos görbe
            Button degree20Button = new Button
            {
                Text = "Fokszám 20",
                Location = new Point(110, 80),
                AutoSize = true
            };
            degree20Button.Click += (s, e) =>
            {
                SetControlPoints(21);
            };
            this.Controls.Add(degree20Button);

            // 50 kontrollpontos görbe
            Button degree50Button = new Button
            {
                Text = "Fokszám 50",
                Location = new Point(220, 80),
                AutoSize = true
            };
            degree50Button.Click += (s, e) =>
            {
                SetControlPoints(51);
            };
            this.Controls.Add(degree50Button);

            calculationsBox = new TextBox
            {
                Multiline = true,
                Width = 400,
                Height = 300,
                Location = new Point(10, 120),
                Font = new Font("Consolas", 10),
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true
            };
            this.Controls.Add(calculationsBox);
            this.Text = "de Casteljau algoritmus és Bernstein polinom összehasonlítása";
            this.Size = new Size(1500, 900);
            this.DoubleBuffered = true;

            degreeLabel = new Label
            {
                Text = "Fokszám: 0",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            this.Controls.Add(degreeLabel);
            this.MouseDown += BezierForm_MouseDown;
            this.MouseMove += BezierForm_MouseMove;
            this.MouseUp += BezierForm_MouseUp;
            this.Paint += BezierForm_Paint;
        }

        private void BezierForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                for (int i = 0; i < controlPoints.Count; i++)
                {
                    if (Distance(controlPoints[i], e.Location) < 10)
                    {
                        selectedPointIndex = i;
                        isDragging = true;
                        dragOffset = new PointF(controlPoints[i].X - e.X, controlPoints[i].Y - e.Y);
                        return;
                    }
                }
                controlPoints.Add(e.Location);
                UpdateDegreeLabel();
                pontFrissito();
                this.Invalidate();
            }
            else if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < controlPoints.Count; i++)
                {
                    if (Distance(controlPoints[i], e.Location) < 10)
                    {
                        controlPoints.RemoveAt(i);
                        selectedPointIndex = -1;
                        UpdateDegreeLabel();
                        pontFrissito();
                        this.Invalidate();
                        return;
                    }
                }
            }
        }

        private void BezierForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && selectedPointIndex != -1)
            {
                controlPoints[selectedPointIndex] = new PointF(e.X + dragOffset.X, e.Y + dragOffset.Y);
                pontFrissito();
                this.Invalidate();
            }
        }

        private void BezierForm_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        // Kirajzolás canvasre
        private void BezierForm_Paint(object sender, PaintEventArgs e)
        {
            if (controlPoints.Count < 2)
                return;

            Graphics g = e.Graphics;
            Pen controlPen = new Pen(Color.Black, 1);
            Pen casteljauPen = new Pen(Color.Blue, 2);
            Pen bernsteinPen = new Pen(Color.Red, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
            Pen highlightPen = new Pen(Color.Green, 2);

            // Kontrollpontok közti vonalak
            for (int i = 0; i < controlPoints.Count - 1; i++)
            {
                g.DrawLine(controlPen, controlPoints[i], controlPoints[i + 1]);
            }

            // Görbe kirajzolása
            if (currentMode == CurveMode.CasteljauOnly || currentMode == CurveMode.Both)
            {
                if (cachedCasteljauCurve.Count > 1)
                {
                    g.DrawLines(casteljauPen, cachedCasteljauCurve.ToArray());
                }
            }

            if (currentMode == CurveMode.BernsteinOnly || currentMode == CurveMode.Both)
            {
                if (cachedBernsteinCurve.Count > 1)
                {
                    g.DrawLines(bernsteinPen, cachedBernsteinCurve.ToArray());
                }
            }

            // Kontrollpontok megjelenítése
            for (int i = 0; i < controlPoints.Count; i++)
            {
                g.FillEllipse(Brushes.Black, controlPoints[i].X - 5, controlPoints[i].Y - 5, 10, 10);
                if (i == selectedPointIndex)
                {
                    g.DrawRectangle(highlightPen, controlPoints[i].X - 6, controlPoints[i].Y - 6, 12, 12);
                }
            }

            if (controlPoints.Count >= 1)
            {
                PointF start = controlPoints[0];
                Font legendFont = new Font("Arial", 10, FontStyle.Bold);
                if (currentMode == CurveMode.CasteljauOnly || currentMode == CurveMode.Both)
                {
                    g.DrawString("de Casteljau", legendFont, Brushes.Blue, start.X + 10, start.Y - 20);
                }
                if (currentMode == CurveMode.BernsteinOnly || currentMode == CurveMode.Both)
                {
                    g.DrawString("Bernstein", legendFont, Brushes.Red, start.X + 10, start.Y);
                }
            }

            // Újraszámolás (csak a pontok változtatása esetén, teljesítmény miatt)
            string currentHash = HashControlPoints();
            if (controlPoints.Count >= 2 && (currentHash != lastControlPointsHash || Math.Abs(currentT - lastComputedT) > 0.0001f))
            {
                cachedCasteljauCurve.Clear();
                cachedBernsteinCurve.Clear();
                for (double t = 0; t <= 1; t += 0.01f)
                {
                    cachedCasteljauCurve.Add(BezierCalculator.CalculatePointCasteljau(controlPoints, t));
                    cachedBernsteinCurve.Add(BezierCalculator.CalculatePointBernstein(controlPoints, t));
                }
                lastComputedT = currentT;
                lastControlPointsHash = currentHash;
            }
        }

        // Egy adott t érték esetén a két algoritmus eredménye és összehasonlítása
        private void Result(double t, CurveMode mode)
        {
            if (controlPoints.Count < 2)
            {
                calculationsBox.Text = "Adj hozzá legalább 2 pontot a számításhoz.";
                return;
            }
            var stopwatch = new System.Diagnostics.Stopwatch();
            PointF casteljauPoint;
            PointF bernsteinPoint;
            stopwatch.Start();
            casteljauPoint = BezierCalculator.CalculatePointCasteljau(controlPoints, t);
            stopwatch.Stop();
            double casteljauTimeSec = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();
            bernsteinPoint = BezierCalculator.CalculatePointBernstein(controlPoints, t);
            stopwatch.Stop();
            double bernsteinTimeSec = stopwatch.Elapsed.TotalSeconds;
            double dx = casteljauPoint.X - bernsteinPoint.X;
            double dy = casteljauPoint.Y - bernsteinPoint.Y;
            double diff = Math.Sqrt(dx * dx + dy * dy);

            string output = "";
            if (mode == CurveMode.CasteljauOnly)
            {
                output += $"de Casteljau pont (t={t:F3}): ({casteljauPoint.X:F3}, {casteljauPoint.Y:F3})\r\n";
                output += $"Számítási idő: {casteljauTimeSec:F6} s\r\n";
                output += $"Eltérés Bernstein ponttól: {diff:F6}\r\n";
            }
            else if (mode == CurveMode.BernsteinOnly)
            {
                output += $"Bernstein pont (t={t:F3}): ({bernsteinPoint.X:F3}, {bernsteinPoint.Y:F3})\r\n";
                output += $"Számítási idő: {bernsteinTimeSec:F6} s\r\n";
                output += $"Eltérés de Casteljau ponttól: {diff:F6}\r\n";
            }
            else
            {
                output += $"de Casteljau pont (t={t:F3}): ({casteljauPoint.X:F3}, {casteljauPoint.Y:F3}), idő: {casteljauTimeSec:F6} s\r\n";
                output += $"Bernstein pont (t={t:F3}): ({bernsteinPoint.X:F3}, {bernsteinPoint.Y:F3}), idő: {bernsteinTimeSec:F6} s\r\n";
                output += $"Pontok közti eltérés: {diff:F6}\r\n";
            }
            calculationsBox.Text = output;
        }
        // Görbe újraszámolása
        private void pontFrissito()
        {
            cachedCasteljauCurve.Clear();
            cachedBernsteinCurve.Clear();

            if (controlPoints.Count < 2)
            {
                return;
            }

            for (double t = 0; t <= 1; t += 0.01f)
            {
                cachedCasteljauCurve.Add(BezierCalculator.CalculatePointCasteljau(controlPoints, t));
                cachedBernsteinCurve.Add(BezierCalculator.CalculatePointBernstein(controlPoints, t));
            }
        }

        // Automatikusan generált kontrollpontok beállítása (3, 20 és 50 fokszámos gombok)
        private void SetControlPoints(int count)
        {
            controlPoints.Clear();
            Random rnd = new Random();
            int margin = 50;
            int minY = 600;
            int maxY = this.ClientSize.Height - 100;

            for (int i = 0; i < count; i++)
            {
                int x = margin + i * (this.ClientSize.Width - 2 * margin) / (count - 1);
                int y = rnd.Next(minY, Math.Min(maxY, minY + 100));
                controlPoints.Add(new PointF(x, y));
            }
            UpdateDegreeLabel();
            pontFrissito();
            this.Invalidate();
        }

        // Kontrollpontok hash-értéke az újraszámolás eldöntéséhez
        private string HashControlPoints()
        {
            return string.Join(";", controlPoints.Select(p => $"{p.X:F2},{p.Y:F2}"));
        }

        // Fokszám frissítése
        private void UpdateDegreeLabel()
        {
            degreeLabel.Text = $"Fokszám: {Math.Max(controlPoints.Count - 1, 0)}";
        }

        // Két pont közötti távolság számítása
        private float Distance(PointF p1, PointF p2)
        {
            return (float)Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }
    }
}

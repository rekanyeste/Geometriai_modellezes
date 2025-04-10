using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using System.Drawing.Text;

namespace Bezier_beadando
{
    public partial class Bezier : Form
    {
        private enum CurveMode
        {
            Both,            // Mindkét görbét megjelenítjük
            CasteljauOnly,   // Csak a de Casteljau görbét jelenítjük meg
            BernsteinOnly    // Csak a Bernstein görbét jelenítjük meg
        }

        private TextBox calculationsBox;      // A számítások eredményének megjelenítése
        private float currentT = 0.3f;         // Kezdeti t érték, amely a görbén való helyzetet határozza meg
        private TrackBar tTrackBar;            // A t paraméter módosítására szolgáló csúszka
        private CurveMode currentMode = CurveMode.Both;   // A jelenlegi mód, melyik görbét jelenítsük meg
        private List<PointF> controlPoints = new List<PointF>(); // A kontrollpontok listája
        private int selectedPointIndex = -1;   // A kijelölt kontrollpont indexe
        private bool isDragging = false;       // Jelzi, hogy egy pontot épp húzunk
        private PointF dragOffset;             // A húzás közbeni elmozdulás
        private Label degreeLabel;             // A görbe fokszámát mutató címke

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
                this.Invalidate();
            };
            this.Controls.Add(bernsteinButton);

            Button bothButton = new Button
            {
                Text = "Mindkettő",
                Location = new Point(280, 40),
                AutoSize = true
            };
            bothButton.Click += (s, e) =>
            {
                currentMode = CurveMode.Both;
                this.Invalidate(); 
            };
            this.Controls.Add(bothButton);

            // A számításokat megjelenítő TextBox létrehozása
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

            // A t paramétert szabályozó TrackBar létrehozása
            tTrackBar = new TrackBar
            {
                Minimum = 0,
                Maximum = 100,
                TickFrequency = 5,
                Value = (int)(currentT * 100), // Az aktuális t érték beállítása
                Width = 400,
                Location = new Point(10, 430)
            };
            tTrackBar.Scroll += (s, e) =>
            {
                currentT = tTrackBar.Value / 100f; // Az aktuális t érték frissítése a csúszkáról
                this.Invalidate();
            };
            this.Controls.Add(tTrackBar);

            this.Text = "de Casteljau algoritmus és Bernstein polinom összehasonlítása";
            this.Size = new Size(1500, 900);
            this.DoubleBuffered = true; // A képernyő villogásának csökkentése
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
                this.Invalidate();
            }
        }

        private void BezierForm_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void BezierForm_Paint(object sender, PaintEventArgs e)
        {
            if (controlPoints.Count < 2)
                return; // Legalább két kontrollpont szükséges a görbék kirajzolásához

            Graphics g = e.Graphics;
            Pen controlPen = new Pen(Color.Gray, 1) 
            { 
                DashStyle = System.Drawing.Drawing2D.DashStyle.Dash 
            };
            Pen casteljauPen = new Pen(Color.Blue, 2);
            Pen bernsteinPen = new Pen(Color.Red, 2)
            {
                DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
            };
            Pen highlightPen = new Pen(Color.Green, 2);

            // A kontrollpontok közötti vonalak kirajzolása
            for (int i = 0; i < controlPoints.Count - 1; i++)
            {
                g.DrawLine(controlPen, controlPoints[i], controlPoints[i + 1]);
            }

            // A de Casteljau és Bernstein görbék listái
            List<PointF> casteljauCurve = new List<PointF>();
            List<PointF> bernsteinCurve = new List<PointF>();

            // A görbék kiszámítása a t paraméter értékeihez (0-tól 1-ig)
            for (float t = 0; t <= 1; t += 0.01f)
            {
                casteljauCurve.Add(CalculateBezierPointCasteljau(controlPoints, t));  // De Casteljau görbe kiszámítása
                bernsteinCurve.Add(CalculateBezierPointBernstein(controlPoints, t));  // Bernstein görbe kiszámítása
            }

            // A megfelelő görbe kirajzolása a választott mód alapján
            if (currentMode == CurveMode.CasteljauOnly || currentMode == CurveMode.Both)
            {
                if (casteljauCurve.Count > 1)
                    g.DrawLines(casteljauPen, casteljauCurve.ToArray());
            }

            if (currentMode == CurveMode.BernsteinOnly || currentMode == CurveMode.Both)
            {
                if (bernsteinCurve.Count > 1)
                    g.DrawLines(bernsteinPen, bernsteinCurve.ToArray());
            }

            // A kontrollpontok kirajzolása és a számítások megjelenítése
            for (int i = 0; i < controlPoints.Count; i++)
            {
                g.FillEllipse(Brushes.Black, controlPoints[i].X - 5, controlPoints[i].Y - 5, 10, 10);

                if (controlPoints.Count > 1)
                {
                    // A de Casteljau és Bernstein számítási lépések megjelenítése a TextBox-ban
                    string casteljau = GetDeCasteljauSteps(controlPoints, currentT);
                    string bernstein = GetBernsteinCalculations(controlPoints, currentT);
                    calculationsBox.Text = casteljau + "\r\n" + bernstein;
                }
                else
                {
                    calculationsBox.Text = "Adj hozzá legalább 2 pontot a számításhoz.";
                }

                // A kijelölt pont kiemelése
                if (i == selectedPointIndex)
                {
                    g.DrawRectangle(highlightPen, controlPoints[i].X - 6, controlPoints[i].Y - 6, 12, 12);
                }
            }

            // A görbéket jelölő szövegek kirajzolása
            Font legendFont = new Font("Arial", 10, FontStyle.Bold);
            if (controlPoints.Count >= 1)
            {
                PointF start = controlPoints[0];
                if (currentMode == CurveMode.CasteljauOnly || currentMode == CurveMode.Both)
                {
                    e.Graphics.DrawString("de Casteljau", legendFont, Brushes.Blue, start.X + 10, start.Y - 20);
                }
                if (currentMode == CurveMode.BernsteinOnly || currentMode == CurveMode.Both)
                {
                    e.Graphics.DrawString("Bernstein", legendFont, Brushes.Red, start.X + 10, start.Y);
                }
            }
        }

        // A de Casteljau algoritmus lépéseit számító metódus
        private string GetDeCasteljauSteps(List<PointF> points, float t)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("de Casteljau számítási lépések:");
            List<PointF> current = new List<PointF>(points);
            int level = 0;

            // Az összes szinten végigmegyünk, amíg csak egy pont marad
            while (current.Count > 1)
            {
                sb.AppendLine($"Szint {level}:");
                List<PointF> next = new List<PointF>();
                for (int i = 0; i < current.Count - 1; i++)
                {
                    float x = (1 - t) * current[i].X + t * current[i + 1].X;
                    float y = (1 - t) * current[i].Y + t * current[i + 1].Y;
                    next.Add(new PointF(x, y));
                    sb.AppendLine($"  P{i}{level} = ({x:F2}, {y:F2})");
                }
                current = next;
                level++;
            }
            sb.AppendLine($"Eredmény: ({current[0].X:F2}, {current[0].Y:F2})");
            return sb.ToString();
        }

        // A Bernstein polinom számítását végző metódus
        private string GetBernsteinCalculations(List<PointF> points, float t)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Bernstein számítás:");

            int n = points.Count - 1;
            float x = 0, y = 0;
            // Az összes Bernstein polinom számítása
            for (int i = 0; i <= n; i++)
            {
                float coeff = Bernstein(n, i, t); // A Bernstein együttható számítása
                float bx = coeff * points[i].X;   // x koordináta számítása
                float by = coeff * points[i].Y;   // y koordináta számítása
                sb.AppendLine($"  B_{i},{n}({t:F2}) = {coeff:F4} → ({bx:F2}, {by:F2})");
                x += bx;  // Az összegzés
                y += by;
            }

            sb.AppendLine($"Eredmény: ({x:F2}, {y:F2})");
            return sb.ToString();
        }

        // A fokszám frissítése a kontrollpontok számának alapján
        private void UpdateDegreeLabel()
        {
            degreeLabel.Text = $"Fokszám: {Math.Max(controlPoints.Count - 1, 0)}";
        }

        // A de Casteljau algoritmus egy pontjának kiszámítása
        private PointF CalculateBezierPointCasteljau(List<PointF> points, float t)
        {
            if (points.Count == 1) return points[0]; // Ha csak egy pont marad, akkor ez a görbe pontja
            List<PointF> newPoints = new List<PointF>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                float x = (1 - t) * points[i].X + t * points[i + 1].X;
                float y = (1 - t) * points[i].Y + t * points[i + 1].Y;
                newPoints.Add(new PointF(x, y));
            }
            return CalculateBezierPointCasteljau(newPoints, t); // Rekurzív hívás
        }

        // A Bernstein polinom egy pontjának kiszámítása
        private PointF CalculateBezierPointBernstein(List<PointF> points, float t)
        {
            int n = points.Count - 1;
            float x = 0, y = 0;
            for (int i = 0; i <= n; i++)
            {
                float coeff = Bernstein(n, i, t); // A Bernstein együttható számítása
                x += coeff * points[i].X;
                y += coeff * points[i].Y;
            }
            return new PointF(x, y);
        }

        // A Bernstein együttható számítása
        private float Bernstein(int n, int i, float t)
        {
            return Factorial(n) / (Factorial(i) * Factorial(n - i)) * (float)Math.Pow(t, i) * (float)Math.Pow(1 - t, n - i);
        }

        // A faktoriális számítása
        private int Factorial(int n)
        {
            int result = 1;
            for (int i = 1; i <= n; i++)
                result *= i;
            return result;
        }

        // A két pont közötti távolság kiszámítása
        private float Distance(PointF p1, PointF p2)
        {
            return (float)Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }
    }
}

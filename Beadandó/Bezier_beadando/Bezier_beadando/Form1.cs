using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bezier_beadando
{
    public partial class Bezier : Form
    {
        private List<PointF> controlPoints = new List<PointF>();
        private int selectedPointIndex = -1;
        private bool isDragging = false;
        private PointF dragOffset;
        private Label degreeLabel;

        public Bezier()
        {
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
                return;
            Graphics g = e.Graphics;
            Pen controlPen = new Pen(Color.Gray, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
            Pen casteljauPen = new Pen(Color.Blue, 2);
            Pen bernsteinPen = new Pen(Color.Red, 2);
            Pen highlightPen = new Pen(Color.Green, 2);
            for (int i = 0; i < controlPoints.Count - 1; i++)
            {
                g.DrawLine(controlPen, controlPoints[i], controlPoints[i + 1]);
            }
            List<PointF> casteljauCurve = new List<PointF>();
            List<PointF> bernsteinCurve = new List<PointF>();
            for (float t = 0; t <= 1; t += 0.01f)
            {
                casteljauCurve.Add(CalculateBezierPointCasteljau(controlPoints, t));
                bernsteinCurve.Add(CalculateBezierPointBernstein(controlPoints, t));
            }
            if (casteljauCurve.Count > 1) g.DrawLines(casteljauPen, casteljauCurve.ToArray());
            if (bernsteinCurve.Count > 1) g.DrawLines(bernsteinPen, bernsteinCurve.ToArray());
            for (int i = 0; i < controlPoints.Count; i++)
            {
                g.FillEllipse(Brushes.Black, controlPoints[i].X - 5, controlPoints[i].Y - 5, 10, 10);
                if (i == selectedPointIndex)
                {
                    g.DrawRectangle(highlightPen, controlPoints[i].X - 6, controlPoints[i].Y - 6, 12, 12);
                }
            }
        }

        private void UpdateDegreeLabel()
        {
            degreeLabel.Text = $"Fokszám: {Math.Max(controlPoints.Count - 1, 0)}";
        }

        private PointF CalculateBezierPointCasteljau(List<PointF> points, float t)
        {
            if (points.Count == 1) return points[0];
            List<PointF> newPoints = new List<PointF>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                float x = (1 - t) * points[i].X + t * points[i + 1].X;
                float y = (1 - t) * points[i].Y + t * points[i + 1].Y;
                newPoints.Add(new PointF(x, y));
            }

            return CalculateBezierPointCasteljau(newPoints, t);
        }

        private PointF CalculateBezierPointBernstein(List<PointF> points, float t)
        {
            int n = points.Count - 1;
            float x = 0, y = 0;
            for (int i = 0; i <= n; i++)
            {
                float bernstein = Bernstein(n, i, t);
                x += bernstein * points[i].X;
                y += bernstein * points[i].Y;
            }

            return new PointF(x, y);
        }

        private float Bernstein(int n, int i, float t)
        {
            return BinomialCoefficient(n, i) * (float)Math.Pow(t, i) * (float)Math.Pow(1 - t, n - i);
        }

        private int BinomialCoefficient(int n, int k)
        {
            if (k > n - k) k = n - k;
            int result = 1;
            for (int i = 0; i < k; i++)
            {
                result *= (n - i);
                result /= (i + 1);
            }
            return result;
        }

        private float Distance(PointF p1, PointF p2)
        {
            return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
    }
}


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;

namespace Integrales
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        double a, b, interval;
        int q;
        MidPoint s;
        MonteCarlo s1;
        Simpsons s2;

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Value < numericUpDown2.Value)
            {

                if (listBox1.SelectedIndex == 0)
                {
                    Clearing();
                    a = (double)numericUpDown1.Value;
                    b = (double)numericUpDown2.Value;
                    q = (int)numericUpDown3.Value;
                    s = new MidPoint(a, b, (double)numericUpDown3.Value);
                    interval = (b - a) / q;
                    s.EventColumn += OnColumn;
                    s.EventProgress += OnProgress;
                    s.EventFinish += OnFinish;
                    s.EventTime += OnTime;
                    s.Start();
                }
                if (listBox1.SelectedIndex == 1)
                {
                    Clearing();
                    a = (double)numericUpDown1.Value;
                    b = (double)numericUpDown2.Value;
                    q = (int)numericUpDown3.Value;
                    s1 = new MonteCarlo(a, b, (double)numericUpDown3.Value);
                    interval = (b - a) / q;
                    s1.EventPoints += OnPoints;
                    s1.EventNeedPoints += OnNeedPoints;
                    s1.EventProgress += OnProgress;
                    s1.EventFinish += OnFinish;
                    s1.EventTime += OnTime;
                    s1.Start();
                }
                if (listBox1.SelectedIndex == 2)
                {
                    Clearing();
                    a = (double)numericUpDown1.Value;
                    b = (double)numericUpDown2.Value;
                    q = (int)numericUpDown3.Value;
                    s2 = new Simpsons(a, b, (double)numericUpDown3.Value);
                    interval = (double)(b - a) / q;
                    s2.EventSpline += OnSpline1;
                    s2.EventProgress += OnProgress;
                    s2.EventFinish += OnFinish;
                    s2.EventTime += OnTime;
                    s2.Start();
                }
                progressBar1.Maximum = q;
            }
            else label1.Text = "ERROR";
        }

        private void Clearing()
        {
            chart1.Series.Clear();
            chart1.Series.Add("Функция");
            OnSpline();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            chart1.Series[0].Name = "Функция";
            OnSpline();
        }

        void OnNeedPoints(double x, double y)
        {
            if (!chart1.InvokeRequired)
            {
                if (chart1.Series.Count <= 3) chart1.Series.Add("Точки");
                chart1.Series[3].ChartType = SeriesChartType.Bubble;
                chart1.Series[3].Points.AddXY(x, y);
            }
            else
            {
                object[] pars = { x, y };
                Invoke(new MonteCarlo.NeedPoints(OnNeedPoints), pars);
            }
        }

        void OnPoints(double x, double y, double max)
        {
            if (!chart1.InvokeRequired)
            {
                if (chart1.Series.Count <= 2)
                {
                    chart1.Series.Add("Метод2");
                    chart1.Series.Add("Граница");
                    chart1.Series[2].ChartType = SeriesChartType.Line;
                    chart1.Series[2].Points.AddXY(a, 0);
                    chart1.Series[2].Points.AddXY(a, max);
                    chart1.Series[2].Points.AddXY(b, max);
                    chart1.Series[2].Points.AddXY(b, 0);
                    chart1.Series[2].Points.AddXY(a, 0);
                }
                chart1.Series[1].ChartType = SeriesChartType.Bubble;
                chart1.Series[1].Points.AddXY(x, y);
            }
            else
            {
                object[] pars = { x, y, max };
                Invoke(new MonteCarlo.Points(OnPoints), pars);
            }
        }

        void OnColumn(double x, double y)
        {

            if (!chart1.InvokeRequired)
            {
                if (chart1.Series.Count <= 1) chart1.Series.Add("Метод1");
                chart1.Series[1]["PointWidth"] = "1";
                chart1.Series[1].Color = Color.Transparent;
                chart1.Series[1].BorderColor = Color.Orange;
                chart1.Series[1].Points.AddXY(x, y);
            }
            else
            {
                object[] pars = { x, y };
                Invoke(new MidPoint.Column(OnColumn), pars);
            }
        }

        void OnSpline()
        {
            a = (double)numericUpDown1.Value;
            b = (double)numericUpDown2.Value;
            q = (int)numericUpDown3.Value;
            interval = (b - a) / q;
            chart1.Series[0].ChartType = SeriesChartType.Spline;
            double x = a;
            int N = 4 * q + 1;
            for (int i = 1; i <= N; i++)
            {
                double y = func(x);
                chart1.Series[0].Points.AddXY(x, y);
                x += interval / 4;
                label1.Text += x;
            }
        }
        void OnSpline1(double x, double y)
        {
            if (!chart1.InvokeRequired)
            {
                if (chart1.Series.Count <= 1) chart1.Series.Add("Метод3");
                chart1.Series[1].ChartType = SeriesChartType.Spline;
                chart1.Series[1].Points.AddXY(x, y);
                chart1.Series[1].Sort(PointSortOrder.Ascending, "X");
            }
            else
            {
                object[] pars = { x, y };
                Invoke(new Simpsons.Spline(OnSpline1), pars);
            }
        }

        private void OnProgress(int value)
        {
            if (!progressBar1.InvokeRequired)
                progressBar1.Value = value;
            else
            {

                if (s != null) Invoke(new MidPoint.Progress(OnProgress), value);
                if (s1 != null) Invoke(new MonteCarlo.Progress(OnProgress), value);
                if (s2 != null) Invoke(new Simpsons.Progress(OnProgress), value);
            }
        }

        private void OnFinish(double resVal)
        {
            if (!label1.InvokeRequired)
            {
                label1.Text = "Ответ: " + resVal;
            }
            else
            {
                if (s != null) Invoke(new MidPoint.Finish(OnFinish), resVal);
                if (s1 != null) Invoke(new MonteCarlo.Finish(OnFinish), resVal);
                if (s2 != null) Invoke(new Simpsons.Finish(OnFinish), resVal);
            }
        }
        private void OnTime(double resVal)
        {
            if (!label2.InvokeRequired)
            {
                label2.Text = "Время" + resVal;
            }
            else
            {
                if (s != null) Invoke(new MidPoint.Time(OnTime), resVal);
                if (s1 != null) Invoke(new MonteCarlo.Time(OnTime), resVal);
                if (s2 != null) Invoke(new Simpsons.Time(OnTime), resVal);
            }
        }
        double func(double x)
        {
            return (x - 5 * Math.Pow(Math.Sin(x), 2));
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            a = (double)numericUpDown1.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            b = (double)numericUpDown2.Value;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            q = (int)numericUpDown3.Value;
        }

        double maxi()
        {
            const double epsilon = 1e-10;
            double a1 = a;
            double b1 = b;
            double goldenRatio = (1 + Math.Sqrt(5)) / 2;
            double x1, x2;
            while (Math.Abs(b1 - a1) > epsilon)
            {
                x1 = b1 - (b1 - a1) / goldenRatio;
                x2 = a1 + (b1 - a1) / goldenRatio;
                if (func(x1) <= func(x2)) a1 = x1;
                else b1 = x2;
            }
            return func((a1 + b1) / 2);
        }

        private void Scale_Click(object sender, EventArgs e)
        {
            chart1.ChartAreas[0].AxisX.ScaleView.Zoom(0, b);
            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            chart1.ChartAreas[0].AxisY.ScaleView.Zoom(0, maxi());
            chart1.ChartAreas[0].CursorY.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].AxisY.ScrollBar.IsPositionedInside = true;
        }
    }
}
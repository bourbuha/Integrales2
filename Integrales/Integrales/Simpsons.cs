using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Integrales
{
    class Simpsons
    {
        private Thread t = null;
        double a, b;
        int q;
        int parts = 4;
        double h;
        int dP = 0;
        private class Sum{public double value;}
        private Sum res;
        public double Result{get { return res.value; }private set { res.value = value; }}
        public delegate void Spline(double x, double y);
        public event Spline EventSpline;
        public delegate void Progress(int value);
        public event Progress EventProgress;
        public delegate void Finish(double resultValue);
        public event Finish EventFinish;
        public delegate void Time(double resultValue);
        public event Time EventTime;

        public Simpsons(double a, double b, double quantity)
        {
            this.a = a;
            this.b = b;
            this.q = (int)quantity * 2;
            h = (b - a) / (this.q);
            res = new Sum();
            Result = -func(a) + func(b);

        }
        public void Integrate()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Parallel.For(0,parts,new Action<int>(_Integrate));
            double I = (h / 3) * Result;
            sw.Stop();
            if (dP != q) { EventProgress?.Invoke(q / 2); }
            EventFinish?.Invoke(I);
            EventTime?.Invoke(sw.ElapsedMilliseconds);
        }
        private void _Integrate(int part)
        {
            Result = -func(a) + func(b);
            int partsSize = (int)(q / 2) / (parts);
            int ost = (q / 2) - partsSize * parts;
            int st = part * partsSize;
            if (part < ost) st += part;
            else st += ost;
            int fn = (part + 1) * partsSize;
            if (part + 1 < ost) fn += part;
            else fn += ost - 1;
            double sum2 = 0;
            double sum4 = 0;
            for (int i = st; i <= fn; i++)
            {
                Thread.Sleep(100);
                var s2 = func(a + 2 * i * h);
                var s4 = func(a + h * (2 * i + 1));
                sum2 += s2;
                sum4 += s4;
                dP += 1;
                EventProgress?.Invoke(dP);
            }
            Monitor.Enter(res);
            try
            {
                Result += 2 * sum2;
                Result += 4 * sum4;
            }
            finally
            {
                Monitor.Exit(res);
            }
        }
        double func(double x)
        {
            var res = (x - 5 * Math.Pow(Math.Sin(x), 2));
            EventSpline?.Invoke(x, res);
            return (double)res;
        }
        public void Start()
        {
            if (t == null || !t.IsAlive)
            {
                ThreadStart th = new ThreadStart(Integrate);
                t = new Thread(th);
                t.Start();
            }
        }
        public void Stop()
        {
            t.Abort();
            t.Join();
        }
    }
}

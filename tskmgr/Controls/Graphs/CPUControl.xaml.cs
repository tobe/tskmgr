using System;
using System.Threading;
using System.Diagnostics;

namespace tskmgr.Controls
{
    /// <summary>
    /// Interaction logic for CPUControl.xaml
    /// </summary>
    public partial class CPUControl : GraphControl
    {
        PerformanceCounter CpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        public CPUControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Pokreće nit koja kontinuirano učitava nove podatke.
        /// </summary>
        protected override void Read()
        {
            double sum = 0;
            int count = 1;
            this.Peak = 0;
            while (true)
            {
                Thread.Sleep(1000); // Ažuriramo svakih sekundu
                var now = DateTime.Now;

                // Dohvati trenutnu vrijednost
                Trend = CpuCounter.NextValue();

                // Izračunaj peak
                if(Trend > this.Peak) this.Peak = Trend;
                // I prosjek
                sum += Trend; this.Average = sum / count;

                // Dodaj u kolekciju
                ChartValues.Add(new GraphingModel
                {
                    DateTime = now,
                    Value = Trend
                });

                // Postavi nove granice
                SetAxisLimits(now);

                // Pamti samo zadnjih 50 vrijednosti (zbog performansi i preglednosti)
                if (ChartValues.Count > 150) ChartValues.RemoveAt(0);

                count++;
            }
        }
    }
}

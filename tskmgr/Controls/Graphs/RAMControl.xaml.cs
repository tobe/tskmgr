﻿using System;
using System.Threading;

namespace tskmgr.Controls
{
    /// <summary>
    /// Interaction logic for CPUControl.xaml
    /// </summary>
    public partial class RAMControl : GraphControl
    {
        public RAMControl()
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
                Trend = PerformanceInfo.GetUsedMemoryInMiB();

                // Izračunaj peak
                if (trend > this.Peak) this.Peak = trend;
                // I prosjek
                sum += trend; this.Average = sum / count;

                // Dodaj u kolekciju
                ChartValues.Add(new GraphingModel
                {
                    DateTime = now,
                    Value = trend
                });

                // Postavi nove granice
                SetAxisLimits(now);

                // Pamti samo zadnjih 150 vrijednosti (zbog performansi i preglednosti)
                if (ChartValues.Count > 150) ChartValues.RemoveAt(0);

                count++;
            }
        }
    }
}

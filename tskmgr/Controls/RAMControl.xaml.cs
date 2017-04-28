using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Configurations;
using System.Diagnostics;

namespace tskmgr.Controls
{
    /// <summary>
    /// Interaction logic for CPUControl.xaml
    /// </summary>
    public partial class RAMControl : UserControl, INotifyPropertyChanged
    {
        private double _axisMax;
        private double _axisMin;
        private int _trend;
        private double _peak;
        private double _average;

        public RAMControl()
        {
            InitializeComponent();

            var mapper = Mappers.Xy<GraphingModel>()
                .X(model => model.DateTime.Ticks)   // Koristi DateTime.Ticks kao X
                .Y(model => model.Value);           // Koristi vrijednost kao Y

            Charting.For<GraphingModel>(mapper);
            ChartValues = new ChartValues<GraphingModel>();

            // Specificiraj oblik vremena
            DateTimeFormatter = value => new DateTime((long)value).ToString("mm:ss");

            // AxisStep forces the distance between each separator in the X axis
            AxisStep = TimeSpan.FromSeconds(1).Ticks;
            AxisUnit = TimeSpan.TicksPerSecond; // Mapiramo sekunde

            // Postavi granice
            SetAxisLimits(DateTime.Now);

            // Pokreni Task odnosno pozadinsku nit koja je zadužena za dohvaćanje novih podataka
            Task.Factory.StartNew(Read);

            DataContext = this;
        }

        public ChartValues<GraphingModel> ChartValues { get; set; }
        public Func<double, string> DateTimeFormatter { get; set; }
        public double AxisStep { get; set; }
        public double AxisUnit { get; set; }

        public double AxisMax
        {
            get { return _axisMax; }
            set
            {
                _axisMax = value;
                OnPropertyChanged("AxisMax");
            }
        }
        public double AxisMin
        {
            get { return _axisMin; }
            set
            {
                _axisMin = value;
                OnPropertyChanged("AxisMin");
            }
        }
        public double Peak
        {
            get { return _peak; }
            set
            {
                _peak = value;
                OnPropertyChanged("Peak");
            }
        }
        public double Average
        {
            get { return _average; }
            set
            {
                _average = value;
                OnPropertyChanged("Average");
            }
        }
        public int Trend
        {
            get { return _trend; }
            set
            {
                _trend = value;
                OnPropertyChanged("Trend");
            }
        }

        private void Read()
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
                if (_trend > this.Peak) this.Peak = _trend;
                // I prosjek
                sum += _trend; this.Average = sum / count;

                // Dodaj u kolekciju
                ChartValues.Add(new GraphingModel
                {
                    DateTime = now,
                    Value = _trend
                });

                // Postavi nove granice
                SetAxisLimits(now);

                // Pamti samo zadnjih 50 vrijednosti (zbog performansi i preglednosti)
                if (ChartValues.Count > 50) ChartValues.RemoveAt(0);

                count++;
            }
        }

        private void SetAxisLimits(DateTime now)
        {
            AxisMax = now.Ticks + TimeSpan.FromSeconds(1).Ticks; // Forsira os da bude jednu sekundu ispred
            AxisMin = now.Ticks - TimeSpan.FromSeconds(100).Ticks; // i 100 izad
        }
    
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

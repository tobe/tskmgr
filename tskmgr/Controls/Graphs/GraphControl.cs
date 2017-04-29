using LiveCharts;
using LiveCharts.Configurations;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace tskmgr.Controls
{
    /// <summary>
    /// GraphControla koju extendaju (inheritaju) CPUControl i RAMControl.
    /// </summary>
    public class GraphControl : UserControl, INotifyPropertyChanged
    {
        // Drži podatke
        public ChartValues<GraphingModel> ChartValues { get; set; }

        // Formatiranje vremena
        public Func<double, string> DateTimeFormatter { get; set; }

        public double AxisStep { get; set; }
        public double AxisUnit { get; set; }

        protected double axisMax;
        public double AxisMax
        {
            get { return axisMax; }
            set
            {
                axisMax = value;
                OnPropertyChanged("AxisMax");
            }
        }

        protected double axisMin;
        public double AxisMin
        {
            get { return axisMin; }
            set
            {
                axisMin = value;
                OnPropertyChanged("AxisMin");
            }
        }

        protected double peak;
        public double Peak
        {
            get { return peak; }
            set
            {
                peak = value;
                OnPropertyChanged("Peak");
            }
        }

        protected double average;
        public double Average
        {
            get { return average; }
            set
            {
                average = value;
                OnPropertyChanged("Average");
            }
        }

        protected double trend;
        public double Trend
        {
            get { return trend; }
            set
            {
                trend = value;
                OnPropertyChanged("Trend");
            }
        }

        /* Virtualne funkcije koje child klase (CPUControl, RAMControl)
         * inheritaju i specificiraju vlastite implementacije */
        protected virtual void SetAxisLimits(DateTime now) { }
        protected virtual void Read() { }

        /// <summary>
        /// Postavlja graf.
        /// </summary>
        public GraphControl()
        {
            // Stvori novog Mappera
            var mapper = Mappers.Xy<GraphingModel>()
                .X(model => model.DateTime.Ticks)   // Koristi DateTime.Ticks kao X
                .Y(model => model.Value);           // Koristi vrijednost kao Y

            // Specificiraj podatke
            Charting.For<GraphingModel>(mapper);
            this.ChartValues = new ChartValues<GraphingModel>();

            // Specificiraj oblik vremena
            DateTimeFormatter = value => new DateTime((long)value).ToString("mm:ss");

            // AxisStep se koristi za razmak između svakog unosa (?)
            AxisStep = TimeSpan.FromSeconds(1).Ticks;
            AxisUnit = TimeSpan.TicksPerSecond; // Mapiramo sekunde

            // Postavi granice
            SetAxisLimits(DateTime.Now);

            // Pokreni Task odnosno pozadinsku nit koja je zadužena za dohvaćanje novih podataka
            Task.Factory.StartNew(Read);

            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

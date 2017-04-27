using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace tskmgr.Controls
{
    /// <summary>
    /// Interaction logic for ProcessControl.xaml
    /// </summary>
    public partial class ProcessControl : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<ProcessList> ProcessCollection { get; set; }
        private int? selectedIndex = 0;

        private Process _Process = new Process();
        private PerformanceCounter cpuLoad = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        public ProcessControl()
        {
            InitializeComponent();
            this.DataContext = this;
            this.ProcessCollection = _Process.GetProcessList();

            // Stvaranje nove niti za asinkrono ažuriranje sadržaja
            AsyncUpdate();
        }

        private async void AsyncUpdate()
        {
            await Task.Factory.StartNew(() => this.UpdateData(), TaskCreationOptions.LongRunning);
        }

        private void UpdateData()
        {
            // http://stackoverflow.com/questions/19558644/update-an-observablecollection-from-another-collection
            while (true)
            {
                // Pozovi UI nit da očisti sve trenutne procese
                App.Current.Dispatcher.BeginInvoke((Action)delegate {
                    this.ProcessCollection.Clear();
                });

                // Dohvati nove procese
                var newProcesses = _Process.GetProcessList();
                // Pozovi UI nit da doda svaki unos, jedan po jedan -- Smanjuje blokiranje GUI niti na koju se offloada cijela kolekcija.
                foreach (var p in newProcesses)
                {
                    App.Current.Dispatcher.BeginInvoke((Action)delegate {
                        this.ProcessCollection.Add(p);
                    });
                }

                App.Current.Dispatcher.BeginInvoke((Action)delegate {
                    // Ažuriraj selektirane pozive
                    DataGrid.SelectedIndex = (int)this.selectedIndex;

                    // Ažuriraj CPU i RAM Usage
                    this.Processes.Text = String.Format("Processes: {0}%", (int)_Process.GetTotalCPUUsage());
                    this.MemoryUsage.Text = String.Format("Available Memory: {0}MB", (int)_Process.GetTotalMemoryUsage());
                    this.Processes.Text = String.Format("Processes: {0}", this.ProcessCollection.Count.ToString());
                });

                Task.Delay(5000).Wait();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(DataGrid.SelectedIndex != -1) this.selectedIndex = DataGrid.SelectedIndex;
        }
    }
}

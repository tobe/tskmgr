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
using System.Data;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace tskmgr.Controls
{
    /// <summary>
    /// Interaction logic for ProcessControl.xaml
    /// </summary>
    public partial class ProcessControl : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<ProcessList> ProcessCollection { get; set; }
        private int lastPID = 0;

        private Process _Process = new Process();

        private MetroWindow metroWindow = (Application.Current.MainWindow as MetroWindow);

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

                var newProcesses = _Process.GetProcessList(); // Dohvati nove procese
                // Pozovi UI nit da doda svaki unos, jedan po jedan -- Smanjuje blokiranje GUI niti na koju se offloada cijela kolekcija.
                foreach (var p in newProcesses)
                {
                    App.Current.Dispatcher.BeginInvoke((Action)delegate {
                        this.ProcessCollection.Add(p);
                    });
                }
                
                // Par stvarčica na kraju
                App.Current.Dispatcher.BeginInvoke((Action)delegate {
                    // Nakon ažuriranja kolekcije (uključujući i sortiranje), pronađi prethodno selektirani element i reselektiraj ga.
                    int i;
                    for(i = 0; i < this.ProcessCollection.Count; i++)
                    {
                        if (this.ProcessCollection[i].ProcessId == this.lastPID) break;
                    }
                    if(i < this.ProcessCollection.Count)
                        DataGrid.SelectedItem = this.ProcessCollection[i];

                    // Ažuriraj CPU i RAM Usage
                    this.CPUUsage.Text      = String.Format("CPU Usage: {0}%", (int)_Process.GetTotalCPUUsage());
                    this.MemoryUsage.Text   = String.Format("Available Memory: {0}MB", (int)_Process.GetTotalMemoryUsage());
                    this.Processes.Text     = String.Format("Processes: {0}", this.ProcessCollection.Count.ToString());
                });

                Task.Delay(5000).Wait();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Funkcija pozvana nakon što se promijeni korisnički odabir u DataGridu (odabere redak)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProcessList newSelectedProcess = this.DataGrid.SelectedItem as ProcessList;
            if (newSelectedProcess != null) // ili DataGrid.SelectedIndex != -1
            {
                Debug.WriteLine(newSelectedProcess.ProcessName + " - " + newSelectedProcess.ProcessId + ", index:" + this.DataGrid.SelectedIndex);
                this.lastPID = newSelectedProcess.ProcessId;
            }
        }

        private async void EndProcessButton_Click(object sender, RoutedEventArgs e)
        {
            /*Debug.WriteLine("Selected index is: " + this.selectedIndex);
            Debug.WriteLine("Process is: " + this.ProcessCollection[this.selectedIndex].ProcessName + " and PID is: " + this.ProcessCollection[this.selectedIndex].ProcessId);*/

            /*
             * Kako je ObservableCollection bindan sa UI-jem, svaki indeks ProcessList klase je sinkroniziran s DataGridom!
             * Dakle, kako znamo već index selektiranog retka, jednostavno znamo i indeks tog elementa (klase) u ProcessList.
             * */
            try
            {
                ProcessList selectedProcess = this.DataGrid.SelectedItem as ProcessList;

                System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(selectedProcess.ProcessId);
                p.Kill();

                await metroWindow.ShowMessageAsync("Success", String.Format("{0}.exe has been successfully killed.", selectedProcess.ProcessName));
                //this.DataGrid.SelectedItem = this.ProcessCollection[0];
            }
            catch (System.ArgumentException e_)
            {
                await metroWindow.ShowMessageAsync("Error", "There has been trouble killing the process: " + e_.Message);
            }
        }

        private void NewProcessButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

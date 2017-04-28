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
        // Referenca na glavni MahApps prozor
        private MetroWindow metroWindow = (Application.Current.MainWindow as MetroWindow);

        // ObservableCollection kolekcija informacija o procesima
        public ObservableCollection<ProcessList> ProcessCollection { get; set; }

        // Praćenje zadnjeg procesa zbog fokusiranja
        private int lastPID = 0;

        // Process object
        private Process oProcess = new Process();

        public ProcessControl()
        {
            InitializeComponent();
            this.DataContext = this;
            this.ProcessCollection = oProcess.GetProcessList();

            // Stvaranje nove niti za asinkrono ažuriranje sadržaja
            AsyncUpdate();
        }

        /// <summary>
        /// Metoda koja postavlja pozadinsku nit za dohvaćanje novih informacija o procesima
        /// </summary>
        private async void AsyncUpdate()
        {
            await Task.Factory.StartNew(() => this.UpdateData(), TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Asinkrona metoda koja dohvaća nove podatke i ažurira GUI
        /// </summary>
        private void UpdateData()
        {
            // http://stackoverflow.com/questions/19558644/update-an-observablecollection-from-another-collection
            while (true)
            {
                // 1. Pozovi UI nit da očisti sve trenutne procese
                App.Current.Dispatcher.BeginInvoke((Action)delegate {
                    this.ProcessCollection.Clear();
                });

                var newProcesses = oProcess.GetProcessList(); // Dohvati nove procese
                // 2. Pozovi UI nit da doda svaki unos, jedan po jedan -- Smanjuje blokiranje GUI niti na koju se offloada cijela kolekcija.
                foreach (var p in newProcesses)
                {
                    App.Current.Dispatcher.BeginInvoke((Action)delegate {
                        this.ProcessCollection.Add(p);
                    });
                }
                
                // 3. Par stvarčica na kraju
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
                    this.CPUUsage.Text      = String.Format("CPU Usage: {0}%", (int)oProcess.GetTotalCpuUsage);
                    this.MemoryUsage.Text   = String.Format("Available Memory: {0}MB", (int)oProcess.GetTotalMemoryUsage);
                    this.Processes.Text     = String.Format("Processes: {0}", this.ProcessCollection.Count.ToString());
                });

                // I to čini svakih 5 sekundi.
                Task.Delay(5000).Wait();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Metoda pozvana nakon što se promijeni korisnički odabir u DataGridu (odabere redak)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Dohvati trenutni, novoodabrani proces.
            ProcessList newSelectedProcess = this.DataGrid.SelectedItem as ProcessList;
            if (newSelectedProcess == null) return; // ili DataGrid.SelectedIndex == -1

            Debug.WriteLine(newSelectedProcess.ProcessName + " - " + newSelectedProcess.ProcessId + ", index:" + this.DataGrid.SelectedIndex);
            // Ažuriraj lastPID tako da pokazuje na njegov PID.
            this.lastPID = newSelectedProcess.ProcessId;
        }

        /// <summary>
        /// Metoda pozvana nakon što se stisne na dugme "End Process"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void EndProcessButton_Click(object sender, RoutedEventArgs e)
        {
            /*
             * Kako je ObservableCollection bindan sa UI-jem, svaki indeks ProcessList klase je sinkroniziran s DataGridom!
             * Možemo dohvatit trenutno odabrani redak jednostavnim castom.
             * */
            try {
                ProcessList selectedProcess = this.DataGrid.SelectedItem as ProcessList;

                System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(selectedProcess.ProcessId);
                p.Kill();

                // Makni ga sada iz liste -- i GUI se automatski ažurira jer implementiramo INotifyPropertyChanged!
                this.ProcessCollection.Remove(selectedProcess);

                await this.metroWindow.ShowMessageAsync("Success", String.Format("{0}.exe has been successfully killed.", selectedProcess.ProcessName));
            }catch (System.ArgumentException _e) {
                await this.metroWindow.ShowMessageAsync("Error", "There has been trouble killing the process: " + _e.Message);
            }
        }

        /// <summary>
        /// Funkcija pozvana nakon klika na New Process dugme.
        /// Cijelo čekanje korisnikovog inputa odvija se u zasebnom threadu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void NewProcessButton_Click(object sender, RoutedEventArgs e)
        {
            // Dohvati ime procesa
            var result = await metroWindow.ShowInputAsync("New Process", "Input the process name");
            if (result == null) return; // Cancel stisnut

            // Pokušaj ga započeti.
            try {
                System.Diagnostics.Process.Start(result);
                await this.metroWindow.ShowMessageAsync("New Process", "The process " + result + " has been successfully started.");
            }catch(System.ComponentModel.Win32Exception _e) {
                await this.metroWindow.ShowMessageAsync("Error", "There has been an error trying to invoke the process: " + _e.Message);
            }
        }
    }
}

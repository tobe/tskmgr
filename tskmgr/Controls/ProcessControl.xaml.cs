﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.ObjectModel;
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
        /// http://stackoverflow.com/questions/19558644/update-an-observablecollection-from-another-collection
        /// </summary>
        private void UpdateData()
        {
            while (true)
            {
                try
                {
                    // 1. Pozovi UI nit da očisti sve trenutne procese
                    App.Current.Dispatcher.BeginInvoke((Action)delegate
                    {
                        this.ProcessCollection.Clear();
                    });

                    var newProcesses = oProcess.GetProcessList(); // Dohvati nove procese

                    /*
                     * 2. Pozovi UI nit da doda svaki unos, jedan po jedan
                     * Smanjuje blokiranje GUI niti na koju se offloada cijela kolekcija.
                     * 
                     * Ovo također smanjuje i "flicker", odnosno pojavu da ukoliko se nalazi veliki broj procesa u DataGridu,
                     * poput 150, da cijela lista nestane na sekundu i onda opet se stvori sekundu nakon.
                     * Alternativa je da sve procese stavimo u novu kolekciju i zatim postavim ItemSource na nju, ali na taj način
                     * gubimo sortiranje koje se ovako održava među ažuriranjem liste. Također, unutar XAML datoteke, postavljen je
                     * Async parametar na /true/ i uključena je virtualizacija: http://stackoverflow.com/questions/13764579/improve-wpf-datagrid-performance
                     * 
                     * Nažalost, stvar je da se može apsolutno svaka stavka procesa promijeniti, osim njegovog PID-a i imena.
                     * Još jedan workaround bi bio uspoređivati svako svojstvo postojećeg procesa, s novim procesima i na taj način
                     * ažurirati listu -- naravno, dodavati i brisati postojeće procese ako je to potrebno. No međutim, izgleda da 
                     * ne postoji ni elegantno rješenje za ovo: http://stackoverflow.com/questions/19558644/update-an-observablecollection-from-another-collection
                     * Uz to, potrebno je voditi računa o trenutno odabranom elementu i sortiranju.
                     */
                    foreach (var p in newProcesses)
                    {
                        App.Current.Dispatcher.BeginInvoke((Action)delegate
                        {
                            this.ProcessCollection.Add(p);
                        });
                    }

                    // 3. Par stvarčica na kraju
                    App.Current.Dispatcher.BeginInvoke((Action)delegate
                    {
                        // Nakon ažuriranja kolekcije (uključujući i sortiranje), pronađi prethodno selektirani element i reselektiraj ga.
                        int i;
                        for (i = 0; i < this.ProcessCollection.Count; i++)
                        {
                            if (this.ProcessCollection[i].ProcessId == this.lastPID) break;
                        }
                        if (i < this.ProcessCollection.Count)
                            DataGrid.SelectedItem = this.ProcessCollection[i];

                        // Ažuriraj CPU i RAM Usage
                        this.CPUUsage.Text = String.Format("CPU Usage: {0}%", (int)oProcess.GetTotalCpuUsage);
                        this.MemoryUsage.Text = String.Format("Available Memory: {0}MB", (int)oProcess.GetTotalMemoryUsage);
                        this.Processes.Text = String.Format("Processes: {0}", this.ProcessCollection.Count.ToString());
                    });
                }catch(NullReferenceException) {
                    /* Ukoliko se u milisekundi zatvaranja aplikacije izvrši "Invoke", "App" postane null.
                     * Tada moramo prekinuti petlju jer se ionako aplikacija zatvara. */
                    break;
                }

                // I to čini svakih 5 sekundi.
                Task.Delay(5000).Wait();
            }
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

                await this.metroWindow.ShowMessageAsync("Success", String.Format("{0} has been successfully killed.", selectedProcess.ProcessName));
            }catch (Win32Exception _e) {
                await this.metroWindow.ShowMessageAsync("Error", "The associated process could not be terminated: " + _e.Message);
            }catch (NotSupportedException _e) {
                await this.metroWindow.ShowMessageAsync("Error", "You are attempting to call Kill for a process that is running on a remote computer. The method is available only for processes running on the local computer: " + _e.Message);
            }catch(InvalidOperationException _e) {
                await this.metroWindow.ShowMessageAsync("Error", "The process has already exited: " + _e.Message);
            }catch (System.ArgumentException _e) {
                await this.metroWindow.ShowMessageAsync("Error", "There has been trouble ending the application: " + _e.Message);
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
                await this.metroWindow.ShowMessageAsync("New Process", String.Format("{0} has been successfully started.", result));
            }catch(Win32Exception _e) {
                await this.metroWindow.ShowMessageAsync("Error", "An error occurred when opening the associated file: " + _e.Message);
            }catch(ObjectDisposedException _e) {
                await this.metroWindow.ShowMessageAsync("Error", "The process object has already been disposed: " + _e.Message);
            }catch (System.IO.FileNotFoundException _e) {
                await this.metroWindow.ShowMessageAsync("Error", "The PATH environment variable has a string containing quotes: " + _e.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

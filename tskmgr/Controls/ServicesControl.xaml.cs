using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Controls;

namespace tskmgr.Controls
{
    /// <summary>
    /// Interaction logic for ServicesControl.xaml
    /// </summary>
    public partial class ServicesControl : UserControl, INotifyPropertyChanged
    {
        // Referenca na glavni MahApps prozor
        private MetroWindow metroWindow = (Application.Current.MainWindow as MetroWindow);

        // ObservableCollection kolekcija informacija o servisima
        public ObservableCollection<ServiceController> Services { get; set; }

        public ServicesControl()
        {
            InitializeComponent();

            this.DataContext = this;
            // Inicijaliziraj praznu kolekciju servisa
            this.Services = new ObservableCollection<ServiceController>();
        }

        /// <summary>
        /// Pretplata na događaj koji je pozvan nakon što se kontrola učita
        /// Svaki put kad promijenimo tab, ažuriramo jednostavno listu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.ReloadServices();
        }

        /// <summary>
        /// Ažurira listu servisa.
        /// </summary>
        private void ReloadServices()
        {
            // Ukoliko kolekcija nije prazna, isprazni je.
            if (this.Services.Count >= 0) this.Services.Clear();

            // Dohvati sve servise
            ServiceController[] services = ServiceController.GetServices();
            // Iteriraj kroz sve nezaustavljene servise i dodaj ih u kolekciju.
            foreach (ServiceController service in services)
            {
                if (service.Status != ServiceControllerStatus.Stopped) this.Services.Add(service);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Metoda pozvana nakon što se stisne na dugme "Stop Service". Slično kao i kod Process, Applications kontrola.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void StopServiceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ServiceController selectedService = this.DataGrid.SelectedItem as ServiceController;

                if(!selectedService.CanStop)
                {
                    await this.metroWindow.ShowMessageAsync("Error", "The selected service cannot be stopped.");
                    return;
                }

                selectedService.Stop();
                await this.metroWindow.ShowMessageAsync("Success", "Stop signal sent to service successfully.");

                this.ReloadServices();
            }
            catch(Win32Exception _e) {
                await this.metroWindow.ShowMessageAsync("Error", "An error occurred when accessing a system API. : " + _e.Message);
            }catch(InvalidOperationException _e) {
                await this.metroWindow.ShowMessageAsync("Error", "The service was not found. : " + _e.Message);
            }
        }
    }
}

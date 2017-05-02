using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace tskmgr.Controls
{
    /// <summary>
    /// Interaction logic for ApplicationsControl.xaml
    /// </summary>
    public partial class ApplicationsControl : UserControl, INotifyPropertyChanged
    {
        // Referenca na glavni MahApps prozor
        private MetroWindow metroWindow = (Application.Current.MainWindow as MetroWindow);

        // ObservableCollection kolekcija informacija o aplikacijama
        public ObservableCollection<DesktopWindow> Applications { get; set; }

        public ApplicationsControl()
        {
            InitializeComponent();

            this.DataContext = this;
            this.Applications = User32Helper.GetDesktopWindows();
        }

        /// <summary>
        /// Pretplata na događaj koji je pozvan nakon što se kontrola učita
        /// Svaki put kad promijenimo tab, ažuriramo jednostavno listu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Applications.Clear(); // Očisti trenutnu listu i dodaj novu.
            foreach(var application in User32Helper.GetDesktopWindows())
                this.Applications.Add(application);
        }

        // TODO: http://stackoverflow.com/questions/25578305/c-sharp-focus-window-of-a-runing-program

        /// <summary>
        /// Metoda pozvana nakon što se stisne na dugme "End Application". Slično kao i kod Process kontrole.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void EndApplicationButton_Click(object sender, RoutedEventArgs e)
        {
            try {
                // Dohvati trenutnu aplikaciju
                DesktopWindow selectedApplication = this.DataGrid.SelectedItem as DesktopWindow;

                // Pronađi proces po PID-u i ubij ga.
                System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById((int)selectedApplication.ProcessId);
                p.Kill();

                // Makni aplikaciju iz liste -- refreshamo UI.
                this.Applications.Remove(selectedApplication);

                // Poruka!
                await this.metroWindow.ShowMessageAsync("Success", String.Format("{0}.exe has been successfully ended.", selectedApplication.Title));
            }catch (System.ArgumentException _e) {
                await this.metroWindow.ShowMessageAsync("Error", "There has been trouble ending the application: " + _e.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

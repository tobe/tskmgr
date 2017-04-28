using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace tskmgr.Controls
{
    /// <summary>
    /// Interaction logic for ApplicationsControl.xaml
    /// </summary>
    public partial class ApplicationsControl : UserControl, INotifyPropertyChanged
    {
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
            this.Applications.Clear();
            foreach(var application in User32Helper.GetDesktopWindows())
                this.Applications.Add(application);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

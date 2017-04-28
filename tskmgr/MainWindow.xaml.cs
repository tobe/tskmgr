using System;
using System.Collections.Generic;
using System.Linq;
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
using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using MahApps.Metro.Controls.Dialogs;

namespace tskmgr
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        // Boolovi da znamo jesu li prozori već pokrenuti (jer se svaki pokreće u svojoj niti)
        private bool CPUWindowRunning = false;
        private bool RAMWindowRunning = false;

        // Niti
        private Thread CPUThread;
        private Thread RAMThread;

        // Prozori
        private CPUWindow CpuWindow;
        private RAMWindow RamWindow;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void CPUWindowButton_Click(object sender, RoutedEventArgs e)
        {
            // Provjeri je li već pokrenut.
            if(this.CPUWindowRunning)
            {
                await (this as MetroWindow).ShowMessageAsync("Warning", "The CPU graph window is already running. Close it first before continuing.");
                return;
            }

            // Nije pokrenut, stvori novi -- http://stackoverflow.com/a/21568316
            CPUThread = new Thread(() =>
            {
                this.CpuWindow = new CPUWindow();
                this.CpuWindow.Closing += OnCPUWindowClose;
                this.CpuWindow.Show();

                this.CpuWindow.Closed += (sender2, e2) =>
                    this.CpuWindow.Dispatcher.InvokeShutdown();

                System.Windows.Threading.Dispatcher.Run();
            });

            CPUThread.SetApartmentState(ApartmentState.STA);
            CPUThread.Start();

            this.CPUWindowRunning = true;
        }

        private async void RAMWindowButton_Click(object sender, RoutedEventArgs e)
        {
            // Provjeri je li već pokrenut.
            if (this.RAMWindowRunning)
            {
                await (this as MetroWindow).ShowMessageAsync("Warning", "The RAM graph window is already running. Close it first before continuing.");
                return;
            }

            // Nije pokrenut, stvori novi -- http://stackoverflow.com/a/21568316
            RAMThread = new Thread(() =>
            {
                this.RamWindow = new RAMWindow();
                this.RamWindow.Closing += OnRAMWindowClose;
                this.RamWindow.Show();

                this.RamWindow.Closed += (sender2, e2) =>
                    this.RamWindow.Dispatcher.InvokeShutdown();

                System.Windows.Threading.Dispatcher.Run();
            });

            RAMThread.SetApartmentState(ApartmentState.STA);
            RAMThread.Start();

            this.RAMWindowRunning = true;
        }

        private void OnCPUWindowClose(object sender, CancelEventArgs e)
        {
            System.Windows.Threading.Dispatcher.FromThread(CPUThread).InvokeShutdown();
            this.CPUWindowRunning = false;
        }

        private void OnRAMWindowClose(object sender, CancelEventArgs e)
        {
            System.Windows.Threading.Dispatcher.FromThread(RAMThread).InvokeShutdown();
            this.RAMWindowRunning = false;
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            // Prvo izgasi threadove pa app ;)
            if(CPUThread != null)
                System.Windows.Threading.Dispatcher.FromThread(CPUThread).InvokeShutdown();
            if(RAMThread != null)
                System.Windows.Threading.Dispatcher.FromThread(RAMThread).InvokeShutdown();

            App.Current.Shutdown();
        }
    }
}

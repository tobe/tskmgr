﻿using MahApps.Metro.Controls;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using MahApps.Metro.Controls.Dialogs;
using System.Threading.Tasks;

namespace tskmgr.Controls
{
    /// <summary>
    /// Interaction logic for NetworkControl.xaml
    /// </summary>
    public partial class NetworkControl : UserControl
    {
        // Referenca na glavni MahApps prozor
        private MetroWindow metroWindow = (Application.Current.MainWindow as MetroWindow);

        // P/Invokeaj funkciju iz WINAPI-ja
        [DllImport("iphlpapi.dll", SetLastError = true)]
        static extern int GetBestInterface(UInt32 DestAddr, out UInt32 BestIfIndex);

        // Sučelje
        private NetworkInterface interfaceInfo;
        public string InterfaceDescription { get; set; }

        // Vrijednosti
        private ChartValues<long> Bytes;
        private ChartValues<long> Packets;

        // Stvari za graf
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }

        public NetworkControl()
        {
            InitializeComponent();

            // Pronađi sučelje
            this.InvokeInterface();

            SeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Fill = (SolidColorBrush)new BrushConverter().ConvertFromString("#41b1e1"),
                    Title = "Megabytes",
                    Values = this.Bytes
                },
                new ColumnSeries
                {
                    Fill = (SolidColorBrush)new BrushConverter().ConvertFromString("#d35400"),
                    Title = "Kilopackets",
                    Values = this.Packets
                }
            };

            Labels = new[] {
                "Received",
                "Sent",
                "Incoming discarded",
                "Incoming erroneous",
                "Incoming unknown",
                "Outgoing discarded",
                "Outgoing erroneous"
            };
            Formatter = value => value.ToString("0.#");

            this.DataContext = this;


            // Ukoliko imamo sučelje započni nit za asinkrono ažuriranje sadržaja
            if (this.interfaceInfo != null)
                this.AsyncUpdate();
        }

        /// <summary>
        /// Poziva Task za asinkroni update grafa
        /// </summary>
        private async void AsyncUpdate()
        {
            await Task.Factory.StartNew(() => this.UpdateStats(), TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Ažurira podatke sa sučelja.
        /// </summary>
        private void UpdateStats()
        {
            while(true)
            {
                // Dohvati najnoviju statistiku
                IPv4InterfaceStatistics interfaceStats = this.interfaceInfo.GetIPv4Statistics();

                // Ažuriraj vrijednosti, tj dodaj ih
                this.Bytes = new ChartValues<long> { interfaceStats.BytesReceived / 1000000, interfaceStats.BytesSent / 1000000 };
                this.Packets = new ChartValues<long> {
                    (interfaceStats.NonUnicastPacketsReceived + interfaceStats.UnicastPacketsReceived) / 1000,
                    (interfaceStats.NonUnicastPacketsSent + interfaceStats.UnicastPacketsSent) / 1000,
                    interfaceStats.IncomingPacketsDiscarded,
                    interfaceStats.IncomingPacketsWithErrors,
                    interfaceStats.IncomingUnknownProtocolPackets,
                    interfaceStats.OutgoingPacketsDiscarded,
                    interfaceStats.OutgoingPacketsWithErrors
                };

                if (this.SeriesCollection == null) return;

                App.Current.Dispatcher.BeginInvoke((Action)delegate
                {
                    this.SeriesCollection[0].Values = this.Bytes;
                    this.SeriesCollection[1].Values = this.Packets;
                });

                // Svaku minutu ažuriraj graf
                Task.Delay(60000).Wait();
            }
        }

        /// <summary>
        /// Dohvaća sučelje i ažurira podatke.
        /// </summary>
        private async void InvokeInterface()
        {
            try
            {
                // Pokušaj dohvatit apsolutno najbolje sučelje -- ono spojeno na internet
                UInt32 interfaceindex;
                int result = GetBestInterface(134744072, out interfaceindex); // 8.8.8.8 = 134744072
                if (result != 0)
                    throw new Win32Exception(result);

                // Nakon toga probaj dohvatit zapravo sučelje po indexu.
                this.interfaceInfo = GetNetworkInterfaceByIndex(interfaceindex);
                if (this.interfaceInfo == null) throw new Win32Exception();
                this.InterfaceDescription = this.interfaceInfo.Description;
            }
            catch (Win32Exception)
            {
                await this.metroWindow.ShowMessageAsync("Error", "Cannot retrieve interface data. This view cannot continue.");
                return;
            }
        }

        // http://www.pinvoke.net/default.aspx/iphlpapi.getbestinterface
        private NetworkInterface GetNetworkInterfaceByIndex(uint index)
        {
            // Search in all network interfaces that support IPv4.
            NetworkInterface ipv4Interface = (from thisInterface in NetworkInterface.GetAllNetworkInterfaces()
                                              where thisInterface.Supports(NetworkInterfaceComponent.IPv4)
                                              let ipv4Properties = thisInterface.GetIPProperties().GetIPv4Properties()
                                              where ipv4Properties != null && ipv4Properties.Index == index
                                              select thisInterface).SingleOrDefault();

            return (ipv4Interface != null) ? ipv4Interface : null;
        }
    }
}

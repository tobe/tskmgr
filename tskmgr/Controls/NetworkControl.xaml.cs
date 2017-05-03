using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
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
using LiveCharts;
using LiveCharts.Wpf;
using MahApps.Metro.Controls.Dialogs;
using System.Diagnostics;

namespace tskmgr.Controls
{
    /// <summary>
    /// Interaction logic for NetworkControl.xaml
    /// </summary>
    public partial class NetworkControl : UserControl
    {
        // Referenca na glavni MahApps prozor
        private MetroWindow metroWindow = (Application.Current.MainWindow as MetroWindow);

        // Potrebna funkcija iz WINAPI-ja
        [DllImport("iphlpapi.dll", SetLastError = true)]
        static extern int GetBestInterface(UInt32 DestAddr, out UInt32 BestIfIndex);

        // Sučelje
        private NetworkInterface interfaceInfo;
        public string InterfaceDescription { get; set; }

        // Vrijednosti
        private ChartValues<long> Bytes;
        private ChartValues<long> Packets;

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }

        public NetworkControl()
        {
            InitializeComponent();

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

            Labels = new[] { "Received", "Sent", "Incoming discarded", "Incoming erroneous", "Incoming unknown", "Outgoing discarded", "Outgoing erroneous"};
            Formatter = value => value.ToString("N");

            this.DataContext = this;
        }

        private void UpdateStats()
        {
            IPv4InterfaceStatistics interfaceStats = this.interfaceInfo.GetIPv4Statistics();

            // Ažuriraj vrijednosti, tj dodaj ih
            this.Bytes   = new ChartValues<long> { interfaceStats.BytesReceived/1000000, interfaceStats.BytesSent/1000000 };
            this.Packets = new ChartValues<long> {
                (interfaceStats.NonUnicastPacketsReceived + interfaceStats.UnicastPacketsReceived) / 1000,
                (interfaceStats.NonUnicastPacketsSent + interfaceStats.UnicastPacketsSent) / 1000,
                interfaceStats.IncomingPacketsDiscarded,
                interfaceStats.IncomingPacketsWithErrors,
                interfaceStats.IncomingUnknownProtocolPackets,
                interfaceStats.OutgoingPacketsDiscarded,
                interfaceStats.OutgoingPacketsWithErrors
            };
        }

        private async void InvokeInterface()
        {
            // Pokušaj dohvatit apsolutno najbolje sučelje -- ono spojeno na internet
            try
            {
                UInt32 interfaceindex;
                int result = GetBestInterface(134744072, out interfaceindex); // 8.8.8.8 = 134744072
                if (result != 0)
                    throw new Win32Exception(result);

                // Nakon toga probaj dohvatit zapravo sučelje po indexu.
                this.interfaceInfo = GetNetworkInterfaceByIndex(interfaceindex);
                if (this.interfaceInfo == null) throw new Win32Exception();
                this.InterfaceDescription = this.interfaceInfo.Description;

                // Sve super sad dohvati statistiku (ne throwa ništa)
                this.UpdateStats();
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

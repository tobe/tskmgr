﻿using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Windows;
using System.Windows.Controls;

namespace tskmgr.Controls
{
    /// <summary>
    /// Interaction logic for SpecificationsControl.xaml
    /// </summary>
    public partial class SpecificationsControl : UserControl
    {
        // Referenca na glavni MahApps prozor
        private MetroWindow metroWindow = (Application.Current.MainWindow as MetroWindow);

        // Lista podataka.
        List<SpecsItem> Specifications = new List<SpecsItem>();

        public SpecificationsControl()
        {
            InitializeComponent();

            this.GetOSInformation();
            this.GetCPUInformation();
            this.GetGPUInformation();
            this.GetMBOInformation();

            this.SpecsList.ItemsSource = Specifications;
        }

        /// <summary>
        /// Dohvaća informacije o matičnoj ploči (ili barem pokuša!)
        /// </summary>
        private async void GetMBOInformation()
        {
            try {
                var mbo = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard").Get().Cast<ManagementObject>().First();
                this.Specifications.Add(new SpecsItem() { Title = "MBO manufacturer", Value = mbo["Manufacturer"] != null ? mbo["Manufacturer"].ToString() : "Unknown" });
                this.Specifications.Add(new SpecsItem() { Title = "MBO model", Value = mbo["Product"] != null ? mbo["Product"].ToString() : "Unknown" });
                this.Specifications.Add(new SpecsItem() { Title = "MBO replaceable?", Value = (bool)mbo["Replaceable"] ? "Yes" : "No" });
            }
            catch (Exception e) {
                await this.metroWindow.ShowMessageAsync("Error", "An error occured while querying for WMI/MBO data: " + e.Message);
            }
        }

        /// <summary>
        /// Dohvaća informacije o grafičkoj kartici.
        /// </summary>
        private async void GetGPUInformation()
        {
            try {
                var gpu = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController").Get().Cast<ManagementObject>().First();
                this.Specifications.Add(new SpecsItem() { Title = "GPU ID", Value = gpu["DeviceID"] != null ? gpu["DeviceID"].ToString() : "Unknown" });
                this.Specifications.Add(new SpecsItem() { Title = "GPU name", Value = gpu["Name"] != null ? gpu["Name"].ToString() : "Unknown" });
                this.Specifications.Add(new SpecsItem() { Title = "GPU memory", Value = gpu["AdapterRAM"] != null ? gpu["AdapterRAM"].ToString() : "Unknown" });
                this.Specifications.Add(new SpecsItem() { Title = "GPU DAC type", Value = gpu["AdapterDACType"] != null ? gpu["AdapterDACType"].ToString() : "Unknown" });
                this.Specifications.Add(new SpecsItem() { Title = "GPU driver version", Value = gpu["DriverVersion"] != null ? gpu["DriverVersion"].ToString() : "Unknown" });
                this.Specifications.Add(new SpecsItem() { Title = "GPU video processor", Value = gpu["VideoProcessor"] != null ? gpu["VideoProcessor"].ToString() : "Unknown" });
            }
            catch (Exception e) {
                await this.metroWindow.ShowMessageAsync("Error", "An error occured while querying for WMI/GPU data: " + e.Message);
            }
        }

        /// <summary>
        /// Dohvaća informacije o procesoru.
        /// </summary>
        private async void GetCPUInformation()
        {
            try {
                var cpu = new ManagementObjectSearcher("SELECT * FROM Win32_Processor").Get().Cast<ManagementObject>().First();
                this.Specifications.Add(new SpecsItem() { Title = "CPU ID", Value = cpu["ProcessorId"] != null ? cpu["ProcessorId"].ToString() : "Unknown" });
                this.Specifications.Add(new SpecsItem() { Title = "CPU name", Value = cpu["Name"] != null ? cpu["Name"].ToString() : "Unknown" });
                this.Specifications.Add(new SpecsItem() { Title = "CPU description", Value = cpu["Caption"] != null ? cpu["Caption"].ToString() : "Unknown" });
                this.Specifications.Add(new SpecsItem() { Title = "CPU frequency (MHz)", Value = cpu["MaxClockSpeed"] != null ? cpu["MaxClockSpeed"].ToString() : "Unknown" });
                this.Specifications.Add(new SpecsItem() { Title = "CPU L2 cache size (MB)", Value = cpu["L2CacheSize"] != null ? ((uint)cpu["L2CacheSize"]).ToString() : "Unknown" });
                this.Specifications.Add(new SpecsItem() { Title = "CPU L3 cache size (MB)", Value = cpu["L2CacheSize"] != null ? ((uint)cpu["L3CacheSize"]).ToString() : "Unknown"});
                this.Specifications.Add(new SpecsItem() { Title = "CPU cores", Value = cpu["NumberOfCores"] != null ? cpu["NumberOfCores"].ToString() : "Unknown" });
                this.Specifications.Add(new SpecsItem() { Title = "CPU threads", Value = cpu["NumberOfLogicalProcessors"] != null ? cpu["NumberOfLogicalProcessors"].ToString() : "Unknown" });
            }
            catch(ManagementException e) {
                await this.metroWindow.ShowMessageAsync("Error", "An error occured while querying for WMI/CPU data: " + e.Message);
            }
        }

        /// <summary>
        /// Dohvaća informacije o operacijskom sustavu.
        /// </summary>
        private void GetOSInformation()
        {
            this.Specifications.Add(new SpecsItem() { Title = "OS version", Value = Environment.OSVersion.ToString() });
            this.Specifications.Add(new SpecsItem() { Title = "OS architecture", Value = Environment.Is64BitOperatingSystem ? "64 Bit" : "32 Bit" });
            this.Specifications.Add(new SpecsItem() { Title = "Computer name", Value = Environment.MachineName });
            this.Specifications.Add(new SpecsItem() { Title = "Processor count", Value = Environment.ProcessorCount.ToString() });
            this.Specifications.Add(new SpecsItem() { Title = "Memory installed (MB)", Value = PerformanceInfo.GetTotalMemoryInMiB().ToString() });
        }
    }

    public class SpecsItem
    {
        public string Title { get; set; }
        public string Value { get; set; }
    }
}

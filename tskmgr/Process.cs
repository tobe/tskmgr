using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace tskmgr
{
    public class ProcessList : INotifyPropertyChanged
    {
        private int processId;
        public int ProcessId
        {
            get { return this.processId; }
            set
            {
                this.processId = value;
                this.NotifyPropertyChanged("ProcessId");
            }
        }
        private string processName;
        public string ProcessName {
            get { return this.processName; }
            set
            {
                this.processName = value;
                this.NotifyPropertyChanged("ProcessName");
            }
        }
        private System.TimeSpan processTime;
        public System.TimeSpan ProcessTime {
            get { return this.processTime; }
            set
            {
                this.processTime = value;
                this.NotifyPropertyChanged("ProcessTime");
            }
        }
        private int threadCount;
        public int ThreadCount {
            get { return this.threadCount; }
            set
            {
                this.threadCount = value;
                this.NotifyPropertyChanged("ThreadCount");
            }
        }
        private long workingSet64;
        public long WorkingSet64 {
            get { return this.workingSet64; }
            set
            {
                this.workingSet64 = value;
                this.NotifyPropertyChanged("WorkingSet64");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName) {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

    }

    public class Process
    {
        PerformanceCounter CpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        PerformanceCounter MemCounter = new PerformanceCounter("Memory", "Available MBytes");

        public Process() { }

        public float GetTotalCpuUsage
        {
            get
            {
                return this.CpuCounter.NextValue();
            }
        }

        public float GetTotalMemoryUsage
        {
            get
            {
                return this.MemCounter.NextValue();
            }
        }

        /// <summary>
        /// Vraća kolekciju (ObservableCollection) ProcessList klasâ.
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<ProcessList> GetProcessList()
        {
            ObservableCollection<ProcessList> returnList = new ObservableCollection<ProcessList>();

            // Dohvati sve nove procese i jednostavno za svaki kreiraj klasu
            System.Diagnostics.Process[] localProcesses = System.Diagnostics.Process.GetProcesses();
            foreach (var p in localProcesses)
            {
                ProcessList singleProcess   = new ProcessList();
                singleProcess.ProcessId     = p.Id;
                singleProcess.ProcessName   = (p.ProcessName != "Idle") ? p.ProcessName + ".exe" : p.ProcessName;
                singleProcess.ProcessTime   = (p.Id != 0) ? p.TotalProcessorTime : new System.TimeSpan(0, 0, 0);
                singleProcess.ThreadCount   = p.Threads.Count;
                singleProcess.WorkingSet64  = p.WorkingSet64;

                // I dodaj u kolekciju.
                returnList.Add(singleProcess);
            }

            return returnList;
        }
    }
}
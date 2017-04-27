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
        private string processTime;
        public string ProcessTime {
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
        private string priorityClass;
        public string PriorityClass
        {
            get { return this.priorityClass; }
            set
            {
                this.priorityClass = value;
                this.NotifyPropertyChanged("PriorityClass");
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

        private string ParsePriority(ProcessPriorityClass processPriority) 
        {
            switch (processPriority)
            {
                case ProcessPriorityClass.Normal:
                    return "Normal";
                case ProcessPriorityClass.Idle:
                    return "Idle";
                case ProcessPriorityClass.High:
                    return "High";
                case ProcessPriorityClass.RealTime:
                    return "Realtime";
                case ProcessPriorityClass.BelowNormal:
                    return "Below normal";
                case ProcessPriorityClass.AboveNormal:
                    return "Above normal";
                default:
                    return "Unknown";
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
                try {
                    singleProcess.ProcessTime = p.TotalProcessorTime.ToString(@"h\h\ m\m");
                }catch(System.ComponentModel.Win32Exception e) {
                    singleProcess.ProcessTime = "0h 0m";
                }
                singleProcess.ThreadCount   = p.Threads.Count;
                singleProcess.WorkingSet64  = p.WorkingSet64 / 1000;
                try {
                    singleProcess.PriorityClass = this.ParsePriority(p.PriorityClass);
                }catch(Exception e) {
                    singleProcess.PriorityClass = "Unknown";
                }

                // I dodaj u kolekciju.
                returnList.Add(singleProcess);
            }

            return returnList;
        }
    }
}
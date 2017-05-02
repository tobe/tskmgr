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
            get { return this.CpuCounter.NextValue(); }
        }

        public float GetTotalMemoryUsage
        {
            get { return this.MemCounter.NextValue(); }
        }

        /// <summary>
        /// Parsira prioritet procesa.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private string ParsePriority(System.Diagnostics.Process p) 
        {
            // Pokušaj dohvatit i parsirat prioritet (ukoliko imamo prava i sl.)
            try {
                switch (p.PriorityClass)
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
            }catch {
                return "Unknown";
            }
        }

        /// <summary>
        /// Parsira ukupno vrijeme procesora koje zauzima proces p
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private string ParseTotalProcessTime(System.Diagnostics.Process p)
        {
            try {
                return p.TotalProcessorTime.ToString(@"h\h\ m\m");
            }catch {
                return "0h 0m";
            }
        }

        /// <summary>
        /// Parsira System.Diagnostics.Process u vlastitu Process klasu. Na ovaj način izbjegavamo višak nepotrebnih
        /// informacija i offloadamo količinu podataka koju GUI mora preuzeti (jer je bindan s ObservableCollector 
        /// kolekcijom tipa ProcessList)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public ProcessList ParseProcessIntoProcessList(System.Diagnostics.Process p)
        {
            ProcessList singleProcess   = new ProcessList();
            singleProcess.ProcessId     = p.Id;
            singleProcess.ProcessName   = (p.ProcessName != "Idle") ? p.ProcessName + ".exe" : p.ProcessName;
            singleProcess.ProcessTime   = this.ParseTotalProcessTime(p);
            singleProcess.ThreadCount   = p.Threads.Count;
            singleProcess.WorkingSet64  = p.WorkingSet64 / 1000;
            singleProcess.PriorityClass = this.ParsePriority(p);

            return singleProcess;
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
                // I dodaj u kolekciju.
                returnList.Add(this.ParseProcessIntoProcessList(p));
            }

            return returnList;
        }
    }
}
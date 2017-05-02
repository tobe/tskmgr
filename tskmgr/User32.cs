using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

/**
* Original code is from http://stackoverflow.com/a/23889907
* */
namespace tskmgr
{
    public class DesktopWindow : INotifyPropertyChanged
    {
        private string title;
        public string Title {
            get { return this.title; }    
            set { this.title = value; this.NotifyPropertyChanged("Title"); }
        }

        private uint processId;
        public uint ProcessId
        {
            get { return this.processId; }
            set { this.processId = value; this.NotifyPropertyChanged("ProcessId"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

    public class User32Helper
    {
        public delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowText",
            ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
            ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction,
            IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public static ObservableCollection<DesktopWindow> GetDesktopWindows()
        {
            var collection = new ObservableCollection<DesktopWindow>();
            EnumDelegate filter = delegate (IntPtr hWnd, int lParam)
            {
                var result = new StringBuilder(255);
                GetWindowText(hWnd, result, result.Capacity + 1);
                string title = result.ToString();

                var isVisible = !string.IsNullOrEmpty(title) && IsWindowVisible(hWnd);

                uint pid;
                var ProcessId = GetWindowThreadProcessId(hWnd, out pid);

                if (title.Length > 0 && isVisible)
                    collection.Add(new DesktopWindow { Title = title, ProcessId = pid });

                return true;
            };

            EnumDesktopWindows(IntPtr.Zero, filter, IntPtr.Zero);
            return collection;
        }
    }
}

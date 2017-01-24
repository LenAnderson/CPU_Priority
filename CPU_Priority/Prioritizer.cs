using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace CPU_Priority
{
    class Prioritizer
    {
        public Dictionary<string, string> processes { get; set; }
        private ManagementEventWatcher startWatch;

        public Prioritizer(Dictionary<string, string> processes)
        {
            this.processes = processes;
            InitEventWatchers();
        }


        public void Stop()
        {
            startWatch.Stop();
        }


        private void InitEventWatchers()
        {
            startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            startWatch.EventArrived += ProcessStarted;
            startWatch.Start();
        }

        private void ProcessStarted(object sender, EventArrivedEventArgs e)
        {
            Console.WriteLine(e.NewEvent.Properties["ProcessName"].Value.ToString() + "  -  " + e.NewEvent.Properties["ProcessID"].Value.ToString());
            try
            {
                Process process = Process.GetProcessById(Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value));
                UpdatePriority(process);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //Process process = Process.GetProcessById(Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value));
            //UpdatePriority(process);
        }

        private void UpdatePriority(Process process)
        {
            string[] priority = processes.Where(pair => pair.Key.ToLower() == process.ProcessName.ToLower()).Select(pair => pair.Value).ToArray();
            if (priority.Length > 0)
            {
                switch (priority[0])
                {
                    case "Idle":
                        process.PriorityClass = ProcessPriorityClass.Idle;
                        break;
                    case "Below Normal":
                        process.PriorityClass = ProcessPriorityClass.BelowNormal;
                        break;
                    case "Normal":
                        process.PriorityClass = ProcessPriorityClass.Normal;
                        break;
                    case "Above Normal":
                        process.PriorityClass = ProcessPriorityClass.AboveNormal;
                        break;
                    case "High":
                        process.PriorityClass = ProcessPriorityClass.High;
                        break;
                    case "Realtime":
                        process.PriorityClass = ProcessPriorityClass.RealTime;
                        break;
                }
            }
        }
    }
}

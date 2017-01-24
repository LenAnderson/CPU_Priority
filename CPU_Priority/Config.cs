using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU_Priority
{
    class Config
    {
        public bool startMinimized = false;
        public bool minimizeToTray = true;
        public Dictionary<string, string> processes = new Dictionary<string, string>();
    }
}

using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CPU_Priority
{
    public partial class MainForm : Form
    {
        Config config;
        Prioritizer prioritizer;

        public MainForm()
        {
            InitializeComponent();
            if (File.Exists(@"config.json"))
            {
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(@"config.json"));
            }
            else
            {
                config = new Config();
            }

            if (config.startMinimized)
            {
                this.WindowState = FormWindowState.Minimized;
            }
            chkStartWithWindows.Checked = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", "CPU_Priority", null) != null;
            chkStartMinimized.Checked = config.startMinimized;
            chkMinimizeToTray.Checked = config.minimizeToTray;

            foreach (KeyValuePair<string,string> process in config.processes)
            {
                lstProcesses.Rows.Add(new string[]{ process.Key, process.Value});
            }

            prioritizer = new Prioritizer(config.processes);
        }


        private void saveConfig()
        {
            File.WriteAllText(@"config.json", JsonConvert.SerializeObject(config));
            if (prioritizer != null)
            {
                prioritizer.processes = config.processes;
            }
        }


        #region: processes
        private void lstProcesses_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            config.processes.Remove(e.Row.Cells[0].Value.ToString());
            saveConfig();
        }

        private void lstProcesses_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.FormattedValue != null && lstProcesses.Rows[e.RowIndex].Cells[1].Value != null)
            {
                if (lstProcesses.Rows[e.RowIndex].Cells[0].Value != null)
                {
                    config.processes.Remove(lstProcesses.Rows[e.RowIndex].Cells[0].Value.ToString());
                }
                config.processes[e.FormattedValue.ToString()] = lstProcesses.Rows[e.RowIndex].Cells[1].Value.ToString();
            }
            else if (e.FormattedValue != null && lstProcesses.Rows[e.RowIndex].Cells[0].Value != null)
            {
                config.processes[lstProcesses.Rows[e.RowIndex].Cells[0].Value.ToString()] = e.FormattedValue.ToString();
            }
            saveConfig();
        }
        #endregion


        #region: preferences
        private void chkStartWithWindows_CheckedChanged(object sender, EventArgs e)
        {
            if (chkStartWithWindows.Checked)
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Micrsofot\Windows\CurrentVersion\Run", "CPU_Priority", Path.GetFullPath(@"CPU_Priority.exe"));
            }
            else
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key.GetValueNames().Contains("CPU_Priority"))
                    {
                        key.DeleteValue("CPU_Priority");
                    }
                }
            }
        }

        private void chkStartMinimized_CheckedChanged(object sender, EventArgs e)
        {
            config.startMinimized = chkStartMinimized.Checked;
            saveConfig();
        }

        private void chkMinimizeToTray_CheckedChanged(object sender, EventArgs e)
        {
            config.minimizeToTray = chkMinimizeToTray.Checked;
            saveConfig();
        }
        #endregion


        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (config.minimizeToTray && this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void systray_DoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.WindowState = FormWindowState.Minimized;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            prioritizer.Stop();
        }
    }
}

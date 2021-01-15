using IWshRuntimeLibrary;
using Microsoft.Win32;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinQT
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mqttHandler = new MqttHandler();
            var task = mqttHandler.ConnectAsync();
            SystemEvents.PowerModeChanged += OnPowerChange;

            Application.Run(new WinQTApplicationContext());
            

        }
        

        private static void OnPowerChange(object s, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    break;
                case PowerModes.Suspend:
                    break;
            }
        }
    }

    public class WinQTApplicationContext : ApplicationContext
    {
        private NotifyIcon trayIcon;

        public WinQTApplicationContext()
        {
            // Initialize Tray Icon
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Configuration));
            trayIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.mosquitto,
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Configuration", Show),
                    new MenuItem("Autostart", new MenuItem[] {
                        new MenuItem("Enable", EnableAutostart),
                        new MenuItem("Disable", DisableAutostart)
                    }),
                    new MenuItem("-"),
                    new MenuItem("Exit", Exit)
                }),
                Text = "WinQT",
                Visible = true
            };
        }

        void SetWindows(object sender, EventArgs e)
        {

        }

        void Show(object sender, EventArgs e)
        {
            var configuration = new Configuration
            {
                ShowInTaskbar = true
            };
            configuration.Show();
        }

        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;

            Application.Exit();
        }

        void EnableAutostart(object sender, EventArgs e)
        {
            if (!System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\WinQT.lnk"))
            {
                WshShell wshShell = new WshShell();
                IWshRuntimeLibrary.IWshShortcut shortcut;
                string startUpFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

                // Create the shortcut
                shortcut = (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(startUpFolderPath + "\\WinQT.lnk");
                shortcut.TargetPath = Application.ExecutablePath;
                shortcut.WorkingDirectory = Application.StartupPath;
                shortcut.Description = "Launch WinQT";
                // shortcut.IconLocation = Application.StartupPath + @"\App.ico";
                shortcut.Save();
            }
        }

        void DisableAutostart(object sender, EventArgs e)
        {
            string startUpFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

            DirectoryInfo di = new DirectoryInfo(startUpFolderPath);
            FileInfo[] files = di.GetFiles("*.lnk");

            foreach (FileInfo fi in files)
            {
                string shortcutTargetFile = GetShortcutTargetFile(fi.FullName);

                if (shortcutTargetFile.EndsWith("WinQT.exe", StringComparison.InvariantCultureIgnoreCase))
                {
                    System.IO.File.Delete(fi.FullName);
                }
            }
        }

        public string GetShortcutTargetFile(string shortcutFilename)
        {
            string pathOnly = Path.GetDirectoryName(shortcutFilename);
            string filenameOnly = Path.GetFileName(shortcutFilename);

            Shell32.Shell shell = new Shell32.Shell();
            Shell32.Folder folder = shell.NameSpace(pathOnly);
            Shell32.FolderItem folderItem = folder.ParseName(filenameOnly);
            if (folderItem != null)
            {
                Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink;
                return link.Path;
            }

            return String.Empty; // Not found
        }
    }
}

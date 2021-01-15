using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using MQTTnet.Server;
using WinQT.Properties;

namespace WinQT
{
    public partial class Configuration : Form
    {
        private bool ValueChanged = false;

        public Configuration()
        {
            InitializeComponent();
            SettingsGrid.SelectedObject = Settings.Default;
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            ValueChanged = true;
        }

        private void Configuration_FormClosed(object sender, FormClosedEventArgs e)
        {
            Settings.Default.Save();
            if (ValueChanged)
            {
                if (MessageBox.Show("Setting was changed\nRestart now?", "Updated", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Application.Restart();
                }
            }
        }
    }
}

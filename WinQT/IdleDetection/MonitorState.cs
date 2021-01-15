using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinQT.IdleDetection
{
    class MonitorState
    {
        /*[DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SystemParametersInfo(int uiAction, int uiParam, bool pvParam, int fWinIni);

        public static bool screenOff()
        {
            bool screenSaverRunning = true;
            IntPtr test = new IntPtr();
            MonitorState.SystemParametersInfo(0x0072, 0, screenSaverRunning, 0);
            Console.WriteLine("test:" + screenSaverRunning);
            return screenSaverRunning;
        }*/

        // P/Invoke declarations
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_SCREENSAVE = 0xF140;
        private const int SPI_GETSCREENSAVERRUNNING = 0x0072;
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SystemParametersInfo(int action, int param, ref int retval, int updini);

        /*protected override void WndProc(ref Message m)
        {
            Console.WriteLine(m.ToString());
            if (m.Msg == WM_SYSCOMMAND && (m.WParam.ToInt32() & 0xfff0) == SC_SCREENSAVE) ScreenSaverActive();
            base.WndProc(ref m);
        }
        private void ScreenSaverActive()
        {
            // Screen saver turned on, start timer to check when it turns off
            Console.WriteLine("Screen saver active on at {0}", DateTime.Now);
            mTimer.Enabled = true;
        }
        private void mTimer_Tick(object sender, EventArgs e)
        {
            // Checks if the screen saver is still active
            int active = 1;
            SystemParametersInfo(SPI_GETSCREENSAVERRUNNING, 0, ref active, 0);
            if (active == 0)
            {
                Console.WriteLine("Screen saver off at {0}", DateTime.Now);
                mTimer.Enabled = false;
            }
        }*/
    }
}

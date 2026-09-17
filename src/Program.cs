using System;
using System.Threading;
using System.Windows.Forms;

namespace RMDarkHelper
{
    internal static class Program
    {
        private const string MutexName = @"Local\RMDarkHelper_7AEE8B2E-8D52-4C20-ABCB-5E73779DDB11";

        [STAThread]
        private static void Main()
        {
            bool createdNew;
            using (Mutex singleInstance = new Mutex(true, MutexName, out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show(
                        "RM Dark Helper is already running.",
                        "RM Dark Helper",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                try
                {
                    NativeMethods.SetProcessDpiAwarenessContext(new IntPtr(-4));
                }
                catch
                {
                    try { NativeMethods.SetProcessDPIAware(); } catch { }
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                if (!NativeMethods.MagInitialize())
                {
                    MessageBox.Show(
                        "The Windows Magnification API could not be initialized.",
                        "RM Dark Helper",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    Application.Run(new ControllerForm());
                }
                finally
                {
                    NativeMethods.MagUninitialize();
                }
            }
        }
    }
}

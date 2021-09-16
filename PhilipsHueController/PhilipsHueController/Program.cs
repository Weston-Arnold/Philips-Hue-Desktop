using PhilipsHueController.Helpers;
using System;
using System.Windows.Forms;

namespace PhilipsHueController
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ConfigHelpers.DeleteSettingByKey("AppKey");
            Application.Run(new Dashboard());
        }
    }
}

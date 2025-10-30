using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChekScanner;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        using var mainForm = new MainForm();
        Application.Run(mainForm);
    }
}

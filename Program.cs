using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Threading;

namespace Downloader
{
    class Program
    {
        private static Downloader dl;
        public static String dlpfad="";
        public static WebClient myWebClient;
        public static Boolean cancel = false;
        // Quick-Edit-Modus aktivieren
        #region QuickEdit
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int mode);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetStdHandle(int handle);



        const int STD_INPUT_HANDLE = -10;
        const int ENABLE_QUICK_EDIT_MODE = 0x40 | 0x80;
        #endregion

        public static void EnableQuickEditMode()
        {
            int mode;
            IntPtr handle = GetStdHandle(STD_INPUT_HANDLE);
            GetConsoleMode(handle, out mode);
            mode |= ENABLE_QUICK_EDIT_MODE;
            SetConsoleMode(handle, mode);
        }

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private delegate bool EventHandler(CtrlType sig);

        // Sample Console CTRL handler
        private static bool Handler(CtrlType sig)
        {
            bool handled = false;
            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                    {
                        if (File.Exists(dlpfad))
                        {                          
                            cancel = true;
                            myWebClient.Dispose();
                            myWebClient.CancelAsync();                 
                            while(File.Exists(dlpfad))
                            {
                                try
                                {
                                    File.Delete(dlpfad);
                                }            
                                catch (Exception e)  // dauert eine Weile, bis die File freigegeben wird.
                                {
 
                                }
                            }

                        }
                            
                    
                    }
                    // return false if you want the process to exit.
                    // returning true, causes the system to display a dialog
                    // giving the user the choice to terminate or continue
                    
                    
                    break;
                default:
                    return handled;
            }
            return handled;
        }

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler,
        bool add);

  

        static void Main(string[] args)
        {
          
            SetConsoleCtrlHandler(new EventHandler(Handler), true);
            EnableQuickEditMode();
            dl = new Downloader();
         

        }



    }
}

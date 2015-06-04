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


        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }


        private static bool Handler(CtrlType sig)
        {
            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:

                    if (File.Exists(dlpfad))
                    {
                        cancel = true;
                        myWebClient.Dispose();
                        myWebClient.CancelAsync();
                        while (File.Exists(dlpfad))
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

                    return false;
                default:
                    return true;
            }
        }
  
       

                            
                    
                    
                    // return false if you want the process to exit.
                    // returning true, causes the system to display a dialog
                    // giving the user the choice to terminate or continue
                    
                    
           

      

  

        static void Main(string[] args)
        {
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);
    
            EnableQuickEditMode();
            dl = new Downloader();
         

        }



    }
}

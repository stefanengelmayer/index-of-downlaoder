﻿using System;
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
        // Quick-Edit-Modus aktivieren
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int mode);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetStdHandle(int handle);

        const int STD_INPUT_HANDLE = -10;
        const int ENABLE_QUICK_EDIT_MODE = 0x40 | 0x80;

        public static void EnableQuickEditMode()
        {
            int mode;
            IntPtr handle = GetStdHandle(STD_INPUT_HANDLE);
            GetConsoleMode(handle, out mode);
            mode |= ENABLE_QUICK_EDIT_MODE;
            SetConsoleMode(handle, mode);
        }

        static void Main(string[] args)
        {
            EnableQuickEditMode();
            new Downloader();
        }
    }
}

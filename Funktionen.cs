using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Downloader
{
    class Funktionen
    {

        public static void Lösche_Zeile()
        {
            Console.WriteLine("                                                                                                                                                                               ");
        }

        public static void Zeile_Farbig(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = Config.standard;
        }

    }
}

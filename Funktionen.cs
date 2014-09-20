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

        //public static void Zeile_Farbig(string text, ConsoleColor color)   (noch) nicht notwendig?
        //{
        //    Console.ForegroundColor = color;
        //    Console.WriteLine(text);
        //    Console.ForegroundColor = Config.standard;
        //}


        public static void Buffer_höhe_erhöhen(int zahl)  // Wichtig, dass das Prog. bei einem "überlauf" des Buffers noch richtig funzt
        {
            try
            {
                Console.SetBufferSize(Console.BufferWidth, Console.BufferHeight + zahl);
            }
            catch (Exception e)
            {
                Console.Clear();
                Console.WriteLine(e.ToString());
            }
        }

    }
}

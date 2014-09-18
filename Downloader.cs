using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Downloader
{
    public class Downloader
    {
        // Programm Variablen
        private Config config = new Config(ConfigType.DownloadConfig); //initiale Config kann zur Laufzeit geändert werden für andere werte


        // Download Variablen
        private string pfad = ""; //wird durch Config() gesetzt.
        private string leerzeichen = " ";
        private string Ordner = null;
        private string Speicherpfad = null;
        private string Speicherpfad_neu = null;

        public Downloader()
        {
            pfad = config.ReadConfig();
            Run();
        }

        /// <summary>
        /// Aktiviert die Downloadroutine!
        /// </summary>
        private void Run()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Bitte geben Sie die zu durchsuchende URL ein: \n(Qick Edit Modus ist an)");

            string url = Console.ReadLine();

            Console.WriteLine("Leerzeichen kodiert als: (zuerst leer lassen, manche Server verwenden ein +)");
            leerzeichen = Console.ReadLine();

            if (leerzeichen == " ")
                Console.WriteLine("Leerzeichen als Standard gespeichert.");
            else
                Console.WriteLine("Leerzeichen als {0} dargestellt", leerzeichen);

            Thread.Sleep(700); // fancy

            Console.WriteLine("Speicherpfad: " + pfad);

            Thread.Sleep(1000); // fancy

            Console.WriteLine("Speicherpfad erweitern:  (falls kein Über-Ordner gewünscht ist, bitte Enter drücken)");
            Ordner = Console.ReadLine();

            if (Ordner.EndsWith(@"/"))
                Ordner = Ordner.TrimEnd('/');

            if (!Ordner.EndsWith(@"\"))
                Ordner += @"\";

            Console.WriteLine("Speicherpfad: " + pfad + Ordner);
            Speicherpfad = pfad + Ordner;

            string dir = "";

            GetFilesFromHTTP http = new GetFilesFromHTTP(this);
            http.HoleDateien(url, dir, "");
        }

        public string GetLeerzeichen()
        {
            return leerzeichen;
        }

        public string GetSpeicherpfad()
        {
            return Speicherpfad;
        }

        public string GetNewSpeicherpfad()
        {
            return Speicherpfad_neu;
        }

        public void Restart()
        {
            Run();
        }

        public Config GetConfig()
        {
            return config;
        }
    }
}

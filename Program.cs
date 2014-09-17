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
        public static string pfad = ""; //wird durch Config() gesetzt.
        public static string leerzeichen = " ";
        public static string Ordner = null;
        public static string Speicherpfad = null;
        public static string Speicherpfad_neu = null;

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
            pfad = Config();

            EnableQuickEditMode();
            Console.WriteLine("\nHi,\n\nDer Downloader ist für \"Index of\" Seiten gedacht.\n\n");
            Start();
        }

        public static string Config()
        {

            string startup = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            startup = startup + @"\";
            try
            {
                string[] Zeile = File.ReadAllLines(startup + "config_downloader.cfg");
                for (int i = 0; i < Zeile.Length; i++)
                {
                    if (Zeile[i].Contains("Downloadpfad:"))
                    {
                        return Zeile[i + 1];
                    }
                }

            }
            catch (FileNotFoundException) //Wenn es die config_downloader.cfg
            {
                Console.WriteLine("\"config_downloader.cfg\" wurde nicht gefunden. Bitte Erstellen mit:\nDownloadpfad:\n[Ihr gewünschter Pfad] z.B. C:\\dl\\");
            }

            return "";

        }


        public static void Start()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Bitte geben Sie die zu durchsuchende URL ein: \n(Qick Edit Modus ist an)");

            string url = Console.ReadLine();
            
            Console.WriteLine("Leerzeichen kodiert als: (zuerst leer lassen, manche Server verwenden ein +)");
            Program.leerzeichen = Console.ReadLine();
            
            if (Program.leerzeichen == " ") 
                Console.WriteLine("Leerzeichen als Standard gespeichert.");
            else 
                Console.WriteLine("Leerzeichen als {0} dargestellt", Program.leerzeichen);
            
            Thread.Sleep(700); // fancy

            Console.WriteLine("Speicherpfad: " + pfad);
            
            Thread.Sleep(1000); // fancy
            
            Console.WriteLine("Speicherpfad erweitern:  (falls kein Über-Ordner gewünscht ist, bitte Enter drücken)");
            Program.Ordner = Console.ReadLine();
            
            if (Program.Ordner.EndsWith(@"/")) 
                Program.Ordner = Program.Ordner.TrimEnd('/');
            
            if (!Program.Ordner.EndsWith(@"\")) 
                Program.Ordner += @"\";
            
            Console.WriteLine("Speicherpfad: " + pfad + Ordner);
            Speicherpfad = pfad + Ordner;
            string dir = "";


            GetallFilesFromHttp.HoleDateien(url, dir, "");
        }

    }


    public static class GetallFilesFromHttp
    {



        public static string GetDirectoryListingRegexForUrl(string url)
        {

            return "\\\"([^\"]*)\\\"";

        }


        public static void HoleDateien(string url, string DIR, string letzterzusatz)
        {

            string offlinepfad = DIR;
            Boolean ersterOrdner = true;
            HttpWebRequest request = null;
            if (DIR != "") url = url + letzterzusatz;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
            }
            catch (Exception e)
            {
                Console.WriteLine("Fehler in der URL: " + e.ToString());
                Program.Start();
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {


                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string html = reader.ReadToEnd();

                        Regex regex = new Regex(GetDirectoryListingRegexForUrl(url));
                        MatchCollection matches = regex.Matches(html);
                        if (matches.Count > 0)
                        {
                            Boolean dir = false;
                            string tmp = "";
                            string tmp2 = "";
                            foreach (Match match in matches)
                            {

                                if (match.Success)
                                {
                                    tmp = match.ToString();
                                    tmp2 = tmp.Replace("%20", Program.leerzeichen);
                                    //Console.WriteLine(tmp2);  //Debug-Text


                                    if (dir)
                                    {
                                        tmp = Regex.Replace(tmp, "\"", string.Empty);
                                        if (ersterOrdner)
                                        {
                                            ersterOrdner = false;
                                            continue;
                                        }

                                        Console.ForegroundColor = ConsoleColor.DarkRed;
                                        Console.WriteLine("Gehe in Ordner " + tmp.Replace("%20", " "));
                                        Console.ForegroundColor = ConsoleColor.White;
                                        HoleDateien(url, DIR + tmp, tmp);
                                        dir = false;

                                    }

                                    if (tmp.Contains("[DIR]"))
                                    {
                                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                                        Console.WriteLine("Directory gefunden");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        dir = true;
                                        continue;
                                    }

                                    if (tmp.EndsWith("/\""))
                                    {
                                        tmp = Regex.Replace(tmp, "\"", string.Empty);
                                        if (ersterOrdner)
                                        {
                                            ersterOrdner = false;
                                            continue;
                                        }
                                        Console.ForegroundColor = ConsoleColor.DarkRed;
                                        Console.WriteLine("Gehe in Ordner /" + tmp.Replace("%20", " "));
                                        Console.WriteLine(url + tmp);
                                        Console.ForegroundColor = ConsoleColor.White;
                                        HoleDateien(url, DIR + tmp, tmp);

                                    }


                                    if (

                                        (tmp.Contains(".mp3")) || tmp.Contains(".aac") || tmp.Contains(".wav")
                                        || tmp.Contains(".avi") || tmp.Contains(".mkv") || tmp.Contains(".mp4")
                                        || tmp.Contains(".txt") || tmp.Contains(".sub") || tmp.Contains(".nfo")
                                        || tmp.Contains(".idx") || tmp.Contains(".srt")

                                        )
                                    {
                                        tmp = Regex.Replace(tmp, "\"", string.Empty);
                                        tmp = tmp.Replace("amp;", string.Empty);
                                        Console.WriteLine(LadeDatei(url, tmp, DIR, offlinepfad));
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                }
                            }

                        }


                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Fehler. " + e.ToString());
            }

            if (DIR != "")
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Gehe einen Ordner zurück");
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nDownload von " + url.Replace("%20", " ") + " komplett abgeschlossen.\n\n");
            Console.ForegroundColor = ConsoleColor.White;
            Thread.Sleep(2000);
            Program.Start();

        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">komplette URL</param>
        /// <param name="datei">datei=Dateiname.Endung</param>
        /// <param name="dir">aktuelles Verzeichnis</param>
        /// <param name="offlinepfad">aktuelles Verzeichnis</param>
        /// <returns></returns>
        public static string LadeDatei(string url, string datei, string dir, string offlinepfad)
        {
            string myStringWebResource = null;
            string pfad = Program.Speicherpfad + dir.Replace("/", @"\");
            string file = pfad + dir + datei.Replace("%20", Program.leerzeichen);
            pfad = pfad.Replace("%20", " ");
            offlinepfad = offlinepfad.Replace("/", @"\");
            Program.Speicherpfad_neu = Program.Speicherpfad + offlinepfad;
            Program.Speicherpfad_neu = Program.Speicherpfad_neu.Replace("%20", " ");



            if (!Directory.Exists(Program.Speicherpfad_neu))
            {
                Directory.CreateDirectory(Program.Speicherpfad_neu);
            }

            WebClient myWebClient = new WebClient();

            if (File.Exists(Program.Speicherpfad_neu + datei.Replace("%20", " "))) { return "Datei schon vorhanden"; }
            myStringWebResource = url + datei;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Lade  {0}  runter  ....", datei.Replace("%20", " "));
            Console.ForegroundColor = ConsoleColor.White;
            datei = datei.Replace("%20", " ");
            try
            {
                myWebClient.DownloadFile(myStringWebResource, Program.Speicherpfad_neu + datei);


            }
            catch (WebException e)
            {
                Console.WriteLine("Bitte nochmal versuchen: " + e.ToString());
            }
            Console.ForegroundColor = ConsoleColor.Red;
            return ("\nDownload der Datei abgeschlossen, gespeichert in:  " + pfad);

        }

    }

    //public class WebDownload : WebClient    !!! ka ob des was bringt??!?!?!.... !!!
    //{
    //    /// <summary>
    //    /// Time in milliseconds
    //    /// </summary>
    //    public int Timeout { get; set; }

    //    public WebDownload() : this(60000) { }

    //    public WebDownload(int timeout)
    //    {
    //        this.Timeout = timeout;
    //    }

    //    protected override WebRequest GetWebRequest(Uri address)
    //    {
    //        var request = base.GetWebRequest(address);
    //        if (request != null)
    //        {
    //            request.Timeout = this.Timeout;
    //        }
    //        return request;
    //    }
    //}
}

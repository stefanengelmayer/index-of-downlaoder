using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;

namespace Downloader
{
    public class GetFilesFromHTTP
    {
        private Downloader downloader;
        private HttpWebResponse response;
        private StreamReader reader;
        
        //für die Stats während des Downloads 
        private int top;
        static string statusstring; 

        // async download variablen
        bool completed = false;
        static double length=0;
        static double length_old = 0;
        double speed=0;
        static int[] speed_durchschnitt = new int[5] { 0, 0, 0, 0, 0 };
        static bool durchschnitt_verfügbar = false;
        
        long dl_total;
        long dl_aktuell;
        int dl_progress;
        string pathToCheck = String.Empty;
        Stopwatch sw = new Stopwatch();

        public GetFilesFromHTTP(Downloader downloader)
        {
            this.downloader = downloader;
        }

        public void HoleDateien(string url, string DIR, string letzterzusatz)
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
                downloader.Restart();
            }


            try
            {
                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream());
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
                            tmp2 = tmp.Replace("%20", downloader.GetLeerzeichen());

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


                            if (downloader.GetConfig().CorrectMediaType(tmp))
                            {
                                tmp = Regex.Replace(tmp, "\"", string.Empty);
                                tmp = tmp.Replace("amp;", string.Empty);
                                if (tmp.Contains("."))
                                {
                                    Console.WriteLine(LadeDatei(url, tmp, DIR, offlinepfad));
                                }
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Clear();
                Console.WriteLine(e.Message);
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

            downloader.Restart();
        }

        /// <summary>
        /// Downloadet alle Dateien einer Index Of Webseite
        /// </summary>
        /// <param name="url">komplette URL</param>
        /// <param name="datei">datei=Dateiname.Endung</param>
        /// <param name="dir">aktuelles Verzeichnis</param>
        /// <param name="offlinepfad">aktuelles Verzeichnis</param>
        /// <returns></returns>
        public string LadeDatei(string url, string datei, string dir, string offlinepfad)
        {
            string myStringWebResource = null;
            string pfad = downloader.GetSpeicherpfad() + dir.Replace("/", @"\");
            string file = pfad + dir + datei.Replace("%20", downloader.GetLeerzeichen());
            pfad = pfad.Replace("%20", " ");
            offlinepfad = offlinepfad.Replace("/", @"\");

            string save_path = downloader.GetSpeicherpfad() + offlinepfad;
            save_path = save_path.Replace("%20", " ");

            if (!Directory.Exists(save_path))
            {
                Directory.CreateDirectory(save_path);
            }

            WebClient myWebClient = new WebClient();
            myWebClient.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(myWebClient_DownloadFileCompleted);
            myWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(myWebClient_DownloadProgressChanged);
            // to check for the file in the callback handler
            pathToCheck = save_path + datei;

            if (File.Exists(save_path + datei.Replace("%20", " "))) 
            { 
                return "Datei schon vorhanden"; 
            }
            
            myStringWebResource = url + datei;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Lade  {0}  runter  ....", datei.Replace("%20", " "));
            Console.ForegroundColor = ConsoleColor.White;
            datei = datei.Replace("%20", " ");
            

            try
            {
                completed = false;
                sw.Start();
                myWebClient.DownloadFileAsync(new Uri(myStringWebResource), save_path + datei);
                Console.WriteLine("File: {0}", pathToCheck.Replace("%20", " "));
                top = Console.CursorTop;
                Console.CursorVisible = false;

                while (!completed)
                {
                    if (Console.CursorTop + 10 > Console.BufferHeight) Funktionen.Buffer_höhe_erhöhen(200);
                   
                        Thread.Sleep(200);
                        Console.SetCursorPosition(0, top);                   
                        Funktionen.Lösche_Zeile();              
                        Funktionen.Lösche_Zeile();      
                        Console.SetCursorPosition(0, top);
                       
                        aktueller_speed(pathToCheck, speed, sw, dl_total, dl_aktuell, dl_progress);
                        
                        Console.WriteLine(statusstring);
                    

                }

                Console.CursorVisible = true; 
            }
            catch (WebException e)
            {
                Console.WriteLine("Bitte nochmal versuchen: " + e.ToString());
            }
            Console.ForegroundColor = ConsoleColor.Red;
            return ("\nDownload der Datei abgeschlossen, gespeichert in:  " + pfad);

        }

        void myWebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            dl_total = e.TotalBytesToReceive;
            dl_aktuell = e.BytesReceived;
            dl_progress = e.ProgressPercentage;
           //moved: aktueller_speed()
        }

        void myWebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            completed = true;
            sw.Stop();
            sw.Reset();
            Console.SetCursorPosition(1, top);
            Funktionen.Lösche_Zeile();
            Funktionen.Lösche_Zeile();
            Funktionen.Lösche_Zeile();
            Console.SetCursorPosition(1, top);
            Console.WriteLine("[ 100% ] Download Complete!"); // hier besser aufgehoben da es ein Callback ist
        }

        public string GetDirectoryListingRegexForUrl(string url)
        {
            return "\\\"([^\"]*)\\\"";
        }

        private static void aktueller_speed(string pathToCheck, double speed, Stopwatch sw, long totalbytes, long aktuellbytes, int progress)
        {
            FileInfo info = new FileInfo(pathToCheck.Replace("%20", " "));
            length = info.Length;
            try
            {
                    speed = (length-length_old)/0.2;
                       
            }
            catch (DivideByZeroException)
            {
                speed = 0;
            }

            string einheit = "Byte/s";

            if (speed > 1024)
            {
                speed = speed / 1024;
                einheit = "KByte/s";
            }

            long total = totalbytes;
            int totalrest=0;
            string totalbyte = "Bytes";
            if (total > 1024)
            {
                totalrest = (int)total % 1024;
                total = total / 1024;
                totalbyte = "KBytes";

                if (total > 1024)
                {
                    totalrest = (int)total % 1024;
                    total = total / 1024;
                    totalbyte = "MBytes";

                    if (total > 1024)
                    {
                        totalrest = (int)total % 1024;                       
                        total = total / 1024;
                        totalbyte = "GBytes";
                    }
                }
            }

            long aktuell = aktuellbytes;
            int aktuellrest=0;
            string aktuellEinheit = "Bytes";

            if (aktuell > 1024)
            {
                aktuellrest = (int)aktuell % 1024;
                aktuell = aktuell / 1024;
                aktuellEinheit = "KBytes";

                if (aktuell > 1024)
                {
                    aktuellrest = (int)aktuell % 1024;
                    aktuell = aktuell / 1024;
                    aktuellEinheit = "MBytes";

                    if (aktuell > 1024)
                    {
                        aktuellrest = (int)aktuell % 1024;  
                        aktuell = aktuell / 1024;
                        aktuellEinheit = "GBytes";
                    }
                }
            }
            int speedint = (int)speed;
            int speed_ds = durchschnitt_speed(speedint);
            if (speed_ds != 0)
            {
                speedint = speed_ds;
            }
            length_old = length;
            statusstring = "[ " + progress + "% ] File Size: " + total + ","+ totalrest +" " + totalbyte + " received: " + aktuell + "." + aktuellrest+ " "  + aktuellEinheit + " Speed: " + speedint + einheit;
        }


        public static int durchschnitt_speed(int speedint)
        {
            for (int i = 0; i < speed_durchschnitt.Length; i++)
            {
                if(speed_durchschnitt[i]==0) //Falls noch keine 5 Downloadspeeds erfasst wurden
                {
                    speed_durchschnitt[i] = speedint;
                    continue;
                }
                else  //läuft im normalfalle durch
                {
                    durchschnitt_verfügbar=true;
                    if(i == speed_durchschnitt.Length-1) //wenn Array voll ist. 
                    {
                        for (int j = 0; j < speed_durchschnitt.Length; j++)
			            {

                            
                            if(j== (speed_durchschnitt.Length-1))
                            {
                                speed_durchschnitt[j]=speedint;
                                continue;
                            }
                            speed_durchschnitt[j] = speed_durchschnitt[j + 1];   //Alle eins vorziehen im Array
			                  
			            }
                    }
                }
                
            }

            if(durchschnitt_verfügbar)
            {
               int speed =0;

                for (int i = 0; i < speed_durchschnitt.Length; i++)
                {
                    speed = speed + speed_durchschnitt[i];
                }

                return (speed / 5);

            }
            return 0;

        }



    }
 
}

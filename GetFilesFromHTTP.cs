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
        string statusstring; 

        // async download variablen
        bool completed = false;
        long length;
        long speed;
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
                                Console.WriteLine(LadeDatei(url, tmp, DIR, offlinepfad));
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
                    Console.SetCursorPosition(0, top);
                    Console.WriteLine("");
                    Console.WriteLine("");
                    Console.SetCursorPosition(0, top);
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
            FileInfo info = new FileInfo(pathToCheck.Replace("%20", " "));
            length = info.Length;
            try
            {
                speed = length / (sw.ElapsedMilliseconds / 1000);
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
            
            long total = e.TotalBytesToReceive;
            string totalbyte = "Bytes";
            if (total > 1024)
            {
                total = total / 1024;
                totalbyte = "KBytes";

                if (total > 1024)
                {
                    total = total / 1024;
                    totalbyte = "MBytes";

                    if (total > 1024)
                    {
                        total = total / 1024;
                        totalbyte = "GBytes";
                    }
                }
            }

            long aktuell = e.BytesReceived;
            string aktuellEinheit = "Bytes";

            if (aktuell > 1024)
            {
                aktuell = aktuell / 1024;
                aktuellEinheit = "KBytes";

                if (aktuell > 1024)
                {
                    aktuell = aktuell / 1024;
                    aktuellEinheit = "MBytes";

                    if (aktuell > 1024)
                    {
                        aktuell = aktuell / 1024;
                        aktuellEinheit = "GBytes";
                    }
                }
            }
            int progress = e.ProgressPercentage;
            statusstring = "File Size: " + total + " " + totalbyte + " received: " + aktuell + " " + aktuellEinheit + " Progress: " + progress + "%, Speed: "+ speed + " " + einheit; 
        }

        void myWebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            completed = true;
            sw.Stop();
            sw.Reset();
            Console.SetCursorPosition(1, top);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.SetCursorPosition(1, top);
            Console.WriteLine("Download fertig! - Mache weiter!");
        }

        public string GetDirectoryListingRegexForUrl(string url)
        {
            return "\\\"([^\"]*)\\\"";
        }
    }
}

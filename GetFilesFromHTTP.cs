using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace Downloader
{
    public class GetFilesFromHTTP
    {
        private Downloader downloader;
        private HttpWebResponse response;
        private StreamReader reader;

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
                myWebClient.DownloadFile(myStringWebResource, save_path + datei);
            }
            catch (WebException e)
            {
                Console.WriteLine("Bitte nochmal versuchen: " + e.ToString());
            }
            Console.ForegroundColor = ConsoleColor.Red;
            return ("\nDownload der Datei abgeschlossen, gespeichert in:  " + pfad);

        }

        public string GetDirectoryListingRegexForUrl(string url)
        {
            return "\\\"([^\"]*)\\\"";
        }
    }
}

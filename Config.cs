using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Downloader
{
    public class Config
    {
        private string configPath = Environment.CurrentDirectory; // ausführpfad
        private ConfigType type;

        public Config(ConfigType type)
        {
            this.type = type;
        }

        public string ReadConfig()
        {
            try
            {
                if (type == ConfigType.DownloadConfig)
                {
                    string path = configPath + "\\config_downloader.cfg";
                    int line = (int)type;
                    string[] content = File.ReadAllLines(path);

                    // clean up
                    path = null;

                    if (!Directory.Exists(content[line]))
                        Directory.CreateDirectory(content[line]);

                    return content[line];
                }
                else
                    return null;
            }
            catch (Exception e)
            {
                Console.Clear();
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public bool CorrectMediaType(string tmp)
        {
            type = ConfigType.MediaTypesConfig;
            try
            {
                // eingabe wie folgt: mp3;mp4;wav;
                string path = configPath + "\\config_downloader.cfg";
                int line = (int)type;
                string[] content = File.ReadAllLines(path);

                string[] tmp_ending = tmp.Split('.');
                string ending =  tmp_ending[tmp_ending.Length - 1];
                ending = ending.Replace("\"", "");

                if (content[line].Contains(ending))
                    return true;
                else
                    return false;
            }
            catch (Exception e)
            {
                Console.Clear();
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public void SetConfigType(ConfigType type)
        {
            this.type = type;
        }
    }

    public enum ConfigType
    {
        DownloadConfig, MediaTypesConfig
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TShockAPI;


namespace Quake
{
    /// <summary>
    /// �����ļ�
    /// </summary>
    public class Config
    {
        // ��ͼ��ѡ����
        public List<string> seeds;

        // ��ͼ��С 0/1/2/3 �Զ�/С/��/��
        public int size = 0;

        // �����ʵ�����
        public bool keepFTW;
        public bool keepNTB;
        public bool keepDST;
        public bool keep2020;
        public bool keep2021;

        // ��ͼ�����ɹ���ʾ
        public string successTips;

        // �Զ�����������С�׼�
        public bool autoCreateRoom;

        // ȫԱ����
        public bool freezePlayer;
        // ����buff
        public List<int> freezeBuffs;


        public static Config Load(string path)
        {
            if (File.Exists(path))
            {
                return JsonConvert.DeserializeObject<Config>(File.ReadAllText(path), new JsonSerializerSettings()
                {
                    Error = (sender, error) => error.ErrorContext.Handled = true
                });
            }
            else
            {
                var c = new Config();
                c.InitDefault();
                File.WriteAllText(path, JsonConvert.SerializeObject(c, Formatting.Indented));
                return c;
            }
        }

        public void InitDefault()
        {
            seeds = new List<string>() {
                "5162020",
                "5162021",
                "for the worthy",
                "not the bees",
                "constant"
            };
            keepFTW = true;
            freezePlayer = false;
            freezeBuffs = new List<int>() { 156, 163 };
            successTips = $"[i:50]���� /spawn �س�����\n[i:3199]���� /home �ؼ�";
        }
    }



    // ----

    public class ConfigHelper
    {
        private static Config _config;
        public static readonly string SaveDir = Path.Combine(TShock.SavePath, "Quake");
        private static readonly string SaveFile = Path.Combine(SaveDir, "config.json");

        public static void Init()
        {
            if (!Directory.Exists(SaveDir)) Directory.CreateDirectory(SaveDir);
            _config = Config.Load(SaveFile);
        }

        public static void Reload()
        {
            _config = Config.Load(SaveFile);
        }

        public static void Save()
        {
            File.WriteAllText(SaveFile, JsonConvert.SerializeObject(_config, Formatting.Indented));
        }

        public static Config Config { get { return _config; } }

        public static string GetRandomSeed
        {
            get
            {
                Random rand = new Random();
                if (rand.Next(3) == 0)
                {
                    return "0";
                }
                else
                {
                    int index = rand.Next(Config.seeds.Count);
                    string seed = Config.seeds[index];
                    Console.WriteLine($"�������ļ�����ѡ���� {seed}");
                    return seed;
                }
            }
        }

        public static void Dispose() { _config = null; }
    }

}
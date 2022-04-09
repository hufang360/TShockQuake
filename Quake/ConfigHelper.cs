using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TShockAPI;


namespace Quake
{
    /// <summary>
    /// 配置文件
    /// </summary>
    public class Config
    {
        // 地图可选种子
        public List<string> seeds;

        // 地图大小 0/1/2/3 自动/小/中/大
        public int size = 0;

        // 保留彩蛋特性
        public bool keepFTW;
        public bool keepNTB;
        public bool keepDST;
        public bool keep2020;
        public bool keep2021;

        // 地图创建成功提示
        public string successTips;

        // 自动创建出生点小套间
        public bool autoCreateRoom;

        // 全员禁足
        public bool freezePlayer;
        // 禁足buff
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
            successTips = $"[i:50]输入 /spawn 回出生点\n[i:3199]输入 /home 回家";
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
                    Console.WriteLine($"从配置文件里挑选种子 {seed}");
                    return seed;
                }
            }
        }

        public static void Dispose() { _config = null; }
    }

}
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;

namespace Quake
{
    public class Config
    {
        // 地震延迟（秒）
        public int quakeDelay = 30;

        // 地图可选种子
        public List<string> seeds;

        // 地图大小 0/1/2/3 自动/小/中/大
        public int size = 0;

        // 保留彩蛋特性
        public bool keepFTW = true;
        public bool keepNTB;
        public bool keepDST;
        public bool keep2020;
        public bool keep2021;

        // 创建世界时踢出玩家
        public bool kickGen;

        // 地图创建成功提示
        public string successTips;

        // 自动创建出生点小套间
        public bool autoCreateRoom = true;

        // 自动创建地狱直通车（需要击败骷髅王）
        public bool autoCreateHellevator = true;

        // 搬家区域，x和y是相对于出生点
        [JsonConverter(typeof(RectangleConverter))]
        public Rectangle area = new Rectangle(-61, -60, 122, 68);

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
            successTips = $"[i:50]输入 /spawn 回出生点\n[i:3199]输入 /home 回家";
        }

        // 实际的搬家区域
        public Rectangle GetArea() { return new Rectangle(Main.spawnTileX + area.X, Main.spawnTileY + area.Y, area.Width, area.Height); }
    }


    public class ConfigHelper
    {
        private static Config _config;
        public static string SaveFile;

        public static void Init() { Reload(); }
        public static void Reload() { _config = Config.Load(SaveFile); }
        public static void Save() { File.WriteAllText(SaveFile, JsonConvert.SerializeObject(_config, Formatting.Indented)); }
        public static Config Con { get { return _config; } }



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
                    int index = rand.Next(Con.seeds.Count);
                    string seed = Con.seeds[index];
                    utils.Log($"从配置文件里挑选种子 {seed}");
                    return seed;
                }
            }
        }
    }


    public class RectangleConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Rectangle rect = (Rectangle)value;
            JObject.FromObject(new { rect.X, rect.Y, rect.Width, rect.Height }).WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var o = JObject.Load(reader);

            var x = GetTokenValue(o, "x") ?? 0;
            var y = GetTokenValue(o, "y") ?? 0;
            var width = GetTokenValue(o, "width") ?? 0;
            var height = GetTokenValue(o, "height") ?? 0;

            return new Rectangle(x, y, width, height);
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        private static int? GetTokenValue(JObject o, string tokenName)
        {
            JToken t;
            return o.TryGetValue(tokenName, StringComparison.InvariantCultureIgnoreCase, out t) ? (int)t : (int?)null;
        }
    }
}
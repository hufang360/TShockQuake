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
        // �����ӳ٣��룩
        public int quakeDelay = 30;

        // ��ͼ��ѡ����
        public List<string> seeds;

        // ��ͼ��С 0/1/2/3 �Զ�/С/��/��
        public int size = 0;

        // �����ʵ�����
        public bool keepFTW = true;
        public bool keepNTB;
        public bool keepDST;
        public bool keep2020;
        public bool keep2021;

        // ��������ʱ�߳����
        public bool kickGen;

        // ��ͼ�����ɹ���ʾ
        public string successTips;

        // �Զ�����������С�׼�
        public bool autoCreateRoom = true;

        // �Զ���������ֱͨ������Ҫ������������
        public bool autoCreateHellevator = true;

        // �������x��y������ڳ�����
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
            successTips = $"[i:50]���� /spawn �س�����\n[i:3199]���� /home �ؼ�";
        }

        // ʵ�ʵİ������
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
                    utils.Log($"�������ļ�����ѡ���� {seed}");
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
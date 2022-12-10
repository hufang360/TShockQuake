using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Tile_Entities;
using TShockAPI;


namespace Quake
{
    public class WorldHelper
    {
        private static WorldInfo _config;
        public static string SaveFile;
        public static void Init() { Reload(); }
        public static void Reload() { _config = WorldInfo.Load(SaveFile); }
        public static void Save() { WorldInfo.Save(SaveFile, _config); }
        public static WorldInfo Rec { get { return _config; } }
        public static Config Con { get { return ConfigHelper.Con; } }

        public static string SaveDir;
        // 写入 chest.txt
        public static void SaveChestTxt(string saveStr) { File.WriteAllText(Path.Combine(SaveDir, "chest.txt"), saveStr); }


        public static bool LunarApocalypseIsUp
        {
            get { return Rec.lunarApocalypseIsUp; }
            set { Rec.lunarApocalypseIsUp = value; }
        }

        #region 备份世界纪录
        public static void Backup()
        {
            // WorldGen.cs clearWorld
            Rec.hardMode = Main.hardMode;
            Rec.downedBoss1 = NPC.downedBoss1;
            Rec.downedBoss2 = NPC.downedBoss2;
            Rec.downedBoss3 = NPC.downedBoss3;
            Rec.downedQueenBee = NPC.downedQueenBee;
            Rec.downedSlimeKing = NPC.downedSlimeKing;
            Rec.downedMechBossAny = NPC.downedMechBossAny;
            Rec.downedMechBoss1 = NPC.downedMechBoss1;
            Rec.downedMechBoss2 = NPC.downedMechBoss2;
            Rec.downedMechBoss3 = NPC.downedMechBoss3;
            Rec.downedFishron = NPC.downedFishron;
            Rec.downedAncientCultist = NPC.downedAncientCultist;
            Rec.downedMoonlord = NPC.downedMoonlord;
            Rec.downedHalloweenKing = NPC.downedHalloweenKing;
            Rec.downedHalloweenTree = NPC.downedHalloweenTree;
            Rec.downedChristmasIceQueen = NPC.downedChristmasIceQueen;
            Rec.downedChristmasSantank = NPC.downedChristmasSantank;
            Rec.downedChristmasTree = NPC.downedChristmasTree;
            Rec.downedPlantBoss = NPC.downedPlantBoss;
            Rec.downedGolemBoss = NPC.downedGolemBoss;
            Rec.downedEmpressOfLight = NPC.downedEmpressOfLight;
            Rec.downedQueenSlime = NPC.downedQueenSlime;
            Rec.downedDeerclops = NPC.downedDeerclops;
            Rec.combatBookWasUsed = NPC.combatBookWasUsed;
            Rec.savedStylist = NPC.savedStylist;
            Rec.savedGoblin = NPC.savedGoblin;
            Rec.savedWizard = NPC.savedWizard;
            Rec.savedMech = NPC.savedMech;
            Rec.savedTaxCollector = NPC.savedTaxCollector;
            Rec.savedAngler = NPC.savedAngler;
            Rec.savedBartender = NPC.savedBartender;
            Rec.savedGolfer = NPC.savedGolfer;
            Rec.boughtCat = NPC.boughtCat;
            Rec.boughtDog = NPC.boughtDog;
            Rec.boughtBunny = NPC.boughtBunny;
            Rec.downedGoblins = NPC.downedGoblins;
            Rec.downedClown = NPC.downedClown;
            Rec.downedFrost = NPC.downedFrost;
            Rec.downedPirates = NPC.downedPirates;
            Rec.downedMartians = NPC.downedMartians;
            Rec.downedTowerSolar = NPC.downedTowerSolar;
            Rec.downedTowerVortex = NPC.downedTowerVortex;
            Rec.downedTowerNebula = NPC.downedTowerNebula;
            Rec.downedTowerStardust = NPC.downedTowerStardust;

            Rec.worldID = Main.worldID;


            // 怪物图鉴
            Rec.kills.Clear();
            Rec.sights.Clear();
            Rec.chats.Clear();
            foreach (string key in Main.BestiaryTracker.Kills._killCountsByNpcId.Keys)
            {
                Rec.kills.Add(key, Main.BestiaryTracker.Kills._killCountsByNpcId[key]);
            }
            foreach (string key in Main.BestiaryTracker.Sights._wasNearPlayer)
            {
                Rec.sights.Add(key);
            }
            foreach (string key in Main.BestiaryTracker.Chats._chattedWithPlayer)
            {
                Rec.chats.Add(key);
            }

            // NPC
            Rec.npcs = NPCHelper.ListedAliveTownNPC();
            // 报告搬家情况
            Report();
            // 保存到配置
            Save();
        }

        /// <summary>
        /// 报告搬家情况
        /// </summary>
        private static void Report()
        {
            // 箱子
            ListedChest(true);

            // 晶塔
            ListedPylon(true);

            // 额外的物品
            ListedExtra(true);
        }

        public static List<string> ListedChest(bool needRecord = false)
        {
            if (needRecord) Rec.chests.Clear();
            List<string> names = new();
            Rectangle area = Con.GetArea();
            for (int i = 0; i < Main.chest.Length; i++)
            {
                Chest chest = Main.chest[i];
                if (chest == null || string.IsNullOrEmpty(chest.name) || !area.Contains(chest.x, chest.y)) continue;

                int count = 0;
                ChestData data = new(chest.name);
                for (int k = 0; k < chest.item.Length; k++)
                {
                    if (chest.item[k] == null || !chest.item[k].active) continue;
                    if (needRecord) data.AddItem(k, chest.item[k]);
                    count++;
                }

                if (count > 0)
                {
                    names.Add(chest.name);
                    if (needRecord) Rec.chests.Add(data);
                }
            }

            if (needRecord && names.Count > 0)
            {
                utils.Log($"箱子 {names.Count}个: " + string.Join("\n", PaginationTools.BuildLinesFromTerms(names)));
            }
            return names;
        }
        public static List<string> ListedPylon(bool needRecord = false)
        {
            List<int> ids = new();
            foreach (TeleportPylonInfo pylon in Main.PylonSystem.Pylons)
            {
                switch (pylon.TypeOfPylon)
                {
                    case TeleportPylonType.SurfacePurity: ids.Add(4876); break;
                    case TeleportPylonType.Jungle: ids.Add(4875); break;
                    case TeleportPylonType.Hallow: ids.Add(4916); break;
                    case TeleportPylonType.Underground: ids.Add(4917); break;
                    case TeleportPylonType.Beach: ids.Add(4918); break;
                    case TeleportPylonType.Desert: ids.Add(4919); break;
                    case TeleportPylonType.Snow: ids.Add(4920); break;
                    case TeleportPylonType.GlowingMushroom: ids.Add(4921); break;
                    case TeleportPylonType.Victory: ids.Add(4951); break;
                }
            }

            if (needRecord)
            {
                List<string> logs = new();
                Rec.pylons.Clear();
                if (ids.Count > 0)
                {
                    ChestData data = new("晶塔");
                    for (int k = 0; k < ids.Count; k++)
                    {
                        data.AddItem(k, ids[k], 1, 0);
                        logs.Add(utils.GetItemName(ids[k]));
                    }
                    Rec.pylons.Add(data);
                    if (logs.Count > 0)
                    {
                        logs[0] = $"晶塔 {logs.Count}个: " + logs[0];
                        utils.Log(string.Join("\n", PaginationTools.BuildLinesFromTerms(logs)));
                    }
                }
            }

            List<string> texts = new();
            foreach (var id in ids)
            {
                texts.Add($"[i:{id}]");
            }

            return texts;
        }
        public static List<string> ListedExtra(bool needRecord = false)
        {
            TileHelper.ResetSkip();

            Rectangle area = Con.GetArea();
            Dictionary<int, int> found = new();

            void addToDict(int _id, int _stack = 1)
            {
                if (found.Keys.Contains(_id))
                    found[_id] += _stack;
                else
                    found.Add(_id, _stack);
            }

            for (int k = area.Bottom; k > area.Y; k--)
            {
                for (int i = area.X; i < area.Right; i++)
                {
                    int id;
                    ITile tile = Main.tile[i, k];
                    if (tile.active())
                    {
                        id = TileHelper.GetItem(i, k);
                        int num;
                        if (id != 0)
                        {
                            addToDict(id);

                            // 找出物品框内的物品
                            if (tile.type == 395)
                            {
                                num = TEItemFrame.Find(i, k);
                                if (num != -1)
                                {
                                    num = ((TEItemFrame)TileEntity.ByID[num]).item.netID;
                                    addToDict(num);
                                }
                            }

                            // 找出武器架内的物品
                            else if (tile.type == 471)
                            {
                                num = TEWeaponsRack.Find(i, k);
                                if (num != -1)
                                {
                                    num = ((TEWeaponsRack)TileEntity.ByID[num]).item.netID;
                                    addToDict(num);
                                }
                            }
                        }

                        // 收集草药、破坏人工方块
                        else if (TileHelper.CanGetKillItemDrop(tile.type))
                        {
                            WorldGen.KillTile_GetItemDrops(i, k, tile, out int dropItem, out int dropItemStack, out int secondaryItem, out int secondaryItemStack);
                            if (dropItem != 0) addToDict(dropItem, dropItemStack);
                            if (secondaryItem != 0) addToDict(secondaryItem, secondaryItemStack);
                        }
                    }


                    // 墙
                    if (tile.wall != 0)
                    {
                        id = TileHelper.GetWallItem(tile.wall);
                        if (id != 0) addToDict(id);
                    }
                }
            }

            List<Item> items = new();
            foreach (int id in found.Keys)
            {
                Item item = new();
                item.SetDefaults(id);

                int stack = found[id];
                if (stack <= item.maxStack)
                {
                    item.stack = stack;
                    items.Add(item);
                }
                else
                {
                    item.stack = item.maxStack;
                    items.Add(item);

                    stack -= item.maxStack;
                    int pcs = stack / item.maxStack;
                    for (int i = 0; i < pcs; i++)
                    {
                        item = new Item();
                        item.SetDefaults(id);
                        item.stack = item.maxStack;
                    }

                    item = new Item();
                    item.SetDefaults(id);
                    item.stack = stack % item.maxStack;
                }
            }


            if (needRecord) Rec.extras.Clear();
            List<string> texts = new();
            List<string> logs = new();
            int count = 0;
            int index = 0;
            ChestData data = new("其它");
            Rec.extras.Add(data);
            foreach (Item item in items)
            {
                if (item.stack > 1)
                    texts.Add($"[i/s{item.stack}:{item.netID}]");
                else
                    texts.Add($"[i:{item.netID}]");

                if (needRecord)
                {
                    logs.Add($"{item.Name}x{item.stack}");
                    if (count != 0 && count % 40 == 0)
                    {
                        data = new ChestData($"其它{1 + count / 40}");
                        Rec.extras.Add(data);
                        index = 0;
                    }
                    data.AddItem(index, item);
                    count++;
                    index++;
                }
            }

            if (logs.Count > 0)
            {
                logs[0] = $"杂物 {logs.Count}个: " + logs[0];
                utils.Log(string.Join("\n", PaginationTools.BuildLinesFromTerms(logs)));
            }
            return texts;
        }
        #endregion

        #region 恢复世界纪录
        public static void Recover()
        {
            // Main.hardMode = Con.hardMode; //之后会执行进入肉后操作，此时不应还原此属性
            NPC.downedBoss1 = Rec.downedBoss1;
            NPC.downedBoss2 = Rec.downedBoss2;
            NPC.downedBoss3 = Rec.downedBoss3;
            NPC.downedQueenBee = Rec.downedQueenBee;
            NPC.downedSlimeKing = Rec.downedSlimeKing;
            NPC.downedMechBossAny = Rec.downedMechBossAny;
            NPC.downedMechBoss1 = Rec.downedMechBoss1;
            NPC.downedMechBoss2 = Rec.downedMechBoss2;
            NPC.downedMechBoss3 = Rec.downedMechBoss3;
            NPC.downedFishron = Rec.downedFishron;
            NPC.downedAncientCultist = Rec.downedAncientCultist;
            NPC.downedMoonlord = Rec.downedMoonlord;
            NPC.downedHalloweenKing = Rec.downedHalloweenKing;
            NPC.downedHalloweenTree = Rec.downedHalloweenTree;
            NPC.downedChristmasIceQueen = Rec.downedChristmasIceQueen;
            NPC.downedChristmasSantank = Rec.downedChristmasSantank;
            NPC.downedChristmasTree = Rec.downedChristmasTree;
            NPC.downedPlantBoss = Rec.downedPlantBoss;
            NPC.downedGolemBoss = Rec.downedGolemBoss;
            NPC.downedEmpressOfLight = Rec.downedEmpressOfLight;
            NPC.downedQueenSlime = Rec.downedQueenSlime;
            NPC.downedDeerclops = Rec.downedDeerclops;
            NPC.combatBookWasUsed = Rec.combatBookWasUsed;
            NPC.savedStylist = Rec.savedStylist;
            NPC.savedGoblin = Rec.savedGoblin;
            NPC.savedWizard = Rec.savedWizard;
            NPC.savedMech = Rec.savedMech;
            NPC.savedTaxCollector = Rec.savedTaxCollector;
            NPC.savedAngler = Rec.savedAngler;
            NPC.savedBartender = Rec.savedBartender;
            NPC.savedGolfer = Rec.savedGolfer;
            NPC.boughtCat = Rec.boughtCat;
            NPC.boughtDog = Rec.boughtDog;
            NPC.boughtBunny = Rec.boughtBunny;
            NPC.downedGoblins = Rec.downedGoblins;
            NPC.downedClown = Rec.downedClown;
            NPC.downedFrost = Rec.downedFrost;
            NPC.downedPirates = Rec.downedPirates;
            NPC.downedMartians = Rec.downedMartians;
            NPC.downedTowerSolar = Rec.downedTowerSolar;
            NPC.downedTowerVortex = Rec.downedTowerVortex;
            NPC.downedTowerNebula = Rec.downedTowerNebula;
            NPC.downedTowerStardust = Rec.downedTowerStardust;

            Main.worldID = Rec.worldID;


            // 怪物图鉴
            foreach (string key in Rec.kills.Keys)
            {
                if (Main.BestiaryTracker.Kills._killCountsByNpcId.ContainsKey(key))
                    Main.BestiaryTracker.Kills._killCountsByNpcId[key] = Rec.kills[key];
                else
                    Main.BestiaryTracker.Kills._killCountsByNpcId.Add(key, Rec.kills[key]);
            }
            foreach (string key in Rec.sights)
            {
                if (!Main.BestiaryTracker.Sights._wasNearPlayer.Contains(key))
                    Main.BestiaryTracker.Sights._wasNearPlayer.Add(key);
            }
            foreach (string key in Rec.chats)
            {
                if (!Main.BestiaryTracker.Chats._chattedWithPlayer.Contains(key))
                    Main.BestiaryTracker.Chats._chattedWithPlayer.Add(key);
            }
        }
        #endregion

    }



    #region 世界属性
    public class WorldInfo
    {
        #region downed
        public bool downedSlimeKing = false;
        public bool downedBoss1 = false;
        public bool downedBoss2 = false;
        public bool downedBoss3 = false;
        public bool downedQueenBee = false;
        public bool downedDeerclops = false;
        public bool hardMode = false;
        public bool downedMechBoss1 = false;
        public bool downedMechBoss2 = false;
        public bool downedMechBoss3 = false;
        public bool downedMechBossAny = false;
        public bool downedPlantBoss = false;
        public bool downedGolemBoss = false;
        public bool downedQueenSlime = false;
        public bool downedFishron = false;
        public bool downedEmpressOfLight = false;
        public bool downedAncientCultist = false;
        public bool downedTowerSolar = false;
        public bool downedTowerVortex = false;
        public bool downedTowerNebula = false;
        public bool downedTowerStardust = false;
        public bool downedMoonlord = false;
        public bool downedGoblins = false;
        public bool downedClown = false;
        public bool downedFrost = false;
        public bool downedPirates = false;
        public bool downedMartians = false;
        public bool downedHalloweenKing = false;
        public bool downedHalloweenTree = false;
        public bool downedChristmasIceQueen = false;
        public bool downedChristmasSantank = false;
        public bool downedChristmasTree = false;
        public bool combatBookWasUsed = false;
        public bool savedStylist = false;
        public bool savedGoblin = false;
        public bool savedWizard = false;
        public bool savedMech = false;
        public bool savedTaxCollector = false;
        public bool savedAngler = false;
        public bool savedBartender = false;
        public bool savedGolfer = false;
        public bool boughtCat = false;
        public bool boughtDog = false;
        public bool boughtBunny = false;
        #endregion
        public bool lunarApocalypseIsUp = false;

        public int worldID = 0;

        public List<ChestData> chests = new();
        public List<ChestData> pylons = new();
        public List<ChestData> extras = new();
        public List<int> npcs = new();
        public Dictionary<string, int> kills = new();
        public List<string> sights = new();
        public List<string> chats = new();

        public static WorldInfo Load(string path)
        {
            if (File.Exists(path))
            {
                return JsonConvert.DeserializeObject<WorldInfo>(File.ReadAllText(path), new JsonSerializerSettings()
                {
                    Error = (sender, error) => error.ErrorContext.Handled = true
                });
            }
            else return Save(path, new WorldInfo());
        }
        public static WorldInfo Save(string path, WorldInfo info)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(info, Formatting.Indented));
            return info;
        }
    }

    public class ChestData
    {
        [JsonProperty("name")]
        public string name;

        [JsonProperty("item")]
        public string itemstr = "";

        [JsonIgnore]
        public NetItem[] items = new NetItem[40];

        [JsonIgnore]
        public bool splited = false;

        public ChestData(string _name) { name = _name; }

        public void AddItem(int index, Item item)
        {
            items[index] = new NetItem(item.netID, item.stack, item.prefix);
            itemstr = string.Join("~", items);
        }

        public void AddItem(int index, int id, int stack, byte prefix)
        {
            items[index] = new NetItem(id, stack, prefix);
            itemstr = string.Join("~", items);
        }

        public NetItem GetItem(int index)
        {
            if (!splited && !string.IsNullOrEmpty(itemstr))
            {
                items = itemstr.Split('~').Select(NetItem.Parse).ToArray();
                splited = true;
            }

            return items[index];
        }
    }
    #endregion

}
using System;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;


namespace Quake
{
    public class ReGenHelper
    {
        public static bool isTaskRunning = false;

        private static int CoolDown = -1;

        private static int secondLast;

        private static int townNPCTotal = 1;

        private static Config Config { get { return ConfigHelper.Config; } }


        public static void Init()
        {
        }


        public static void Reset()
        {
            isTaskRunning = false;
            CoolDown = -1;
            townNPCTotal = 1;
            secondLast = 0;
        }

        public static void Trigger(int second = 30)
        {
            CoolDown = second * 60 + 1; //30秒后执行
            BackupHelper.Backup();
        }

        public static void OnGameUpdate()
        {
            if (CoolDown == -1)
                return;

            //Console.WriteLine($"{CoolDown}");
            if (CoolDown > 0)
                CoolDown--;

            if (CoolDown == 0)
            {
                CoolDown = -1;
                GenWorld(ConfigHelper.GetRandomSeed);
            }
            else if (CoolDown == 30 * 60)
                TSPlayer.All.SendInfoMessage($"[i:{GetSpecailItemID}]地震还有 30秒 抵达，请及时抢救物资");
            else if (CoolDown == 15 * 60)
                TSPlayer.All.SendInfoMessage($"[i:{GetSpecailItemID}]地震还有 15秒 抵达");
            else if (CoolDown == 10 * 60)
                TSPlayer.All.SendInfoMessage($"[i:{GetSpecailItemID}]地震还有 10秒 抵达！");
            else if (CoolDown == 5 * 60)
                TSPlayer.All.SendErrorMessage($"[i:{GetSpecailItemID}]地震还有 5秒 抵达！！");
        }

        // 世界吞噬怪面具 | 毁灭者面具
        private static int GetSpecailItemID { get { return Main.hardMode ? 2113 : 2111; } }




        #region GenWorld
        // 参考：https://github.com/Illuminousity/WorldRefill/blob/master/WorldRefill/WorldRefill.cs#L997
        public static async void GenWorld(string seedStr = "", int size = 0, int evil = -1, string eggStr = "")
        {
            if (isTaskRunning) return;

            // 备份
            WorldHelper.Backup();
            TSPlayer.All.SendErrorMessage("[i:556]世界正在解体~");
            secondLast = utils.GetUnixTimestamp;
            townNPCTotal = NPCHelper.CountTownNPC(false);
            if (Config.freezePlayer) FreezePlayer();

            // 设置创建参数
            // 种子
            ProcessSeeds(seedStr);
            ProcessEggSeeds(eggStr);
            seedStr = seedStr.ToLowerInvariant();
            if (string.IsNullOrEmpty(seedStr) || seedStr == "0")
                seedStr = "random";
            if (Main.ActiveWorldFileData.Seed == 5162020)
                seedStr = "5162020";

            if (seedStr == "random")
                Main.ActiveWorldFileData.SetSeedToRandom();
            else
                Main.ActiveWorldFileData.SetSeed(seedStr);

            // 大小 腐化
            if (Config.size > 0 && Config.size < 4)
                size = Config.size;
            int tilesX = 0;
            int tilesY = 0;
            if (size == 1)
            {
                tilesX = 4200;
                tilesY = 1200;
            }
            else if (size == 2)
            {
                tilesX = 6400;
                tilesY = 1800;
            }
            else if (size == 3)
            {
                tilesX = 8400;
                tilesY = 2400;
            }
            if (tilesX > 0)
            {
                Main.maxTilesX = tilesX;
                Main.maxTilesY = tilesY;
                Main.ActiveWorldFileData.SetWorldSize(tilesX, tilesY);
            }
            WorldGen.WorldGenParam_Evil = evil;

            // 开始创建
            TSPlayer.All.SendErrorMessage($"[i:3061]世界正在重建（{WorldGen.currentWorldSeed}）");
            await AsyncGenerateWorld(Main.ActiveWorldFileData.Seed);
        }
        #endregion

        #region 处理种子
        /// <summary>
        /// 处理秘密世界种子
        /// </summary>
        /// <param name="seed"></param>
        private static void ProcessSeeds(string seed)
        {
            // UIWorldCreation.ProcessSpecialWorldSeeds(seedStr);
            WorldGen.notTheBees = false;
            WorldGen.getGoodWorldGen = false;
            WorldGen.tenthAnniversaryWorldGen = false;
            WorldGen.dontStarveWorldGen = false;
            ToggleSpecialWorld(seed.ToLowerInvariant());
        }

        /// <summary>
        /// 处理彩蛋
        /// </summary>
        /// <param name="seedstr">例如：2020,2021,ftw</param>
        private static void ProcessEggSeeds(string seedstr)
        {
            string[] seeds = seedstr.ToLowerInvariant().Split(',');
            foreach (string newseed in seeds)
            {
                ToggleSpecialWorld(newseed);
            }
        }
        /// <summary>
        /// 开关秘密世界（创建器的属性）
        /// </summary>
        /// <param name="seed"></param>
        private static void ToggleSpecialWorld(string seed)
        {
            switch (seed)
            {
                case "2020":
                case "516":
                case "5162020":
                case "05162020":
                    Main.ActiveWorldFileData._seed = 5162020;
                    break;

                case "2021":
                case "5162011":
                case "5162021":
                case "05162011":
                case "05162021":
                case "celebrationmk10":
                    WorldGen.tenthAnniversaryWorldGen = true;
                    break;

                case "ntb":
                case "not the bees":
                case "not the bees!":
                    WorldGen.notTheBees = true;
                    break;

                case "ftw":
                case "for the worthy":
                    WorldGen.getGoodWorldGen = true;
                    break;

                case "dst":
                case "constant":
                case "theconstant":
                case "the constant":
                case "eye4aneye":
                case "eyeforaneye":
                    WorldGen.dontStarveWorldGen = true;
                    break;

                case "superegg":
                    Main.ActiveWorldFileData._seed = 5162020;

                    WorldGen.notTheBees = true;
                    WorldGen.getGoodWorldGen = true;
                    WorldGen.tenthAnniversaryWorldGen = true;
                    WorldGen.dontStarveWorldGen = true;
                    break;
            }
        }
        #endregion

        #region  AsyncGenerateWorld
        private static Task AsyncGenerateWorld(int seed)
        {
            isTaskRunning = true;
            ClearSomething();
            WorldGen.clearWorld();
            return Task.Run(() => GenerateWorld(seed)).ContinueWith((d) => GenWorldAfter());
        }
        private static void GenerateWorld(int seed)
        {
            WorldGen.GenerateWorld(seed);
            // 自动创建房间
            if (Config.autoCreateRoom)
                GenRooms(Main.spawnTileX, Main.spawnTileY - 4, townNPCTotal, true, true);
        }
        private static void ClearSomething()
        {
            for (int i = 0; i < Main.maxItems; i++)
            {
                if (Main.item[i].active)
                {
                    Main.item[i].active = false;
                    TSPlayer.All.SendData(PacketTypes.ItemDrop, "", i);
                }
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active)
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }
            }
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active)
                {
                    Main.projectile[i].active = false;
                    Main.projectile[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", i);
                }
            }
        }

        /// <summary>
        /// 创建世界后
        /// </summary>
        private static void GenWorldAfter()
        {
            // 设置彩蛋属性
            if (Config.keepFTW) Main.getGoodWorld = true;
            if (Config.keepNTB) Main.notTheBeesWorld = true;
            if (Config.keepDST) Main.dontStarveWorld = true;
            if (Config.keep2020) Main.drunkWorld = true;
            if (Config.keep2021) Main.tenthAnniversaryWorld = true;
            // 还原上次的记录
            WorldHelper.Recover();

            int second = utils.GetUnixTimestamp - secondLast;
            string text = $"[i:3061]世界重建完成 （用时 {second}s, {WorldGen.currentWorldSeed}）；-）";
            if (!string.IsNullOrEmpty(Config.successTips))
                text += $"\n{Config.successTips}";

            TSPlayer.All.SendSuccessMessage(text);
            Console.WriteLine(text);


            // 重新生成月亮事件
            if (WorldHelper.LunarApocalypseIsUp)
            {
                NPC.downedAncientCultist = true;
                NPC.LunarApocalypseIsUp = true;
                TSPlayer.All.SendData(PacketTypes.WorldInfo);
                WorldGen.TriggerLunarApocalypse();
            }
            WorldHelper.Reset();

            // 传送到出生点
            foreach (TSPlayer plr in TShock.Players)
            {
                if (plr != null && Config.freezePlayer)
                    utils.ResetBuff(plr.Index);

                if (plr != null && Main.tile[plr.TileX, plr.TileY].active())
                    utils.PlayerGoHome(plr);
            }
            FinishGen();
            InformPlayers();

            // 复活NPC
            NPCHelper.Relive();
        }
        #endregion

        /// <summary>
        /// 全员禁足
        /// </summary>
        /// <param name="second">禁足时长</param>
        public static void FreezePlayer(int second = 300)
        {
            // Max possible buff duration as of Terraria 1.4.2.3 is 35791393 seconds (415 days).
            //var timeLimit = (int.MaxValue / 60) - 1;
            //if (second < 0 || second > timeLimit)
            //    second = timeLimit;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (!Main.player[i].active)
                    continue;

                TSPlayer op = TShock.Players[i];
                // 传回出生点
                op.Teleport(Main.spawnTileX * 16, (Main.spawnTileY * 16) - 48);
                // 设置buff
                foreach (int id in Config.freezeBuffs)
                {
                    op.SetBuff(id, second * 60);
                }
            }
        }


        #region 创建完成后
        /// <summary>
        /// 完成创建
        /// </summary>
        private static void FinishGen()
        {
            isTaskRunning = false;
            foreach (TSPlayer player in TShock.Players)
            {
                if (player != null && (player.Active))
                {
                    NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(player.TPlayer.position, 1, 0, 10, -16));
                }
            }
            TShock.Utils.SaveWorld();
        }
        // 更新物块
        private static void InformPlayers()
        {
            foreach (TSPlayer person in TShock.Players)
            {
                if ((person != null) && (person.Active))
                {
                    for (int i = 0; i < 255; i++)
                    {
                        for (int j = 0; j < Main.maxSectionsX; j++)
                        {
                            for (int k = 0; k < Main.maxSectionsY; k++)
                            {
                                Netplay.Clients[i].TileSections[j, k] = false;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region 生成小房间
        public static Task AsyncGenRoom(int posX, int posY, int total = 1, bool isRight = true, bool needCenter = false)
        {
            isTaskRunning = true;
            return Task.Run(() => GenRooms(posX, posY, total, isRight, needCenter)).ContinueWith((d) => FinishGen());
        }

        public static void GenRooms(int posX, int posY, int total, bool isRight = true, bool needCenter = false)
        {
            int w = 5;
            int row = 6;
            int roomWidth = 1 + Math.Min(row, total) * w;

            int fixPosX = needCenter ? posX - (roomWidth / 2) : posX;
            int startX;
            int startY;
            Console.WriteLine($"房间数:{total} posX:{posX} posY:{posY} startX:{fixPosX}");
            for (int i = 0; i < total; i++)
            {
                startX = fixPosX + i % row * w;
                startY = posY - (int)Math.Floor((float)(i / row)) * 10;
                GenRoom(startX, startY, isRight);
            }
        }

        public static void GenRoom(int posX, int posY, bool isRight = true)
        {
            RoomTheme th = RoomTheme.GetGlass();

            ushort tile = th.tile;
            ushort wall = th.wall;
            TileInfo platform = th.platform;
            TileInfo chair = th.chair;
            TileInfo bench = th.bench;
            TileInfo torch = th.torch;
            TileInfo chest = th.chest;

            int Xstart = posX;
            int Ystart = posY;
            int Width = 6;
            int height = 10;

            if (!isRight)
                Xstart += 2;

            Parallel.For(Xstart, Xstart + Width, (cx) =>
            {
                Parallel.For(Ystart - height, Ystart, (cy) =>
                {
                    // 清空区域
                    Main.tile[cx, cy].ClearEverything();

                    // 墙
                    if ((cx > Xstart) && (cy < Ystart - 5) && (cx < Xstart + Width - 1) && (cy > Ystart - height))
                    {
                        Main.tile[cx, cy].wall = wall;
                    }


                    if ((cx == Xstart && cy > Ystart - 5)
                    || (cx == Xstart + Width - 1 && cy > Ystart - 5)
                    || (cy == Ystart - 1))
                    {
                        // 平台
                        WorldGen.PlaceTile(cx, cy, platform.type, false, true, -1, platform.style);
                    }

                    else if ((cx == Xstart) || (cx == Xstart + Width - 1)
                    || (cy == Ystart - height)
                    || (cy == Ystart - 5))
                    {
                        // 方块
                        Main.tile[cx, cy].type = tile;
                        Main.tile[cx, cy].active(true);
                        Main.tile[cx, cy].slope(0);
                        Main.tile[cx, cy].halfBrick(false);
                    }
                });
            });

            if (isRight)
            {
                // 椅子
                WorldGen.PlaceTile(Xstart + 1, Ystart - 6, chair.type, false, true, 0, chair.style);
                Main.tile[Xstart + 1, posY - 6].frameX += 18;
                Main.tile[Xstart + 1, posY - 7].frameX += 18;

                // 工作台
                WorldGen.PlaceTile(Xstart + 2, Ystart - 6, bench.type, false, true, -1, bench.style);

                // 火把
                WorldGen.PlaceTile(Xstart + 4, Ystart - 5, torch.type, false, true, -1, torch.style);

            }
            else
            {
                WorldGen.PlaceTile(Xstart + 4, Ystart - 6, chair.type, false, true, 0, chair.style);
                WorldGen.PlaceTile(Xstart + 2, Ystart - 6, bench.type, false, true, -1, bench.style);
                WorldGen.PlaceTile(Xstart + 1, Ystart - 5, torch.type, false, true, -1, torch.style);
            }

            // 箱子
            WorldGen.PlaceChest(Xstart + 1, Ystart - 2, 21, false, chest.style);
            WorldGen.PlaceChest(Xstart + 3, Ystart - 2, 21, false, chest.style);
            InformPlayers();
        }

        #endregion

        //public static List<ushort> trees = new List<ushort>{
        //    TileID.Trees,
        //    TileID.TreeAmber,
        //    TileID.TreeAmethyst,
        //    TileID.TreeDiamond,
        //    TileID.TreeEmerald,
        //    TileID.TreeRuby,
        //    TileID.TreeSapphire,
        //    TileID.TreeTopaz,
        //    TileID.MushroomTrees,
        //    TileID.PalmTree,
        //    TileID.VanityTreeYellowWillow,
        //    TileID.VanityTreeSakura
        //};
    }

}
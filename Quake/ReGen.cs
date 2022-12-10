using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TShockAPI;


namespace Quake
{
    public class ReGen
    {
        public static bool isTaskRunning = false;
        public static bool isWorldTaskRunning = false;

        private static int CoolDown = -1;

        private static int secondLast;

        public static int defaultMaxSpawns = NPC.defaultMaxSpawns;

        public static int hotelRooms = 3;

        private static List<Point> chests = new List<Point>();

        private static Config Con { get { return ConfigHelper.Con; } }
        private static WorldInfo Rec { get { return WorldHelper.Rec; } }
        public static bool NeedKick { get { return Con.kickGen && isWorldTaskRunning; } }
        private static int GetIcon { get { return Main.hardMode ? 2113 : 2111; } } // 世界吞噬怪面具 | 毁灭者面具


        public static void Reset()
        {
            isTaskRunning = false;
            isWorldTaskRunning = false;
            CoolDown = -1;
            secondLast = 0;
            chests.Clear();
        }

        public static void Trigger(int second = 30)
        {
            CoolDown = second * 60 + 1; //x秒后执行
            BackupHelper.Backup();
            Report();
        }

        #region OnGameUpdate
        public static void OnGameUpdate()
        {
            // 创建世界期间 移除敌怪
            if (isTaskRunning && isWorldTaskRunning)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && !Main.npc[i].friendly)
                    {
                        Main.npc[i].active = false;
                        Main.npc[i].type = 0;
                        TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                    }
                }
            }

            if (CoolDown == -1) return;

            //Console.WriteLine($"{CoolDown}");
            if (CoolDown > 0) CoolDown--;

            if (CoolDown == 0)
            {
                CoolDown = -1;
                GenWorldBefore(ConfigHelper.GetRandomSeed);
            }
            else if (CoolDown == Con.quakeDelay * 60)
            {
                TSPlayer.All.SendInfoMessage($"[i:{GetIcon}]地震还有 {Con.quakeDelay} 秒 抵达，请及时抢救物资！");
            }
            else if (CoolDown == 30 * 60)
                TSPlayer.All.SendInfoMessage($"[i:{GetIcon}]地震还有 30秒 抵达");
            else if (CoolDown == 15 * 60)
                TSPlayer.All.SendInfoMessage($"[i:{GetIcon}]地震还有 15秒 抵达");
            else if (CoolDown == 10 * 60)
                TSPlayer.All.SendInfoMessage($"[i:{GetIcon}]地震还有 10秒 抵达！");
            else if (CoolDown == 5 * 60)
                TSPlayer.All.SendErrorMessage($"[i:{GetIcon}]地震还有 5秒 抵达！！");
        }
        #endregion


        #region GenWorld前的准备工作
        public static async void GenWorldBefore(string seedStr = "", int size = 0, int evil = -1, string eggStr = "")
        {
            if (isTaskRunning) return;

            // 备份
            WorldHelper.Backup();
            TSPlayer.All.SendErrorMessage("[i:556]世界正在解体~");
            secondLast = utils.GetUnixTimestamp;

            if (Con.kickGen) utils.KickAllPlayer();
            utils.AllPlayerGoHome();

            // 设置创建参数
            // 种子
            ProcessSeeds(seedStr);
            ProcessEggSeeds(eggStr);
            if (Main.ActiveWorldFileData.Seed == 5162020) seedStr = "5162020";
            if (string.IsNullOrEmpty(seedStr) || seedStr == "0") seedStr = "random";

            if (seedStr.ToLowerInvariant() == "random")
                Main.ActiveWorldFileData.SetSeedToRandom();
            else
                Main.ActiveWorldFileData.SetSeed(seedStr);

            // 大小 腐化
            if (Con.size > 0 && Con.size < 4) size = Con.size;
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

            hotelRooms = NPCHelper.CoundHotelRooms();

            //搬家提示
            //Report();

            // 创建世界
            TSPlayer.All.SendErrorMessage($"[i:3061]世界正在重建 | [i:62]{WorldGen.currentWorldSeed}");
            await AsyncGenerateWorld(Main.ActiveWorldFileData.Seed);

            // 创建完成
            int second = utils.GetUnixTimestamp - secondLast;
            string text = $"[i:3061]世界重建完毕 ；-）| [i:17]{second}s | [i:62]{WorldGen.currentWorldSeed}";
            if (!string.IsNullOrEmpty(Con.successTips)) text += $"\n{Con.successTips}";
            TSPlayer.All.SendSuccessMessage(text);
            utils.Log($"世界重建完毕，用时 {second}s, 种子 {WorldGen.currentWorldSeed}");
        }
        /// <summary>
        /// 搬家报告
        /// </summary>
        public static void Report()
        {
            List<string> texts = new List<string>();
            List<string> names = WorldHelper.ListedChest();
            if (names.Count > 0)
            {
                names[0] = $"[i:2748] {names.Count}个: " + names[0];
                texts.Add(string.Join("\n", PaginationTools.BuildLinesFromTerms(names, null, " ", 160)));
            }
            names = WorldHelper.ListedPylon();
            if (names.Count > 0)
            {
                texts.Add($"晶塔 {names.Count}个:" + string.Join("\n", utils.BuildLinesFromTerms(names, 20)));
            }
            names = WorldHelper.ListedExtra();
            if (names.Count > 0)
            {
                texts.Add($"其它 {names.Count}个:\n" + string.Join("\n", utils.BuildLinesFromTerms(names, 20)));
            }
            if (texts.Count > 0)
                TSPlayer.All.SendInfoMessage($"[i:903]搬家清单: \n" + string.Join("\n", texts));
        }
        #endregion

        #region AsyncGenerateWorld
        // 参考：https://github.com/Illuminousity/WorldRefill/blob/master/WorldRefill/WorldRefill.cs#L997
        private static Task AsyncGenerateWorld(int seed)
        {
            // 开始创建
            isTaskRunning = true;
            isWorldTaskRunning = true;
            return Task.Run(() =>
            {
                ClearWorld();
                defaultMaxSpawns = NPC.defaultMaxSpawns;
                NPC.defaultMaxSpawns = 0;
                WorldGen.clearWorld();
                WorldGen.GenerateWorld(seed);
            }).ContinueWith((d) => GenWorldAfter());
        }

        /// <summary>
        /// 清空物品、射弹、非城镇NPC
        /// </summary>
        public static void ClearWorld()
        {
            if (Con.kickGen) return;

            for (int i = 0; i < Main.maxItems; i++)
            {
                if (Main.item[i].active)
                {
                    Main.item[i].active = false;
                    TSPlayer.All.SendData(PacketTypes.ItemDrop, "", i);
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
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active)
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }
            }
        }

        /// <summary>
        /// 创建世界后
        /// </summary>
        private static Task GenWorldAfter()
        {
            return Task.Run(() =>
            {
                // 设置彩蛋属性
                if (Con.keepFTW) Main.getGoodWorld = true;
                if (Con.keepNTB) Main.notTheBeesWorld = true;
                if (Con.keepDST) Main.dontStarveWorld = true;
                if (Con.keep2020) Main.drunkWorld = true;
                if (Con.keep2021) Main.tenthAnniversaryWorld = true;
                // 还原上次的记录
                WorldHelper.Recover();

                // 重新生成月亮事件
                if (WorldHelper.LunarApocalypseIsUp)
                {
                    NPC.downedAncientCultist = true;
                    NPC.LunarApocalypseIsUp = true;
                    WorldGen.TriggerLunarApocalypse();
                }
                TSPlayer.All.SendData(PacketTypes.WorldInfo);

                // NPC小旅馆
                if (Con.autoCreateRoom) GenRooms(Main.spawnTileX, Main.spawnTileY - 5, Math.Max(3, hotelRooms), true, true, true);

                // 还原箱子
                RecoverChest(chests);

                // 地狱直通车
                if (Rec.downedBoss3 && Con.autoCreateHellevator) GenHellevator(Main.spawnTileX, Main.spawnTileY);

                // 传送到出生点
                utils.AllPlayerGoHome();

                FinishGen();
                InformPlayers();
                isWorldTaskRunning = false;

                NPC.defaultMaxSpawns = defaultMaxSpawns;

                // 复活NPC
                NPCHelper.Recover(Rec.npcs);

                // 进入肉后模式
                if (Rec.hardMode) WorldGen.StartHardmode();
            });
        }

        public static void Backup()
        {
            BackupHelper.Backup();
            WorldHelper.Backup();
        }

        // 还原世界
        public static Task AsyncRecover()
        {
            isTaskRunning = true;
            return Task.Run(() => Recover());
        }
        private static void Recover()
        {
            hotelRooms = NPCHelper.CoundHotelRooms();
            Report();
            ClearWorld();
            WorldHelper.Reload();
            GenWorldAfter();
        }


        /// <summary>
        /// 还原箱子
        /// </summary>
        public static Task AsyncRecoverChest(List<Point> chests)
        {
            isTaskRunning = true;
            return Task.Run(() => RecoverChest(chests));
        }
        private static void RecoverChest(List<Point> chests)
        {
            int completed = 0;
            List<ChestData> todoChests = Rec.chests.Concat(Rec.pylons).ToList().Concat(Rec.extras).ToList();
            foreach (Point pos in chests)
            {
                int index = Chest.FindChest(pos.X, pos.Y);
                if (index == -1)
                {
                    utils.Log($"{pos.X} {pos.Y} 位置找不到箱子");
                    continue;
                }

                if (completed >= todoChests.Count) continue;
                ChestData chd = todoChests[completed];
                completed++;

                Chest ch = Main.chest[index];
                ch.name = chd.name;

                for (int k = 0; k < 40; k++)
                {
                    NetItem nItem = chd.GetItem(k);
                    if (nItem.NetId == 0) continue;
                    ch.item[k] = utils.NetItemToItem(nItem);
                }
                if (!Con.kickGen)
                {
                    utils.SyncChestName(index, ch.x, ch.y);
                    NetMessage.SendData(31, -1, -1, null, ch.x, ch.y);
                }
            }

            if (completed < todoChests.Count)
            {
                int remain = todoChests.Count - completed;
                int w = (todoChests.Count - completed) * 2;
                int x = Main.spawnTileX - w / 2;
                int y = Main.spawnTileY;
                int h = 5;
                List<Point> chests2 = new List<Point>();

                Parallel.For(x, x + w, (cx) =>
                {
                    Parallel.For(y - h, y, (cy) =>
                    {
                        // 清空区域
                        Main.tile[cx, cy].ClearEverything();
                    });
                    if (trees.Contains(Main.tile[cx - 1, y].type))
                        Main.tile[cx - 1, y].ClearEverything();
                    if (trees.Contains(Main.tile[cx + w + 1, y].type))
                        Main.tile[cx + w + 1, y].ClearEverything();
                });

                Parallel.For(x, x + w, (cx) =>
                {

                    // 平台
                    WorldGen.PlaceTile(cx, y - 3, 19, false, true, -1, 14);

                    // 地基
                    Main.tile[cx, y].type = TileID.GrayBrick;  // 灰砖 
                    Main.tile[cx, y].active(true);
                    Main.tile[cx, y].slope(0);
                    Main.tile[cx, y].halfBrick(false);
                });

                Parallel.For(x, x + w, (cx) =>
                {
                    if ((cx - x) % 2 == 0)
                    {
                        WorldGen.PlaceChest(cx, y - 4, 21, false, 47);
                        chests2.Add(new Point(cx, y - 5));
                        if (!Con.kickGen) NetMessage.SendData(34, -1, -1, null, 0, x, y, 47);
                    }
                });

                foreach (Point pos in chests2)
                {
                    int index = Chest.FindChest(pos.X, pos.Y);
                    if (index == -1)
                    {
                        utils.Log($"{pos.X} {pos.Y} 位置找不到箱子");
                        continue;
                    }

                    if (completed >= todoChests.Count) continue;
                    ChestData chd = todoChests[completed];
                    completed++;

                    Chest ch = Main.chest[index];
                    ch.name = chd.name;

                    for (int k = 0; k < 40; k++)
                    {
                        NetItem nItem = chd.GetItem(k);
                        if (nItem.NetId == 0) continue;
                        ch.item[k] = utils.NetItemToItem(nItem);
                    }
                    if (!Con.kickGen)
                    {
                        utils.SyncChestName(index, ch.x, ch.y);
                        NetMessage.SendData(31, -1, -1, null, ch.x, ch.y);
                    }
                }
            }
            TSPlayer.All.SendInfoMessage($"[i:3609] 搬家完毕，已处理 {completed} 个箱子");
            if (completed < todoChests.Count)
            {
                List<string> names = new List<string>();
                for (int i = completed; i < todoChests.Count; i++)
                {
                    names.Add(todoChests[i].name);
                    utils.Log($"未处理 {todoChests[i].name} {todoChests[i].itemstr}");
                    WorldHelper.SaveChestTxt(todoChests[i].itemstr);
                }
                TSPlayer.All.SendErrorMessage($"{todoChests.Count - completed}个箱子未处理：\n{string.Join("\n", PaginationTools.BuildLinesFromTerms(names))}\n服主输入/quake ic可快速补录最后一个，更多请手动修改chest.txt");
            }
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

        #region 生成小房间
        public static Task AsyncGenRoom(int posX, int posY, int total = 1, bool isRight = true, bool needCenter = false, bool clearUnder = false)
        {
            isTaskRunning = true;
            return Task.Run(() => GenRooms(posX, posY, total, isRight, needCenter, clearUnder));
        }
        private static void GenRooms(int posX, int posY, int total, bool isRight = true, bool needCenter = false, bool clearUnder = false)
        {
            int w = 5;
            int row = 6;
            int roomWidth = 1 + Math.Min(row, total) * w;

            chests.Clear();
            int fixPosX = needCenter ? posX - (roomWidth / 2) : posX;
            int startX;
            int startY;
            //utils.Log($"房间数:{total} posX:{posX} posY:{posY} startX:{fixPosX}");
            for (int i = 0; i < total; i++)
            {
                startX = fixPosX + i % row * w;
                startY = posY - (int)Math.Floor((float)(i / row)) * 10;
                GenRoom(startX, startY, isRight);
            }


            if (clearUnder)
            {
                int Xstart = isRight ? fixPosX : fixPosX + 2;
                int Ystart = posY;
                Parallel.For(Xstart, Xstart + roomWidth, (cx) =>
                {
                    Parallel.For(Ystart, Ystart + 5, (cy) =>
                    {
                        Main.tile[cx, cy].ClearEverything();    // 清空区域
                    });

                    Main.tile[cx, Ystart + 5].type = TileID.GrayBrick;  // 灰砖 
                    Main.tile[cx, Ystart + 5].active(true);
                    Main.tile[cx, Ystart + 5].slope(0);
                    Main.tile[cx, Ystart + 5].halfBrick(false);

                    if (trees.Contains(Main.tile[cx, Ystart + 6].type))
                        Main.tile[cx, Ystart + 6].ClearEverything();
                });
            }
        }
        private static void GenRoom(int posX, int posY, bool isRight = true)
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
                if (trees.Contains(Main.tile[cx, Ystart - height - 1].type))
                    Main.tile[cx, Ystart - height - 1].ClearEverything();

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
            if (chests.Count < 12)
            {
                int x = Xstart + 1;
                int y = Ystart - 2;
                WorldGen.PlaceChest(x, y, 21, false, chest.style);
                chests.Add(new Point(x, y - 1));
                if (!Con.kickGen) NetMessage.SendData(34, -1, -1, null, 0, x, y, chest.style);

                x = Xstart + 3;
                y = Ystart - 2;
                WorldGen.PlaceChest(x, y, 21, false, chest.style);
                chests.Add(new Point(x, y - 1));
                if (!Con.kickGen) NetMessage.SendData(34, -1, -1, null, 0, x, y, chest.style);
            }
        }
        #endregion

        #region 生成地狱直通车
        public static Task AsyncGenHellevator(int posX, int posY)
        {
            isTaskRunning = true;
            return Task.Run(() => GenHellevator(posX, posY));
        }
        private static void GenHellevator(int posX, int posY)
        {
            int hell;
            int xtile;
            for (hell = Main.UnderworldLayer + 10; hell <= Main.maxTilesY - 100; hell++)
            {
                xtile = posX;
                Parallel.For(posX, posX + 8, (cwidth, state) =>
                {
                    if (Main.tile[cwidth, hell].active() && !Main.tile[cwidth, hell].lava())
                    {
                        state.Stop();
                        xtile = cwidth;
                        return;
                    }
                });

                if (!Main.tile[xtile, hell].active()) break;
            }

            int Width = 5;
            int height = hell;
            int Xstart = posX - 2;
            int Ystart = posY;
            //utils.Log($"地狱的高度：{height}");

            Parallel.For(Xstart, Xstart + Width, (cx) =>
            {
                Parallel.For(Ystart, hell, (cy) =>
                {
                    Main.tile[cx, cy].ClearEverything();

                    if (trees.Contains(Main.tile[cx, cy - 1].type))
                        Main.tile[cx, cy - 1].ClearEverything();
                });
            });
            Parallel.For(Xstart, Xstart + Width, (cx) =>
            {
                Parallel.For(Ystart, hell, (cy) =>
                {
                    if (cx == Xstart + Width / 2)
                    {
                        Main.tile[cx, cy].type = TileID.SilkRope;
                        Main.tile[cx, cy].active(true);
                        Main.tile[cx, cy].slope(0);
                        Main.tile[cx, cy].halfBrick(false);
                    }
                    else if (cx == Xstart || cx == Xstart + Width - 1)
                    {
                        Main.tile[cx, cy].type = TileID.GrayBrick;  // 灰砖 
                        Main.tile[cx, cy].active(true);
                        Main.tile[cx, cy].slope(0);
                        Main.tile[cx, cy].halfBrick(false);
                    }
                });
            });
            // 平台
            WorldGen.PlaceTile(Xstart + 1, Ystart, 19, false, true, -1, 43);
            WorldGen.PlaceTile(Xstart + 2, Ystart, 19, false, true, -1, 43);
            WorldGen.PlaceTile(Xstart + 3, Ystart, 19, false, true, -1, 43);
        }
        #endregion



        #region 创建完成后
        /// <summary>
        /// 完成创建
        /// </summary>
        public static void FinishGen(bool needSound = true)
        {
            isTaskRunning = false;
            if (needSound)
            {
                foreach (TSPlayer player in TShock.Players)
                {
                    if (player != null && (player.Active))
                    {
                        NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(player.TPlayer.position, 1, 0, 10, -16));
                    }
                }
            }
            TShock.Utils.SaveWorld();
        }
        /// <summary>
        /// 更新物块
        /// </summary>
        public static void InformPlayers()
        {
            if (Con.kickGen) return;

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

        #region trees
        public static List<ushort> trees = new List<ushort>{
            TileID.Trees,
            TileID.TreeAmber,
            TileID.TreeAmethyst,
            TileID.TreeDiamond,
            TileID.TreeEmerald,
            TileID.TreeRuby,
            TileID.TreeSapphire,
            TileID.TreeTopaz,
            TileID.MushroomTrees,
            TileID.PalmTree,
            TileID.VanityTreeYellowWillow,
            TileID.VanityTreeSakura
        };
        #endregion

    }

}
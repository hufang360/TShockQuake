using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace Quake
{
    [ApiVersion(2, 1)]
    public class Quake : TerrariaPlugin
    {
        # region Plugin Info
        public override string Name => "Quake";
        public override string Description => "大地动";
        public override string Author => "hufang360";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        #endregion

        readonly string SaveDir = Path.Combine(TShock.SavePath, "Quake");

        public Quake(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command(new List<string>() { "quake" }, QuakeCommand, "quake") { HelpText = "大地动" });
            ServerApi.Hooks.NpcSpawn.Register(this, OnNpcSpawn);
            ServerApi.Hooks.NpcKilled.Register(this, OnNpcKill);
            ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
            ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);

            ConfigHelper.SaveFile = Path.Combine(SaveDir, "config.json");
            WorldHelper.SaveFile = Path.Combine(SaveDir, "record.json");
            WorldHelper.SaveDir = SaveDir;
            BackupHelper.BackupPath = Path.Combine(SaveDir, "backups");
            if (!Directory.Exists(SaveDir)) Directory.CreateDirectory(SaveDir);

            ConfigHelper.Init();
            WorldHelper.Init();
        }

        #region  command
        private async void QuakeCommand(CommandArgs args)
        {
            TSPlayer op = args.Player;
            void ShowHelpText()
            {
                op.SendInfoMessage("/quake trigger，触发大地动");
                op.SendInfoMessage("/quake room <数量>，玻璃小房间");
                op.SendInfoMessage("/quake hotel，NPC小旅馆");
                op.SendInfoMessage("/quake hell，地狱直通车");
                op.SendInfoMessage("/quake ic [txt]，导入箱子 ic=ImportChest");
                op.SendInfoMessage("/quake report，搬家报告");
                op.SendInfoMessage("/quake backup，[测试]备份世界属性");
                op.SendInfoMessage("/quake recover，[测试]恢复世界属性");
                op.SendInfoMessage("/quake reload，[测试]重新读取boss进度");
            }

            if (args.Parameters.Count == 0)
            {
                ShowHelpText();
                return;
            }

            bool isRight;
            int num = 0;
            switch (args.Parameters[0].ToLowerInvariant())
            {
                // 帮助
                case "help":
                default:
                    ShowHelpText();
                    return;

                // 触发大地动
                case "trigger":
                case "t":
                    if (!op.RealPlayer)
                    {
                        op.SendErrorMessage("本命令需在游戏内执行！");
                        return;
                    }
                    if (!utils.TryParseInt(args.Parameters, 1, out num)) num = ConfigHelper.Con.quakeDelay;
                    ReGen.Trigger(num);
                    return;

                // 使用种子创建
                case "seed":
                    if (args.Parameters.Count > 1)
                    {
                        args.Parameters.RemoveAt(0);
                        string seed = string.Join(" ", args.Parameters);
                        Console.WriteLine(seed);
                        ReGen.GenWorldBefore(seed);
                    }
                    else
                    {
                        ReGen.GenWorldBefore(ConfigHelper.GetRandomSeed);
                    }
                    return;


                // 玻璃小房间
                case "room":
                    int total;
                    if (!utils.TryParseInt(args.Parameters, 1, out total))
                    {
                        op.SendErrorMessage("输入的房间数量不对");
                        return;
                    }
                    if (total < 1 || total > 1000) total = 3;
                    isRight = op.TPlayer.direction != -1;
                    int tryX = isRight ? op.TileX : op.TileX;
                    int tryY = op.TileY + 4;
                    await ReGen.AsyncGenRoom(tryX, tryY, total, isRight, true);
                    op.SendSuccessMessage($"已创建 {total}个小房间");
                    ReGen.FinishGen();
                    ReGen.InformPlayers();
                    return;

                // 出生点小套间
                case "hotel":
                    await (ReGen.AsyncGenRoom(Main.spawnTileX, Main.spawnTileY - 5, Math.Max(3, NPCHelper.CoundHotelRooms()), true, true, true));
                    ReGen.FinishGen();
                    ReGen.InformPlayers();
                    op.SendSuccessMessage("创建NPC小旅馆结束");
                    return;

                // 地狱直通车
                case "hell":
                    await (ReGen.AsyncGenHellevator(Main.spawnTileX, Main.spawnTileY));
                    ReGen.FinishGen();
                    ReGen.InformPlayers();
                    op.SendSuccessMessage("创建地狱直通车结束");
                    return;

                // 备份世界状态
                case "backup":
                    if( args.Parameters.Count>1)
                    {
                        args.Parameters.RemoveAt(0);
                        BackupHelper.BackNotes = string.Join("", args.Parameters[1]);
                    }
                    ReGen.Backup();
                    op.SendSuccessMessage("备份世界完成");
                    break;

                // 复原世界状态
                case "recover":
                    await ReGen.AsyncRecover();
                    op.SendSuccessMessage("复原世界完成");
                    break;

                // 搬家报告
                case "report":
                    ReGen.Report();
                    break;

                // 导入箱子
                case "ic": ImportChest(args); break;


                // 重载配置
                case "reload":
                    BossList.Clear();
                    ReGen.Reset();
                    ConfigHelper.Reload();
                    WorldHelper.Reload();
                    op.SendSuccessMessage("已重读boss进度 和 配置文件");
                    return;
            }
        }
        #endregion

        #region 导入箱子
        private void ImportChest(CommandArgs args)
        {
            TSPlayer op = args.Player;

            bool needTxt = utils.TryParseString(args.Parameters, 1) == "txt";

            // 生成示例文件
            string path = Path.Combine(SaveDir, "chest.txt");
            NetItem[] items;
            if (needTxt)
            {
                items = new NetItem[40];
                items[0] = new NetItem(-15, 1, 0);
                items[1] = new NetItem(-13, 1, 0);
                items[2] = new NetItem(-16, 1, 0);
                WorldHelper.SaveChestTxt(string.Join("~", items));
                op.SendErrorMessage($"已生成示例文件 chest.txt");
                return;
            }

            if (!op.RealPlayer)
            {
                op.SendErrorMessage("本命令需在游戏内执行！");
                return;
            }
            int chestIndex = Main.player[op.Index].chest;
            if (chestIndex == -1)
            {
                op.SendErrorMessage("请先打开一个箱子，然后再执行本指令！");
                return;
            }

            if (!File.Exists(path))
            {
                op.SendErrorMessage($"导入箱子里的物品失败，找不到 {path} 文件，输入 /quake chest txt 可以生成示例文件");
                return;
            }


            // 导入箱子
            string text = File.ReadAllText(path).Trim('"').Trim('\n').Trim('\r');
            if (!text.Contains("~"))
            {
                op.SendErrorMessage($"chest.txt 里的内容格式不对，输入 /quake chest txt 生成示例文件");
                return;
            }

            items = text.Split('~').Select(NetItem.Parse).ToArray();
            Chest ch = Main.chest[chestIndex];

            int count = 0;
            for (int i = 0; i < items.Length; i++)
            {
                if (i >= 40)
                    continue;
                NetItem nItem = items[i];
                if (nItem.NetId == 0) continue;
                ch.item[i] = utils.NetItemToItem(nItem);
                NetMessage.TrySendData(32, op.Index, -1, null, chestIndex, i);
                count++;
            }
            op.SendSuccessMessage($"操作完成，共导入 {count} 个物品!");
        }
        #endregion

        #region event
        private void OnGameUpdate(EventArgs args) { ReGen.OnGameUpdate(); }
        private void OnServerJoin(JoinEventArgs args) { if (ReGen.NeedKick) TShock.Players[args.Who].Disconnect("世界正在重建，请稍等2分钟！"); }

        private readonly Dictionary<int, bool> BossList = new Dictionary<int, bool>();
        private void OnNpcSpawn(NpcSpawnEventArgs args)
        {
            NPC npc = Main.npc[args.NpcId];
            int id = npc.netID;
            if (!NPCHelper.BossIDs.Contains(id)) return;

            Console.WriteLine($"OnNpcSpawn: npcID:{id} downed:{NPCHelper.CheckBossDowned(id)} {npc}");
            if (BossList.ContainsKey(id))
                BossList[id] = NPCHelper.CheckBossDowned(id);
            else
                BossList.Add(id, NPCHelper.CheckBossDowned(id));
        }

        private void OnNpcKill(NpcKilledEventArgs args)
        {
            NPC npc = args.npc;
            int id = npc.netID;
            if (!BossList.ContainsKey(id)) return;

            Console.WriteLine($"OnNpcKill: npcID:{id} downed:{NPCHelper.CheckBossDowned(id)} {npc}");
            if (!BossList[id] && NPCHelper.CheckBossDowned(id))
            {
                BossList[id] = true;

                if (!ReGen.isTaskRunning)
                {
                    if (id == 439)
                    {
                        NPC.downedAncientCultist = false;
                        NPC.LunarApocalypseIsUp = false;
                        TSPlayer.All.SendData(PacketTypes.WorldInfo);

                        WorldHelper.LunarApocalypseIsUp = true;

                        List<int> towers = new List<int>() { 517, 422, 507, 493 };
                        foreach (int id2 in towers)
                        {
                            int index = NPCHelper.ClearNPCByID(id2);
                            if (index != -1)
                                NetMessage.SendData(23, -1, -1, null, index);
                        }
                    }
                    //获得进度备注
                    BackupHelper.BackNotes = NPCHelper.GetBossInfoNote(id);
                    // 触发大地动
                    ReGen.Trigger(ConfigHelper.Con.quakeDelay);
                }
            }
        }
        #endregion

        #region dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NpcSpawn.Deregister(this, OnNpcSpawn);
                ServerApi.Hooks.NpcKilled.Deregister(this, OnNpcKill);
                ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
                ServerApi.Hooks.ServerJoin.Deregister(this, OnServerJoin);
            }
            base.Dispose(disposing);
        }
        #endregion
    }

}

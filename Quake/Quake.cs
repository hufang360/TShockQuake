using System;
using System.Collections.Generic;
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


        public Quake(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command(new List<string>() { "quake" }, QuakeCommand, "quake") { HelpText = "大地动" });
            ServerApi.Hooks.NpcSpawn.Register(this, OnNpcSpawn);
            ServerApi.Hooks.NpcKilled.Register(this, OnNpcKill);
            ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);

            ConfigHelper.Init();
            BackupHelper.Init();
            ReGenHelper.Init();
        }

        #region  command
        private async void QuakeCommand(CommandArgs args)
        {
            TSPlayer op = args.Player;
            void ShowHelpText()
            {
                op.SendInfoMessage("/quake trigger，触发大地动");
                op.SendInfoMessage("/quake room <数量>，生成玻璃小房间");
                op.SendInfoMessage("/quake spawnroom，出生点玻璃小套间");
                op.SendInfoMessage("/quake freeze，全员禁足");
                op.SendInfoMessage("/quake size <1/2/3>，设置创建世界的大小");
                op.SendInfoMessage("/quake reload，[测试]重新读取boss进度");
            }

            if (args.Parameters.Count == 0)
            {
                ShowHelpText();
                return;
            }

            bool isRight;
            int num;
            switch (args.Parameters[0].ToLowerInvariant())
            {
                // 帮助
                case "help":
                default:
                    ShowHelpText();
                    return;

                // 使用种子创建
                case "seed":
                    if (args.Parameters.Count > 1)
                    {
                        args.Parameters.RemoveAt(0);
                        string seed = string.Join(" ", args.Parameters);
                        Console.WriteLine(seed);
                        ReGenHelper.GenWorld(seed);
                    }
                    else
                    {
                        ReGenHelper.GenWorld(ConfigHelper.GetRandomSeed);
                    }
                    return;


                // 触发大地动
                case "trigger":
                    if (args.Parameters.Count > 1)
                    {
                        if (!int.TryParse(args.Parameters[1], out num)) num = 30;
                    }
                    else
                    {
                        num = 30;
                    }
                    ReGenHelper.Trigger(num);
                    return;

                // 玻璃小房间
                case "room":
                    int total = 3;
                    if (args.Parameters.Count > 1)
                    {
                        if (!int.TryParse(args.Parameters[1], out total))
                        {
                            op.SendErrorMessage("输入的房间数量不对");
                            return;
                        }
                        if (total < 1 || total > 1000)
                        {
                            total = 3;
                        }
                    }
                    isRight = op.TPlayer.direction != -1;
                    int tryX = isRight ? op.TileX : op.TileX;
                    int tryY = op.TileY + 4;
                    await (ReGenHelper.AsyncGenRoom(tryX, tryY, total, isRight, true));
                    return;

                // 出生点小套间
                case "spawnroom":
                    int npcTotal = NPCHelper.CountTownNPC();
                    await (ReGenHelper.AsyncGenRoom(Main.spawnTileX, Main.spawnTileY - 4, npcTotal, true, true));
                    return;

                // 全员禁足
                case "freeze":
                    ReGenHelper.FreezePlayer(30);
                    return;


                // 配置重建世界的大小
                case "size":
                    int size = ConfigHelper.Config.size;
                    if (args.Parameters.Count == 1)
                    {
                        string sizeStr;
                        if (size == 1)
                            sizeStr = "配置重建 小 世界";
                        else if (size == 2)
                            sizeStr = "配置重建 中 世界";
                        else if (size == 3)
                            sizeStr = "配置重建 大 世界";
                        else
                            sizeStr = "配置的数值不正确！";
                        op.SendInfoMessage(sizeStr);
                    }
                    else
                    {
                        if (int.TryParse(args.Parameters[1], out int size2))
                        {
                            string sizeStr;
                            if (size == 1)
                                sizeStr = "已配置重建 小 世界";
                            else if (size == 2)
                                sizeStr = "已配置重建 中 世界";
                            else if (size == 3)
                                sizeStr = "已配置重建 大 世界";
                            else
                                sizeStr = "所配置的数值不正确！";
                            op.SendInfoMessage(sizeStr);

                            ConfigHelper.Config.size = size2;
                            ConfigHelper.Save();
                        }
                        else
                        {
                            op.SendInfoMessage("请输入 /quake size <1/2/3>");
                        }
                    }
                    return;


                case "reload":
                    BossList.Clear();
                    ReGenHelper.Reset();
                    ConfigHelper.Reload();
                    op.SendSuccessMessage("已重新读取boss进度");
                    return;
            }
        }
        #endregion

        #region OnGameUpdate
        private void OnGameUpdate(EventArgs args)
        {
            ReGenHelper.OnGameUpdate();
        }
        #endregion

        #region OnNpcSpawn & OnNpcKill
        private readonly Dictionary<int, bool> BossList = new Dictionary<int, bool>();
        private readonly List<int> BossIDs = new List<int>(){
            50, // 史莱姆王
            4, // 克苏鲁之眼
            13,14,15, // 世界吞噬怪
            266, // 克苏鲁之脑
            222, // 蜂王
            35, // 骷髅王
            668, // 鹿角怪
            113, // 血肉墙
            134,135,136, // 毁灭者
            125,126, // 双子 激光眼 魔焰眼
            127, // 机械骷髅王
            262, // 世纪之花
            245, // 石巨人
            657, // 史莱姆皇后
            636, // 光之女皇
            370, // 猪龙鱼公爵
            439 // lc
        };
        private void OnNpcSpawn(NpcSpawnEventArgs args)
        {
            NPC npc = Main.npc[args.NpcId];
            int id = npc.netID;
            if (!BossIDs.Contains(id))
                return;

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

                if (!ReGenHelper.isTaskRunning)
                {
                    if (id == 439)
                    {
                        NPC.downedAncientCultist = false;
                        NPC.LunarApocalypseIsUp = false;
                        TSPlayer.All.SendData(PacketTypes.WorldInfo);

                        WorldHelper.LunarApocalypseIsUp = true;

                        // NPC.TowerActiveVortex = (NPC.TowerActiveNebula = (NPC.TowerActiveSolar = (NPC.TowerActiveStardust = false)));
                        // // NPC.LunarApocalypseIsUp = false;
                        // NPC.ShieldStrengthTowerSolar = (NPC.ShieldStrengthTowerVortex = (NPC.ShieldStrengthTowerNebula = (NPC.ShieldStrengthTowerStardust = 0)));
                        // NetMessage.SendData(101);
                        // Console.WriteLine("101");

                        List<int> towers = new List<int>() { 517, 422, 507, 493 };
                        foreach (int id2 in towers)
                        {
                            int index = NPCHelper.ClearNPCByID(id2);
                            if (index != -1)
                                NetMessage.SendData(23, -1, -1, null, index);
                        }
                    }
                    // 触发大地动
                    ReGenHelper.Trigger();
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
                BossList.Clear();
                BossIDs.Clear();
            }
            base.Dispose(disposing);
        }
        #endregion
    }

}

using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using TShockAPI;


namespace Quake
{
    public class NPCHelper
    {
        #region 恢复NPC
        public static void Recover(List<int> found)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].townNPC)
                {
                    if (found.Contains(Main.npc[i].type))
                        found.Remove(Main.npc[i].type);
                }
            }

            // 生成npc
            List<string> names = new List<string>();
            foreach (int npcID in found)
            {
                NPC npc = new NPC();
                npc.SetDefaults(npcID);
                TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, 1, Main.spawnTileX, Main.spawnTileY-2, 16, 0);

                if (names.Count != 0 && names.Count % 10 == 0)
                {
                    names.Add("\n" + npc.FullName);
                }
                else
                {
                    names.Add(npc.FullName);
                }
            }
        }
        #endregion

        /// <summary>
        /// 清理指定id的NPC
        /// </summary>
        public static int ClearNPCByID(int npcID)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].netID == npcID)
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 列出活着的NPC
        /// </summary>
        public static List<int> ListedAliveTownNPC(bool filterPet = true)
        {
            List<int> found = new List<int>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (!Main.npc[i].active || !Main.npc[i].townNPC)
                    continue;

                int id = Main.npc[i].netID;
                if (id == 453 || id == 368 || id == 37) continue;

                if (filterPet)
                {
                    if (id == 637 || id == 638 || id == 656) continue;
                }
                found.Add(id);
            }
            return found;
        }

        public static int CoundHotelRooms(bool filterPet = true)
        {
            List<int> found = new List<int>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (!Main.npc[i].active || !Main.npc[i].townNPC)
                    continue;

                int id = Main.npc[i].netID;
                if (id == 453 || id == 368 || id == 37) continue;

                if (filterPet)
                {
                    if (id == 637 || id == 638 || id == 656) continue;
                }
                found.Add(id);
            }
            return (found.Count + GetRelive(true).Count);
        }
        private static List<int> GetRelive(bool filterAlive=false)
        {
            List<int> found = new List<int>();

            // 解救状态
            // 渔夫
            if (NPC.savedAngler) found.Add(369);

            // 哥布林
            if (NPC.savedGoblin) found.Add(107);

            // 机械师
            if (NPC.savedMech) found.Add(124);

            // 发型师
            if (NPC.savedStylist) found.Add(353);

            // 酒馆老板
            if (NPC.savedBartender) found.Add(550);

            // 高尔夫球手
            if (NPC.savedGolfer) found.Add(588);

            // 巫师
            if (NPC.savedWizard) found.Add(108);

            // 税收管
            if (NPC.savedTaxCollector) found.Add(441);

            // 猫
            if (NPC.boughtCat) found.Add(637);

            // 狗
            if (NPC.boughtDog) found.Add(638);

            // 兔
            if (NPC.boughtBunny) found.Add(656);

            // 怪物图鉴解锁情况
            List<int> remains = new List<int>() {
                22, //向导
                19, //军火商
                54, //服装商
                38, //爆破专家
                20, //树妖
                207, //染料商
                17, //商人
                18, //护士
                227, //油漆工
                208, //派对女孩
                228, //巫医
                633, //动物学家
                209, //机器侠
                229, //海盗
                178, //蒸汽朋克人
                160, //松露人
                663 //公主

                // 453, //骷髅商人
                // 368, //旅商
                // 37, // 老人
            };
            // 142, //圣诞老人
            if (Main.xMas) remains.Add(142);

            foreach (int npcID1 in remains)
            {
                if (DidDiscoverBestiaryEntry(npcID1)) found.Add(npcID1);
            }

            if (filterAlive)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (!Main.npc[i].active || !Main.npc[i].townNPC) continue;
                    found.Remove(Main.npc[i].type);
                }
            }
            return found;
        }
        public static bool DidDiscoverBestiaryEntry(int npcId)
        {
            return Main.BestiaryDB.FindEntryByNPCID(npcId).UIInfoProvider.GetEntryUICollectionInfo().UnlockState > BestiaryEntryUnlockState.NotKnownAtAll_0;
        }
        public static bool CheckBossDowned(int id)
        {
            switch (id)
            {
                case 50:  return NPC.downedSlimeKing;
                case 4:   return NPC.downedBoss1;
                case 668: return NPC.downedDeerclops;
                case 266: return NPC.downedBoss2;   // 克脑
                case 222: return NPC.downedQueenBee;
                case 35:  return NPC.downedBoss3;   // 骷髅王
                case 113: return Main.hardMode;
                case 13:  case 14:  case 15:  return NPC.downedBoss2;       //世界吞噬者
                case 134: case 135: case 136: return NPC.downedMechBoss1;   //毁灭者
                case 125:
                case 126: return NPC.downedMechBoss2;
                case 127: return NPC.downedMechBoss3;
                case 262: return NPC.downedPlantBoss;
                case 245: return NPC.downedGolemBoss;
                case 657: return NPC.downedQueenSlime;
                case 636: return NPC.downedEmpressOfLight;
                case 370: return NPC.downedFishron;
                case 439: return NPC.downedAncientCultist;
                // case 396: return NPC.downedMoonlord;
                // case 325: return NPC.downedHalloweenTree;
                // case 327: return NPC.downedHalloweenKing;
                // case 344: return NPC.downedChristmasTree;
                // case 345: return NPC.downedChristmasIceQueen;
                // case 346: return NPC.downedChristmasSantank;
                // case 392: return NPC.downedMartians;
                // case 517: return NPC.downedTowerSolar;
                // case 422: return NPC.downedTowerVortex;
                // case 507: return NPC.downedTowerNebula;
                // case 493: return NPC.downedTowerStardust;
            }
            return false;
        }
        public static string GetBossInfoNote(int id)
        {
            switch (id)
            {
                case 50: return "史王";
                case 4: return "克眼";
                case 266: return "克脑";   //克脑
                case 668: return "鹿角怪";
                case 222: return "蜂王";
                case 35: return "骷髅王";
                case 113: return "血肉墙";
                case 13: case 14: case 15: return "世吞";       //世界吞噬者
                case 134: case 135: case 136: return "毁灭者";   //毁灭者
                case 125:
                case 126: return "双子魔眼";
                case 127: return "机械骷髅王";
                case 262: return "世花";
                case 245: return "石巨人";
                case 657: return "史莱姆皇后";
                case 636: return "光女";
                case 370: return "猪鲨";
                case 439: return "教徒";
            }
            return "";
        }

        public static readonly List<int> BossIDs = new List<int>(){
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
    }
}
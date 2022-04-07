using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using TShockAPI;


namespace Quake
{
    public class NPCHelper
    {
        #region 复活NPC
        /// <summary>
        // NPC重生
        /// <summary>
        public static void Relive()
        {
            List<int> found = new List<int>();

            // 解救状态
            // 渔夫
            if (NPC.savedAngler)
                found.Add(369);

            // 哥布林
            if (NPC.savedGoblin)
                found.Add(107);

            // 机械师
            if (NPC.savedMech)
                found.Add(124);

            // 发型师
            if (NPC.savedStylist)
                found.Add(353);

            // 酒馆老板
            if (NPC.savedBartender)
                found.Add(550);

            // 高尔夫球手
            if (NPC.savedGolfer)
                found.Add(588);

            // 巫师
            if (NPC.savedWizard)
                found.Add(108);

            // 税收管
            if (NPC.savedTaxCollector)
                found.Add(441);

            // 猫
            if (NPC.boughtCat)
                found.Add(637);

            // 狗
            if (NPC.boughtDog)
                found.Add(638);

            // 兔
            if (NPC.boughtBunny)
                found.Add(656);

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
            if (Main.xMas)
                remains.Add(142);

            foreach (int npcID1 in remains)
            {
                if (DidDiscoverBestiaryEntry(npcID1))
                    found.Add(npcID1);
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (!Main.npc[i].active || !Main.npc[i].townNPC)
                    continue;

                found.Remove(Main.npc[i].type);
            }

            // 生成npc
            List<string> names = new List<string>();
            foreach (int npcID in found)
            {
                NPC npc = new NPC();
                npc.SetDefaults(npcID);
                TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, 1, Main.spawnTileX, Main.spawnTileY, 5, 2);

                if (names.Count != 0 && names.Count % 10 == 0)
                {
                    names.Add("\n" + npc.FullName);
                }
                else
                {
                    names.Add(npc.FullName);
                }
            }

            // if( found.Count>0 )
            // {
            // 	TSPlayer.All.SendInfoMessage($"{found.Count}个 NPC已找回");
            // 	TSPlayer.All.SendInfoMessage($"{string.Join("、", names)}");
            // }
        }
        public static bool DidDiscoverBestiaryEntry(int npcId)
        {
            return Main.BestiaryDB.FindEntryByNPCID(npcId).UIInfoProvider.GetEntryUICollectionInfo().UnlockState > BestiaryEntryUnlockState.NotKnownAtAll_0;
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
        /// 统计NPC数量
        /// </summary>
        public static int CountTownNPC(bool filterPet = true)
        {
            int count = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (!Main.npc[i].active || !Main.npc[i].townNPC)
                    continue;

                int id = Main.npc[i].netID;
                if (id == 453 || id == 368 || id == 37)
                    continue;

                if (filterPet)
                {
                    if (id == 637 || id == 638 || id == 656)
                        continue;
                }
                count++;
            }
            return count;
        }

        public static bool CheckBossDowned(int id)
        {
            switch (id)
            {
                case 50: return NPC.downedSlimeKing;
                case 4: return NPC.downedBoss1;
                case 668: return NPC.downedDeerclops;
                case 13: case 14: case 15: return NPC.downedBoss2; //世界吞噬者
                case 266: return NPC.downedBoss2;   //克脑
                case 222: return NPC.downedQueenBee;
                case 35: return NPC.downedBoss3;
                case 113: return Main.hardMode;
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
    }
}
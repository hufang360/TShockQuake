using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using TShockAPI;


namespace Quake
{
    public class WorldHelper
    {
        private static List<bool> status = new List<bool>();
        private static NPCKillsTracker Kills = new NPCKillsTracker();
        private static NPCWasNearPlayerTracker Sights = new NPCWasNearPlayerTracker();
        private static NPCWasChatWithTracker Chats = new NPCWasChatWithTracker();

        private static int anglerQuest = 0;
        private static bool anglerQuestFinished = false;
        private static List<string> anglerWhoFinishedToday = new List<string>();
        public static bool LunarApocalypseIsUp = false;
        public static bool downedAncientCultist = false;

        private static int worldID = 0;

        #region 重置
        public static void Reset()
        {
            anglerQuest = 0;
            anglerQuestFinished = false;
            anglerWhoFinishedToday.Clear();
            LunarApocalypseIsUp = false;
            worldID = 0;
            Kills.Reset();
            Sights.Reset();
            Chats.Reset();
            status.Clear();
        }
        #endregion

        #region 备份世界纪录
        public static void Backup()
        {
            // WorldGen.cs clearWorld
            status.Add(Main.hardMode);
            status.Add(NPC.downedBoss1);
            status.Add(NPC.downedBoss2);
            status.Add(NPC.downedBoss3);
            status.Add(NPC.downedQueenBee);
            status.Add(NPC.downedSlimeKing);
            status.Add(NPC.downedMechBossAny);
            status.Add(NPC.downedMechBoss1);
            status.Add(NPC.downedMechBoss2);
            status.Add(NPC.downedMechBoss3);
            status.Add(NPC.downedFishron);
            status.Add(NPC.downedAncientCultist);
            status.Add(NPC.downedMoonlord);
            status.Add(NPC.downedHalloweenKing);
            status.Add(NPC.downedHalloweenTree);
            status.Add(NPC.downedChristmasIceQueen);
            status.Add(NPC.downedChristmasSantank);
            status.Add(NPC.downedChristmasTree);
            status.Add(NPC.downedPlantBoss);
            status.Add(NPC.downedGolemBoss);
            status.Add(NPC.downedEmpressOfLight);
            status.Add(NPC.downedQueenSlime);
            status.Add(NPC.downedDeerclops);
            status.Add(NPC.combatBookWasUsed);
            status.Add(NPC.savedStylist);
            status.Add(NPC.savedGoblin);
            status.Add(NPC.savedWizard);
            status.Add(NPC.savedMech);
            status.Add(NPC.savedTaxCollector);
            status.Add(NPC.savedAngler);
            status.Add(NPC.savedBartender);
            status.Add(NPC.savedGolfer);
            status.Add(NPC.boughtCat);
            status.Add(NPC.boughtDog);
            status.Add(NPC.boughtBunny);
            status.Add(NPC.downedGoblins);
            status.Add(NPC.downedClown);
            status.Add(NPC.downedFrost);
            status.Add(NPC.downedPirates);
            status.Add(NPC.downedMartians);
            status.Add(NPC.downedTowerSolar);
            status.Add(NPC.downedTowerVortex);
            status.Add(NPC.downedTowerNebula);
            status.Add(NPC.downedTowerStardust);

            worldID = Main.worldID;

            anglerQuest = Main.anglerQuest;
            foreach (string name in Main.anglerWhoFinishedToday)
            {
                anglerWhoFinishedToday.Add(name);
            }

            // 怪物图鉴
            foreach (string key in Main.BestiaryTracker.Kills._killCountsByNpcId.Keys)
            {
                Kills._killCountsByNpcId.Add(key, Main.BestiaryTracker.Kills._killCountsByNpcId[key]);
            }
            foreach (string key in Main.BestiaryTracker.Sights._wasNearPlayer)
            {
                Sights._wasNearPlayer.Add(key);
            }
            foreach (string key in Main.BestiaryTracker.Chats._chattedWithPlayer)
            {
                Chats._chattedWithPlayer.Add(key);
            }
        }
        #endregion

        #region 恢复世界纪录
        public static void Recover()
        {
            bool pop()
            {
                bool foo = status[0];
                status.RemoveAt(0);
                return foo;
            }
            Main.hardMode = pop();
            NPC.downedBoss1 = pop();
            NPC.downedBoss2 = pop();
            NPC.downedBoss3 = pop();
            NPC.downedQueenBee = pop();
            NPC.downedSlimeKing = pop();
            NPC.downedMechBossAny = pop();
            NPC.downedMechBoss1 = pop();
            NPC.downedMechBoss2 = pop();
            NPC.downedMechBoss3 = pop();
            NPC.downedFishron = pop();
            NPC.downedAncientCultist = pop();
            NPC.downedMoonlord = pop();
            NPC.downedHalloweenKing = pop();
            NPC.downedHalloweenTree = pop();
            NPC.downedChristmasIceQueen = pop();
            NPC.downedChristmasSantank = pop();
            NPC.downedChristmasTree = pop();
            NPC.downedPlantBoss = pop();
            NPC.downedGolemBoss = pop();
            NPC.downedEmpressOfLight = pop();
            NPC.downedQueenSlime = pop();
            NPC.downedDeerclops = pop();
            NPC.combatBookWasUsed = pop();
            NPC.savedStylist = pop();
            NPC.savedGoblin = pop();
            NPC.savedWizard = pop();
            NPC.savedMech = pop();
            NPC.savedTaxCollector = pop();
            NPC.savedAngler = pop();
            NPC.savedBartender = pop();
            NPC.savedGolfer = pop();
            NPC.boughtCat = pop();
            NPC.boughtDog = pop();
            NPC.boughtBunny = pop();
            NPC.downedGoblins = pop();
            NPC.downedClown = pop();
            NPC.downedFrost = pop();
            NPC.downedPirates = pop();
            NPC.downedMartians = pop();
            NPC.downedTowerSolar = pop();
            NPC.downedTowerVortex = pop();
            NPC.downedTowerNebula = pop();
            NPC.downedTowerStardust = pop();

            Main.worldID = worldID;

            // 钓鱼
            Main.anglerQuest = anglerQuest;
            Main.anglerQuestFinished = anglerQuestFinished;
            Main.anglerWhoFinishedToday.Clear();
            foreach (string name in anglerWhoFinishedToday)
            {
                Main.anglerWhoFinishedToday.Add(name);
            }

            // 怪物图鉴
            foreach (string key in Kills._killCountsByNpcId.Keys)
            {
                if (Main.BestiaryTracker.Kills._killCountsByNpcId.ContainsKey(key))
                    Main.BestiaryTracker.Kills._killCountsByNpcId[key] = Kills._killCountsByNpcId[key];
                else
                    Main.BestiaryTracker.Kills._killCountsByNpcId.Add(key, Kills._killCountsByNpcId[key]);
            }
            foreach (string key in Sights._wasNearPlayer)
            {
                if (!Main.BestiaryTracker.Sights._wasNearPlayer.Contains(key))
                    Main.BestiaryTracker.Sights._wasNearPlayer.Add(key);
            }
            foreach (string key in Chats._chattedWithPlayer)
            {
                if (!Main.BestiaryTracker.Chats._chattedWithPlayer.Contains(key))
                    Main.BestiaryTracker.Chats._chattedWithPlayer.Add(key);
            }

            // 进入肉后模式
            if (Main.hardMode) WorldGen.StartHardmode();

            TSPlayer.All.SendData(PacketTypes.WorldInfo);
        }
        #endregion

        

    }
}
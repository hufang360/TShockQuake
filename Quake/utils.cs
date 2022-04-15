using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using TShockAPI;

namespace Quake
{
    public class utils
    {
        public static int GetUnixTimestamp
        {
            get { return (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds; }
        }

        public static void ResetBuff(int index)
        {
            NetMessage.SendData(50, -1, -1, NetworkText.Empty, index, 0f, 0f, 0f, 0);
            NetMessage.SendData(50, index, -1, NetworkText.Empty, index, 0f, 0f, 0f, 0);
        }

        public static void PlayerGoHome(TSPlayer op)
        {
            op.Teleport(Main.spawnTileX * 16, (Main.spawnTileY * 16) - 48);
        }
        public static void AllPlayerGoHome()
        {
            foreach (TSPlayer op in TShock.Players)
            {
                if (op != null && op.Active)
                    op.Teleport(Main.spawnTileX * 16, (Main.spawnTileY * 16) - 48);
            }
            //TSPlayer.All.Teleport(Main.spawnTileX * 16, (Main.spawnTileY * 16) - 48);
        }

        public static void SyncChestName(int index, int posX, int posY)
        {
            foreach (TSPlayer op in TShock.Players)
            {
                //PacketTypes.ChestName = 69
                //PacketTypes.ChestGetContents = 31
                //PacketTypes.ChestItem = 32
                //PacketTypes.ChestOpen = 33
                if (op != null && op.Active)
                    NetMessage.TrySendData(69, op.Index, -1, null, index, posX, posY);
            }

            // 更新箱子里的东西
            //NetMessage.SendData(31, -1, -1, null, ch.x, ch.y);
            // 打开箱子
            //NetMessage.SendData(33, -1, -1, NetworkText.FromLiteral(chest[player.chest].name), Main.player[myPlayer].chest, 1f);

            // 实时添加物品
            //NetMessage.TrySendData(32, op.Index, -1, null, chestIndex, i);
        }

        public static void KickAllPlayer()
        {
            foreach (TSPlayer op in TShock.Players)
            {
                if (op != null && op.Active) op.Disconnect("世界正在重建，请稍等两分钟！");
            }
        }

        public static void Log(string msg)
        {
            TShock.Log.ConsoleInfo("[quake]" + msg);
        }

        public bool InArea(Rectangle area, int x, int y)
        {
            return x >= area.X && x <= area.X + area.Width && y >= area.Y && y <= area.Y + area.Height;
        }

        public static bool TryParseInt(List<string> args, int index, out int num)
        {
            if (index >= args.Count)
            {
                num = 0;
                return false;
            }
            return int.TryParse(args[index], out num);
        }
        public static string TryParseString(List<string> args, int index)
        {
            return index >= args.Count ? "" : args[index];
        }

        public static Item NetItemToItem(NetItem nItem)
        {
            Item item = new Item();
            item.netDefaults(nItem.NetId);
            item.stack = nItem.Stack;
            item.prefix = nItem.PrefixId;
            return item;
        }

        public static string GetItemName(int id) { return id == 0 ? "" : TShock.Utils.GetItemById(id).Name; }
        public static List<string> GetItemNames(List<int> ids)
        {
            List<string> names = new List<string>();
            foreach (int id in ids)
            {
                names.Add(GetItemName(id));
            }
            return names;
        }

        public static List<string> BuildLinesFromTerms(List<string> msgs, int count = 10)
        {
            string[] texts = new string[(int)Math.Ceiling((float)msgs.Count/count)];
            int amount;
            for(int i = 0; i < msgs.Count; i++)
            {
                amount = i/count;
                if (i%count==0)
                    texts[amount] = msgs[i];
                else
                    texts[amount] += msgs[i];
            }
            return new List<string>(texts);
        }

    }
}
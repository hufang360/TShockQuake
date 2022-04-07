using System;
using Terraria;
using Terraria.Localization;
using TShockAPI;

namespace Quake
{
    public class utils
    {
        public static void ShowText(string text, int color=0xFFFFFF)
        {
            // 0xFF2864
            foreach (TSPlayer op in TShock.Players)
            {
                if( op!=null && op.Active )
                    op.SendData(PacketTypes.CreateCombatTextExtended, text, color, op.TPlayer.position.X, op.TPlayer.position.Y, 0f, 0);
            }
        }

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
    }
}
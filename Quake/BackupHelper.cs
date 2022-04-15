using System;
using System.IO;
using System.Threading;
using Terraria;
using TShockAPI;


namespace Quake
{
    public class BackupHelper
    {
        public static string BackupPath { get; set; }

        public static string BackNotes = "";


        public static void Backup()
        {
            Thread t = new Thread(() =>
            {
                DoBackup(null, BackNotes);
            });
            t.Name = "[quake]Backup Thread";
            t.Start();
        }

        private static void DoBackup(object o, string notes)
        {
            try
            {
                string worldname = Main.worldPathName;
                string name = Path.GetFileName(worldname);

                if (string.IsNullOrEmpty(notes))
                    Main.ActiveWorldFileData._path = Path.Combine(BackupPath, string.Format("{0}.{1:yyyyMMddHHmmss}.bak", name, DateTime.Now));
                else
                    Main.ActiveWorldFileData._path = Path.Combine(BackupPath, string.Format("{0}.{1:yyyyMMddHHmmss}_{2}.bak", name, DateTime.Now, notes));

                string worldpath = Path.GetDirectoryName(Main.worldPathName);
                if (worldpath != null && !Directory.Exists(worldpath))
                    Directory.CreateDirectory(worldpath);

                utils.Log($"正在备份地图...");
                TShock.Utils.SaveWorld();
                utils.Log($"世界已备份 ({Main.worldPathName})");

                string text = string.Format("{0:HH:mm.ss}", DateTime.Now);
                if (string.IsNullOrEmpty(notes))
                    TSPlayer.All.SendInfoMessage($"世界已备份 | {text}");
                else
                    TSPlayer.All.SendInfoMessage($"世界已备份 | {text} | {notes}");
                Main.ActiveWorldFileData._path = worldname;
            }
            catch (Exception ex)
            {
                utils.Log("备份失败!");
                utils.Log(ex.ToString());
            }
        }

    }
}

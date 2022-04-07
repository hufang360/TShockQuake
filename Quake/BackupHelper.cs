using System;
using System.IO;
using System.Threading;
using Terraria;
using TShockAPI;


namespace Quake
{
    public class BackupHelper
    {
        public static void Init()
        {
            BackupPath = Path.Combine(ConfigHelper.SaveDir, "backups");
        }

        private static string BackupPath { get; set; }


        public static void Backup()
        {
            Thread t = new Thread(() =>
            {
                DoBackup(null);
            });
            t.Name = "[quake]Backup Thread";
            t.Start();
        }

        private static void DoBackup(object o)
        {
            try
            {
                string worldname = Main.worldPathName;
                string name = Path.GetFileName(worldname);

                Main.ActiveWorldFileData._path = Path.Combine(BackupPath, string.Format("{0}.{1:yyyy-MM-dd HH.mm.ss}.bak", name, DateTime.Now));

                string worldpath = Path.GetDirectoryName(Main.worldPathName);
                if (worldpath != null && !Directory.Exists(worldpath))
                    Directory.CreateDirectory(worldpath);

                TShock.Log.Info("[quake]正在备份地图...");
                Console.WriteLine("[quake]正在备份地图...");

                TShock.Utils.SaveWorld();

                TShock.Log.Info($"[quake]世界已备份 ({Main.worldPathName})");
                Console.WriteLine($"[quake]世界已备份 ({Main.worldPathName})");

                Main.ActiveWorldFileData._path = worldname;
            }
            catch (Exception ex)
            {
                TShock.Log.Error("[quake]备份失败!");
                Console.WriteLine("[quake]备份失败!");
                TShock.Log.Error(ex.ToString());
                Console.WriteLine(ex.ToString());
            }
        }

    }
}

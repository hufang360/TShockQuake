﻿using System;
using System.IO;
using System.Threading;
using Terraria;
using TShockAPI;

namespace Quake
{
	public class BackupManager
	{
		public string BackupPath { get; set; }
		public int Interval { get; set; }
		public int KeepFor { get; set; }

		private DateTime lastbackup = DateTime.UtcNow;

		public BackupManager(string path)
		{
			BackupPath = path;
		}


		public void Backup()
		{
			lastbackup = DateTime.UtcNow;
			Thread t = new Thread(() => {
				DoBackup(null);
				DeleteOld(null);
			});
			t.Name = "Backup Thread";
			t.Start();

			// ThreadPool.QueueUserWorkItem(DoBackup);
			// ThreadPool.QueueUserWorkItem(DeleteOld);
		}

		private void DoBackup(object o)
		{
			try
			{
				string worldname = Main.worldPathName;
				string name = Path.GetFileName(worldname);

				Main.ActiveWorldFileData._path = Path.Combine(BackupPath, string.Format("{0}.{1:yyyy-MM-ddTHH.mm.ssZ}.bak", name, DateTime.UtcNow));

				string worldpath = Path.GetDirectoryName(Main.worldPathName);
				if (worldpath != null && !Directory.Exists(worldpath))
					Directory.CreateDirectory(worldpath);


				TShock.Log.Info("[quake]正在备份地图...");

                TShock.Utils.SaveWorld();
                Console.WriteLine("World backed up.");
				Console.ForegroundColor = ConsoleColor.Gray;
				TShock.Log.Info(string.Format("World backed up ({0}).", Main.worldPathName));

				Main.ActiveWorldFileData._path = worldname;
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Backup failed!");
				Console.ForegroundColor = ConsoleColor.Gray;
				TShock.Log.Error("Backup failed!");
				TShock.Log.Error(ex.ToString());
			}
		}

		private void DeleteOld(object o)
		{
			if (KeepFor <= 0)
				return;
			foreach (var fi in new DirectoryInfo(BackupPath).GetFiles("*.bak"))
			{
				if ((DateTime.UtcNow - fi.LastWriteTimeUtc).TotalMinutes > KeepFor)
				{
					fi.Delete();
				}
			}
		}
	}
}

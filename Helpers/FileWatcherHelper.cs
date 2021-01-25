using System;
using System.IO;
using System.Threading;
using SRAM.Comparison.Services;

namespace SRAM.Comparison.SoE.Watchdog.Helpers
{
	internal class FileWatcherHelper
	{
		private const int ProcessWaitMiliseconds = 50;

		private static IConsolePrinter ConsolePrinter => ServiceCollection.ConsolePrinter;
		private static DateTime lastReadTime ;
		
		public static FileSystemWatcher StartWatching(IOptions options, WatchOptions watchOptions)
		{
			var filePath = options.CurrentFilePath!;
			var directory = Path.GetDirectoryName(filePath)!;
			var fileName = Path.GetFileName(filePath)!;

			var fileSystemWatcher = new FileSystemWatcher(directory, fileName)
			{
				EnableRaisingEvents = true,
				NotifyFilter = NotifyFilters.LastWrite
			};

			fileSystemWatcher.Changed += (_, _) => OnFileChanged(options, watchOptions);

			CommandHelper.OverwriteComp(options);
			ConsoleHelper.PrintWatchingStarted(options);

			return fileSystemWatcher;
		}

		private static bool IsFileChange(string filePath)
		{
			var lastWriteTime = File.GetLastWriteTime(filePath);
			if ((lastWriteTime - lastReadTime).TotalSeconds <= 1) return false;

			lastReadTime = lastWriteTime;
			return true;
		}

		private static void OnFileChanged(IOptions options, WatchOptions watchOptions)
		{
			if (!IsFileChange(options.CurrentFilePath!)) return;

			ConsoleHelper.PrintFileChanged();

			Thread.Sleep(ProcessWaitMiliseconds);

			try
			{
				// Lock file for writing
				using (File.Open(options.CurrentFilePath!, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					CommandHelper.Compare(options);
					if (watchOptions.AutoExport)
						CommandHelper.Export(options);

					if (watchOptions.AutoOverwrite)
						CommandHelper.OverwriteComp(options);
				}
			}
			catch (Exception ex)
			{
				ConsolePrinter.PrintFatalError(ex.Message);
			}
		}
	}
}
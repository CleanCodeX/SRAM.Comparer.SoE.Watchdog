using System;
using System.IO;
using System.Threading;
using SramComparer.Enums;
using SramComparer.Services;

namespace SramComparer.SoE.FileWatcher.Helpers
{
	internal class FileWatcherHelper
	{
		private static IConsolePrinter ConsolePrinter => ServiceCollection.ConsolePrinter;

		private const int ProcessWaitMiliseconds = 50;

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

			DateTime lastReadTime = default;

			fileSystemWatcher.Changed += (_, _) =>
			{
				var lastWriteTime = File.GetLastWriteTime(filePath);
				if ((lastWriteTime - lastReadTime).TotalSeconds <= 1) return;

				lastReadTime = lastWriteTime;
				ConsolePrinter.PrintSectionHeader();
				ConsolePrinter.PrintColoredLine(ConsoleColor.DarkYellow, "Fille changed. Starting comparison...");

				Thread.Sleep(ProcessWaitMiliseconds);

				try
				{
					using (File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						CommandHelper.RunCommand(nameof(Commands.Compare), options);
						if(watchOptions.AutoExport)
							CommandHelper.RunCommand(nameof(Commands.Export), options);
						OverwriteComparisonFile(options);
					}
				}
				catch (Exception ex)
				{
					ConsolePrinter.PrintFatalError(ex.Message);
				}
			};

			OverwriteComparisonFile(options);

			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, @$"Watching ""{fileName}"" for changes...");
			ConsolePrinter.ResetColor();

			return fileSystemWatcher;
		}

		private static void OverwriteComparisonFile(IOptions options)
		{
			ConsolePrinter.PrintColoredLine(ConsoleColor.DarkYellow, $"Sending {nameof(Commands.OverwriteComp)} command...");
			CommandHelper.RunCommand(nameof(Commands.OverwriteComp), options);
		}
	}
}
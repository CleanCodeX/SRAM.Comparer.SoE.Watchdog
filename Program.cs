using System;
using System.Drawing;
using System.IO;
using System.Threading;
using Common.Shared.Min.Extensions;
using SramComparer.Enums;
using SramComparer.Extensions;
using SramComparer.Helpers;
using SramComparer.Services;
using SramComparer.SoE.Services;

namespace SramComparer.SoE.FileWatcher
{
	internal class Program
	{
		private const int ProcessWaitMiliseconds = 50;

		private static FileSystemWatcher? FileSystemWatcher;
		private static IConsolePrinter ConsolePrinter => ServiceCollection.ConsolePrinter;
		
		private static void Main(string[] args)
		{
			var options = new CmdLineParserSoE().Parse(args);

			Initialize(options);

			PrintParams(args, options);
			PrintHelp();

			StartWatching(options);

			while (true)
			{
				var key = Console.ReadLine()!;
				if (key == "??")
				{
					PrintHelp();
					continue;
				}

				try
				{
					if (!SendCommand(key, options))
						break;
				}
				catch (Exception ex)
				{
					ConsolePrinter.PrintFatalError(ex.Message);
				}
			}

			FileSystemWatcher?.Dispose();
		}

		private static bool SendCommand(string command, IOptions options) => SramComparerApiStarter.RunCommand(command, options);

		private static void StartWatching(IOptions options)
		{
			var filePath = options.CurrentFilePath!;
			var directory = Path.GetDirectoryName(filePath)!;
			var fileName = Path.GetFileName(filePath)!;

			FileSystemWatcher = new FileSystemWatcher(directory, fileName)
			{
				EnableRaisingEvents = true,
				NotifyFilter = NotifyFilters.LastWrite
			};

			DateTime lastReadTime = default;

			FileSystemWatcher.Changed += (_, _) =>
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
						SramComparerApiStarter.RunCommand(nameof(Commands.Compare), options);
				}
				catch (Exception ex)
				{
					ConsolePrinter.PrintFatalError(ex.Message);
				}
			};

			ConsolePrinter.PrintColoredLine(ConsoleColor.DarkYellow, $"Sending {nameof(Commands.OverwriteComp)} command...");
			SramComparerApiStarter.RunCommand(nameof(Commands.OverwriteComp), options);

			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, @$"Watching ""{fileName}"" for changes...");
			ConsolePrinter.ResetColor();
		}

		private static void PrintParams(string[] args, IOptions options)
		{
			ConsolePrinter.PrintSectionHeader("Arguments");
			ConsolePrinter.PrintConfigLine("File to watch", "{0}", @$"""{options.CurrentFilePath}""");
			ConsolePrinter.PrintConfigLine("Other params for SRAM-Comparer", args[1..].Join(" "));
			ConsolePrinter.ResetColor();
		}

		private static void PrintHelp()
		{
			ConsolePrinter.PrintSectionHeader("Commands");
			ConsolePrinter.PrintConfigLine("Quit [Q]", "Quit the app");
			ConsolePrinter.PrintConfigLine("??", "Show this help");
			ConsolePrinter.PrintConfigLine("?", "Show help of SRAM-Comparer");
			ConsolePrinter.PrintConfigLine("Any other key", "Will be passed to SRAM-Comparer");
			ConsolePrinter.PrintLine();
			ConsolePrinter.ResetColor();
		}

		private static void Initialize(IOptions options)
		{
			PaletteHelper.SetScreenColors(Color.White, Color.FromArgb(17, 17, 17));
			Console.Clear();

			if (options.UILanguage is not null)
				CultureHelper.TrySetCulture(options.UILanguage);
		}
	}
}

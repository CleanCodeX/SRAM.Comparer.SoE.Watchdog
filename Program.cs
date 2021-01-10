using System;
using SramComparer.Services;
using SramComparer.SoE.FileWatcher.Helpers;
using SramComparer.SoE.Services;

namespace SramComparer.SoE.FileWatcher
{
	internal class Program
	{
		private static IConsolePrinter ConsolePrinter => ServiceCollection.ConsolePrinter;

		private static readonly WatchOptions WatchOptions = new();

		private static void Main(string[] args)
		{
			var options = new CmdLineParserSoE().Parse(args);

			ConsoleHelper.Initialize(options);
			ConsoleHelper.PrintParams(args, options);
			ConsoleHelper.PrintOptions(WatchOptions);
			ConsoleHelper.PrintHelp();

			using var fileSystemWatcher = FileWatcherHelper.StartWatching(options, WatchOptions);

			while (true)
			{
				var key = Console.ReadLine()!.ToLower();

				try
				{
					switch (key)
					{
						case "??":
							ConsoleHelper.PrintHelp();
							continue;
						case "auto_e":
							WatchOptions.AutoExport = !WatchOptions.AutoExport;
							ConsoleHelper.PrintOption(nameof(WatchOptions.AutoExport), WatchOptions.AutoExport);
							continue;
						case "auto_o":
							WatchOptions.AutoOverwrite = !WatchOptions.AutoOverwrite;
							ConsoleHelper.PrintOption(nameof(WatchOptions.AutoOverwrite), WatchOptions.AutoOverwrite);
							continue;
						default:
							if (!CommandHelper.RunCommand(key, options))
								return;
								
							break;
					}
				}
				catch (Exception ex)
				{
					ConsolePrinter.PrintFatalError(ex.Message);
				}
			}
		}
	}
}

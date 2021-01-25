using System;
using SRAM.Comparison.Services;
using SRAM.Comparison.SoE.Watchdog.Helpers;
using SRAM.Comparison.SoE.Services;

namespace SRAM.Comparison.SoE.Watchdog
{
	internal class Program
	{
		private static IConsolePrinter ConsolePrinter => ServiceCollection.ConsolePrinter;

		private static readonly WatchOptions WatchOptions = new();

		private static void Main(string[] args)
		{
			try
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

								continue;
						}
					}
					catch (Exception ex)
					{
						ConsolePrinter.PrintFatalError(ex.Message);
					}
				}
			}
			catch (Exception ex)
			{
				ConsolePrinter.PrintFatalError(ex.Message);
				Console.ReadKey();
			}
		}
	}
}

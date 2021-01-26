using System;
using System.IO;
using SRAM.Comparison.Helpers;
using SRAM.Comparison.Properties;
using SRAM.Comparison.Services;
using SRAM.Comparison.SoE.Watchdog.Helpers;
using SRAM.Comparison.SoE.Services;
using ConsoleHelper = SRAM.Comparison.SoE.Watchdog.Helpers.ConsoleHelper;
using Common.Shared.Min.Extensions;

namespace SRAM.Comparison.SoE.Watchdog
{
	internal class Program
	{
		private static readonly string DefaultConfigFileName = CommandHandler.DefaultConfigFileName;
		private static readonly WatchOptions WatchOptions = new();

		private static IConsolePrinter ConsolePrinter => ServiceCollection.ConsolePrinter;
		
		private static void Main(string[] args)
		{
			try
			{
				IOptions options = null!;
				string? configToLoad = null;
				CmdLineParserSoE cmdParser = new();

				if (File.Exists(DefaultConfigFileName))
					configToLoad = DefaultConfigFileName;
				else
				{
					options = cmdParser.Parse(args);

					if (options.ConfigPath is { } configFile)
						configToLoad = configFile;
				}

				if (configToLoad is not null)
				{
					var loadedConfig = JsonFileSerializer.Deserialize<Options>(configToLoad);
					options = cmdParser.Parse(args, loadedConfig);
				}

				ConsoleHelper.Initialize(options);

				if (configToLoad is not null)
				{
					ConsolePrinter.PrintSectionHeader();
					ConsolePrinter.PrintColoredLine(ConsoleColor.Green,
						Resources.StatusConfigFileLoadedTemplate.InsertArgs(configToLoad));
					ConsolePrinter.ResetColor();
				}

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

		private static IOptions GetDefaultConfigOrNew() => File.Exists(DefaultConfigFileName)
			? JsonFileSerializer.Deserialize<Options>(DefaultConfigFileName)
			: new Options();
	}
}

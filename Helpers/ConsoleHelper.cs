using System;
using System.Drawing;
using Common.Shared.Min.Extensions;
using SramComparer.Extensions;
using SramComparer.Helpers;
using SramComparer.Services;

namespace SramComparer.SoE.FileWatcher.Helpers
{
	internal class ConsoleHelper
	{
		private static IConsolePrinter ConsolePrinter => ServiceCollection.ConsolePrinter;

		public static void Initialize(IOptions options)
		{
			PaletteHelper.SetScreenColors(Color.White, Color.FromArgb(17, 17, 17));
			Console.Clear();

			if (options.UILanguage is not null)
				CultureHelper.TrySetCulture(options.UILanguage);
		}

		public static void PrintParams(string[] args, IOptions options)
		{
			ConsolePrinter.PrintSectionHeader("Arguments");
			ConsolePrinter.PrintConfigLine("File to watch", "{0}", @$"""{options.CurrentFilePath}""");
			ConsolePrinter.PrintConfigLine("Other params for SRAM-Comparer", args[1..].Join(" "));
			ConsolePrinter.ResetColor();
		}

		public static void PrintHelp()
		{
			ConsolePrinter.PrintSectionHeader("Commands");
			ConsolePrinter.PrintConfigLine("Quit [Q]", "Quit the app");
			ConsolePrinter.PrintConfigLine("??", "Show this help");
			ConsolePrinter.PrintConfigLine("?", "Show help of SRAM-Comparer");
			ConsolePrinter.PrintConfigLine("Auto", "Enable / disable Auto-export compararison result: default is false");
			ConsolePrinter.PrintConfigLine("Any other key", "Will be passed to SRAM-Comparer");
			ConsolePrinter.PrintLine();
			ConsolePrinter.ResetColor();
		}

		public static void PrintOption(string name, bool value)
		{
			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.PrintConfigLine(name, value.ToString());
			ConsolePrinter.PrintLine();
			ConsolePrinter.ResetColor();
		}

		public static void PrintOptions(WatchOptions options)
		{
			ConsolePrinter.PrintSectionHeader("Options");
			ConsolePrinter.PrintConfigLine(nameof(options.AutoExport), options.AutoExport.ToString());
			ConsolePrinter.PrintConfigLine(nameof(options.AutoOverwrite), options.AutoOverwrite.ToString());
			ConsolePrinter.ResetColor();
		}
	}
}
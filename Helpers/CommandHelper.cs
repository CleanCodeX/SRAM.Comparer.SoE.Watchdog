using System;
using SramComparer.Enums;
using SramComparer.Services;

namespace SramComparer.SoE.FileWatcher.Helpers
{
	internal class CommandHelper
	{
		private static ICommandHandler CommandHandler => ServiceCollection.CommandHandler!;
		private static IConsolePrinter ConsolePrinter => ServiceCollection.ConsolePrinter;

		public static void Compare(IOptions options) => RunCommand(nameof(Commands.Compare), options);
		public static void Export(IOptions options) => RunCommand(nameof(Commands.Export), options);
		public static void OverwriteComp(IOptions options)
		{
			ConsolePrinter.PrintColoredLine(ConsoleColor.DarkYellow, $"Sending {nameof(Commands.OverwriteComp)} command...");
			RunCommand(nameof(Commands.OverwriteComp), options);
		}

		public static bool RunCommand(string command, IOptions options) => CommandHandler.RunCommand(command, options, Console.Out);
	}
}
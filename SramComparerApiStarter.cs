using System;
using SramComparer.Services;

namespace SramComparer.SoE.FileWatcher
{
	internal class SramComparerApiStarter
	{
		private static ICommandHandler CommandHandler => ServiceCollection.CommandHandler!;

		internal static bool RunCommand(string command, IOptions options) => CommandHandler.RunCommand(command, options, Console.Out);
	}
}
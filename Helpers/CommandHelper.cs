using System;
using SramComparer.Services;

namespace SramComparer.SoE.FileWatcher.Helpers
{
	internal class CommandHelper
	{
		private static ICommandHandler CommandHandler => ServiceCollection.CommandHandler!;

		public static bool RunCommand(string command, IOptions options) => CommandHandler.RunCommand(command, options, Console.Out);
	}
}
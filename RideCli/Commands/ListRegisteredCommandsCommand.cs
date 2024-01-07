using Spectre.Console;
using Spectre.Console.Cli;

using System.Diagnostics.CodeAnalysis;

namespace RideCli.Commands;

internal sealed class ListRegisteredCommandsSettings : CommandSettings{
	[CommandArgument(0, "[type]")] public ManagedCommands ManagedCommands { get; set; } = ManagedCommands.All;
}

internal sealed class ListRegisteredCommandsCommand : Command<ListRegisteredCommandsSettings> {
	public override int Execute([NotNull] CommandContext context, [NotNull] ListRegisteredCommandsSettings settings) {
		var appSettings = Settings.GetSettings();

		if (settings.ManagedCommands is ManagedCommands.All or ManagedCommands.Launch) {
			AnsiConsole.WriteLine("Launch commands:");
			
			foreach (var (alias, setting) in appSettings.LaunchSettings)
            {
				AnsiConsole.WriteLine($"""ride reg launch {alias} "{setting.Launcher}" "{setting.Arguments}" "{(setting.WorkDirectory is {Length: >0} workdir ? $"-wd {workdir}" : "")}" {(setting.AsAdmin ? "-adm" : "")}""");
            }
        }
		if (settings.ManagedCommands is ManagedCommands.All or ManagedCommands.Search) {
			AnsiConsole.WriteLine("Search commands:");

			foreach (var (alias, kindPath) in appSettings.KindPaths) {
				AnsiConsole.WriteLine($"""ride reg search{(appSettings.SelectedKindPath == kindPath ? " -d" : "")} {alias} "{kindPath}" """);
			}
		}

		return 0;
	}
}

internal enum ManagedCommands { All, Launch, Search }
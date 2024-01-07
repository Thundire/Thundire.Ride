using Spectre.Console.Cli;

using System.Diagnostics.CodeAnalysis;

namespace RideCli.Commands;

internal sealed class OpenSettingsCommandSettings : CommandSettings {

}

internal class OpenSettingsCommand : Command<OpenSettingsCommandSettings> {
	public override int Execute([NotNull] CommandContext context, [NotNull] OpenSettingsCommandSettings settings) {
		var appSettingsPath = Settings.SettingsPath;

		ProcessBuilder.Open(appSettingsPath);
		return 0;
	}
}

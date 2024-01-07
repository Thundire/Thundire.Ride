using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;

namespace RideCli.Commands;

internal class RegisterPathSettings : CommandSettings
{
    [CommandArgument(0, "<alias>")] public string Alias { get; set; } = string.Empty;
}

internal sealed class RegisterLauncherSettings : RegisterPathSettings {
	[CommandArgument(1, "<launcher>")] public string Launcher { get; set; } = string.Empty;
    [CommandArgument(2, "[arguments]")] public string Arguments { get; set; } = string.Empty;
	[CommandOption("-wp | --workdir")] public string? WorkDirectory { get; set; }
	[CommandOption("-adm | --admin")] public bool? AsAdmin { get; set; }
}

internal sealed class RegisterKindPathSettings : RegisterPathSettings {
	[CommandArgument(1, "<path>")] public string Path { get; set; } = string.Empty;
	[CommandOption("-d | --default")] public bool? AsDefault { get; set; }
}

internal sealed class RegisterLauncherCommand : Command<RegisterLauncherSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] RegisterLauncherSettings settings)
    {
        try
        {
            AppSettings appSettings = Settings.GetSettings();

            LaunchSetting setting = new (settings.Launcher, settings.Arguments) {
                WorkDirectory = settings.WorkDirectory ?? string.Empty,
                AsAdmin = settings.AsAdmin ?? false
			};
            
			appSettings.Register(settings.Alias, setting);
            Settings.Save();
        }
        catch
        {
            return -1;
        }

        return 0;
    }
}
internal sealed class RegisterKindPathCommand : Command<RegisterKindPathSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] RegisterKindPathSettings settings)
    {
        try
        {
            AppSettings appSettings = Settings.GetSettings();
            appSettings.Register(settings.Alias, settings.Path);
            if (settings.AsDefault is true)
            {
                appSettings.ChangeSelectedKindPath(settings.Alias);
            }
            Settings.Save();
        }
        catch
        {
            return -1;
        }

        return 0;
    }
}
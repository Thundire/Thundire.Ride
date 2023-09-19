using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;

namespace RideCli.Commands;

public class RegisterPathSettings : CommandSettings
{
    [CommandArgument(0, "[alias]")] public string Alias { get; set; } = string.Empty;
    [CommandArgument(1, "[path]")] public string Path { get; set; } = string.Empty;
}

public class RegisterLauncherSettings : RegisterPathSettings
{
    [CommandArgument(2, "[launcher]")] public string Launcher { get; set; } = string.Empty;
    [CommandArgument(3, "[arguments]")] public string Arguments { get; set; } = string.Empty;
}

public class RegisterKindPathSettings : RegisterPathSettings
{
    [CommandOption("-d | --default")] public bool? AsDefault { get; set; }
}

public class RegisterLauncherCommand : Command<RegisterLauncherSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] RegisterLauncherSettings settings)
    {
        try
        {
            AppSettings appSettings = Settings.GetSettings();
            appSettings.Register(settings.Alias, new LaunchSetting(settings.Path, settings.Launcher, settings.Arguments));
            Settings.Save();
        }
        catch
        {
            return -1;
        }

        return 0;
    }
}
public class RegisterKindPathCommand : Command<RegisterKindPathSettings>
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
using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;

namespace RideCli.Commands;

public class RegisterSettings : CommandSettings
{
    [CommandArgument(0, "[alias]")] public string Alias { get; set; } = string.Empty;
    [CommandArgument(1, "[path]")] public string Path { get; set; } = string.Empty;

    [CommandArgument(2, "[launcher]")] public string Launcher { get; set; } = string.Empty;
    [CommandArgument(3, "[arguments]")] public string Arguments { get; set; } = string.Empty;
}

public class RegisterCommand : Command<RegisterSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] RegisterSettings settings)
    {
        try
        {
            AppSettings appSettings = Settings.GetSettings();
            appSettings.Register(settings.Alias, new(settings.Path, settings.Launcher, settings.Arguments));
            Settings.Save(appSettings);
        }
        catch
        {
            return -1;
        }

        return 0;
    }
}
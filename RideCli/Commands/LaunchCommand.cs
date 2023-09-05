using CliWrap;
using Spectre.Console.Cli;

namespace RideCli.Commands;

public class LaunchSettings : CommandSettings
{
    [CommandArgument(0, "[alias]")] public string Alias { get; set; } = string.Empty;
}

public class LaunchCommand : AsyncCommand<LaunchSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, LaunchSettings settings)
    {
        AppSettings appSettings = Settings.GetSettings();
        if (appSettings.Get(settings.Alias) is not { } launchSetting) return -2;
        await Cli.Wrap(launchSetting.Launcher).WithWorkingDirectory(launchSetting.Path).WithArguments(launchSetting.Arguments)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(Spectre.Console.AnsiConsole.WriteLine)).ExecuteAsync();
        return 0;
    }
}
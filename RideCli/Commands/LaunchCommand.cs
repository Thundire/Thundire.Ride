using CliWrap;

using Spectre.Console;
using Spectre.Console.Cli;
using Command = CliWrap.Command;

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
        Command cmd = Cli.Wrap(launchSetting.Launcher).WithWorkingDirectory(launchSetting.Path).WithArguments(launchSetting.Arguments) | AnsiConsole.WriteLine;
	    await cmd.ExecuteAsync(Settings.CancellationTokenSource.Token);
        return 0;
    }
}
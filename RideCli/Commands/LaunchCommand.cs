﻿using Spectre.Console;
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
        ProcessBuilder cmd = ProcessBuilder.Create(launchSetting.Launcher).WithArgument(launchSetting.Arguments);
        if (launchSetting.WorkDirectory is { Length: > 0 }) cmd.WithWorkingDirectory(launchSetting.WorkDirectory);
        if (launchSetting.AsAdmin) cmd.AsAdmin();
        cmd  |= AnsiConsole.WriteLine;
	    await cmd.Execute(Settings.CancellationTokenSource.Token);
        return 0;
    }
}
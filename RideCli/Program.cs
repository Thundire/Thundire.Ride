
using Spectre.Console.Cli;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

if (!Directory.Exists(Settings.ThundirePath)) Directory.CreateDirectory(Settings.ThundirePath);
if (!Directory.Exists(Settings.LauncherPath)) Directory.CreateDirectory(Settings.LauncherPath);
if (!File.Exists(Settings.SettingsPath)) File.Create(Settings.SettingsPath).Dispose();

var app = new CommandApp();
app.Configure(c =>
{
	c.AddCommand<RegisterCommand>("reg");
});
await app.RunAsync(args);

public static class Settings
{
	public static readonly string ThundirePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Thundire");
	public static readonly string LauncherPath = Path.Combine(ThundirePath, "RideCli");
	public static readonly string SettingsPath = Path.Combine(LauncherPath, "Settings.json");

	public static AppSettings GetSettings()
	{
		var data = File.ReadAllText(SettingsPath);
		AppSettings? appSettings = null;
		if (data is { Length: > 2 })
		{
			appSettings = JsonSerializer.Deserialize<AppSettings>(data);
		}
		return appSettings ?? new AppSettings();
	}

	public static void Save(AppSettings paths)
	{
		File.WriteAllText(SettingsPath, JsonSerializer.Serialize(paths));
	}
}

public class AppSettings
{
	public Dictionary<string, LaunchSetting> LaunchSettings { get; set; } = new();

	public void Register(string alias, LaunchSetting settings)
	{
		if (LaunchSettings.ContainsKey(alias))
		{
			LaunchSettings[alias] = settings;
			return;
		}
		LaunchSettings.TryAdd(alias, settings);
	}

	public LaunchSetting? Get(string alias)
	{
		if (LaunchSettings.TryGetValue(alias, out var setting)) return setting;
		return default;
	}
}

public record LaunchSetting(string Path, string Launcher, string Arguments);

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
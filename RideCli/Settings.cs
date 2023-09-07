using System.Text.Json;

namespace RideCli;

public static class Settings
{
	public static readonly string ThundirePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Thundire");
	public static readonly string LauncherPath = Path.Combine(ThundirePath, "RideCli");
	public static readonly string SettingsPath = Path.Combine(LauncherPath, "Settings.json");

	public static readonly CancellationTokenSource CancellationTokenSource = new();
	public static bool CancellationDisposed { get; private set; }
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

	public static void StopProcesses()
	{
		if(CancellationDisposed) return;
		CancellationDisposed = true;
		if (!CancellationTokenSource.IsCancellationRequested)
			CancellationTokenSource.Cancel();
		CancellationTokenSource.Dispose();
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
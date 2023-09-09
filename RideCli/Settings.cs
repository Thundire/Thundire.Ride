using System.Collections.Generic;
using System.Text.Json;

namespace RideCli;

public static class Settings
{
	public static readonly string ThundirePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Thundire");
	public static readonly string LauncherPath = Path.Combine(ThundirePath, "RideCli");
	public static readonly string SettingsPath = Path.Combine(LauncherPath, "Settings.json");

	public static readonly CancellationTokenSource CancellationTokenSource = new();
	public static bool CancellationDisposed { get; private set; }

	private static AppSettings? AppSettings { get; set; }

	public static AppSettings GetSettings()
	{
		if (AppSettings is not null) return AppSettings;
		var data = File.ReadAllText(SettingsPath);
		if (data is { Length: > 2 })
		{
			AppSettings = JsonSerializer.Deserialize<AppSettings>(data);
		}
		return AppSettings ?? new AppSettings();
	}

	public static void Save()
	{
		if(AppSettings is not null)
			File.WriteAllText(SettingsPath, JsonSerializer.Serialize(AppSettings));
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
	public Dictionary<string, string> KindPaths { get; set; } = new();
	public string SelectedKindPath { get; set; } = string.Empty;

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

	public string? GetKindPath(string alias)
	{
		if (KindPaths.TryGetValue(alias, out var setting)) return setting;
		return default;
	}

	public void Register(string alias, string kindPath)
	{
		if (KindPaths.ContainsKey(alias))
		{
			KindPaths[alias] = kindPath;
			return;
		}
		KindPaths.Add(alias, kindPath);
		if (KindPaths.Count == 1)
		{
			SelectedKindPath = kindPath;
		}
	}

	public (string alias, string path)[] ListKindPaths() => KindPaths.Select(x=>(alias:x.Key, path: x.Value)).ToArray();
	public string SelectedKindPaths() => SelectedKindPath;
	public void ChangeSelectedKindPath(string alias)
	{
		if(KindPaths.TryGetValue(alias, out var path)) SelectedKindPath = path;
	}
}

public record LaunchSetting(string Path, string Launcher, string Arguments);
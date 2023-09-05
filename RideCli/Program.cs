using Spectre.Console.Cli;
using RideCli;
using RideCli.Commands;

if (!Directory.Exists(Settings.ThundirePath)) Directory.CreateDirectory(Settings.ThundirePath);
if (!Directory.Exists(Settings.LauncherPath)) Directory.CreateDirectory(Settings.LauncherPath);
if (!File.Exists(Settings.SettingsPath)) File.Create(Settings.SettingsPath).Dispose();

var app = new CommandApp();
app.Configure(c =>
{
	c.AddCommand<RegisterCommand>("reg");
	c.AddCommand<LaunchCommand>("launch");
});
await app.RunAsync(args);

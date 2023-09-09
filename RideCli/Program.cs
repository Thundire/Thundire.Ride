using Spectre.Console.Cli;
using RideCli;
using RideCli.Commands;

if (!Directory.Exists(Settings.ThundirePath)) Directory.CreateDirectory(Settings.ThundirePath);
if (!Directory.Exists(Settings.LauncherPath)) Directory.CreateDirectory(Settings.LauncherPath);
if (!File.Exists(Settings.SettingsPath)) File.Create(Settings.SettingsPath).Dispose();

AppDomain.CurrentDomain.ProcessExit += OnExit;
Console.CancelKeyPress              += OnExit;

var app = new CommandApp();
app.Configure(c =>
{
	c.AddBranch("reg", b =>
	{
		b.AddCommand<RegisterLauncherCommand>("launch");
		b.AddCommand<RegisterKindPathCommand>("kind-path");
	});
	
	c.AddCommand<LaunchCommand>("launch");
	c.AddCommand<SearchDirectoryCommand>("search");
});
return await app.RunAsync(args);

static void OnExit(object? sender, EventArgs e)
{
	Settings.StopProcesses();
	if (e is ConsoleCancelEventArgs consoleCancel)
	{
		consoleCancel.Cancel = true;
	}
}

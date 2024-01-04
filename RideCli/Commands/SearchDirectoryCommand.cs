using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using Spectre.Console;
using Spectre.Console.Cli;

namespace RideCli.Commands;


internal sealed class SearchDirectorySettings : CommandSettings
{
	[CommandOption("-k | --kind-path")] public string? KindPath { get; set; }
	[CommandOption("-d | --default")] public bool? AsDefault { get; set; }
	[CommandOption("-e | --extended")] public bool? ExtendedPrimarySearch { get; set; }

	[CommandArgument(0, "[search]")]
	public string SearchPattern { get; set; } = string.Empty;

	[CommandArgument(1, "[sub-search]")]
	public string SubSearchPattern { get; set; } = string.Empty;
}
internal sealed class SearchDirectoryCommand : Command<SearchDirectorySettings>
{
	private string? KindPath { get; set; }
	private const string _exitWord = "None";

	public override int Execute([NotNull] CommandContext context, [NotNull] SearchDirectorySettings settings)
	{
		AppSettings appSettings = Settings.GetSettings();
		if (settings.KindPath is { Length: > 0 })
		{
			KindPath = appSettings.GetKindPath(settings.KindPath);
			if (KindPath is not null && settings.AsDefault is true)
			{
				appSettings.ChangeSelectedKindPath(settings.KindPath);
				Settings.Save();
			}
		}
		else KindPath = appSettings.SelectedKindPaths();

		string searchPattern = settings.SearchPattern.ToLowerInvariant();
		string subSearchPattern = settings.SubSearchPattern.ToLowerInvariant();

		string rootPath = KindPath ?? string.Empty;
		if (!Directory.Exists(rootPath)) {
			AnsiConsole.WriteLine("Директория поиска не найдена");
			return 1;
		}

		DirectoryInfo root = new(rootPath);
		Result<string> result = string.IsNullOrEmpty(subSearchPattern) && settings.ExtendedPrimarySearch is true 
			? FindDirectoryExtendedPrimarySearch(root, searchPattern) 
			: FindDirectory(root, searchPattern, subSearchPattern);

		if (result.IsSuccess && result.Data != _exitWord) Open(result.Data!);

		return result.ExitCode;
	}

	private static Result<string> FindDirectory(DirectoryInfo root, string searchPattern, string subSearchPattern)
	{
		var directories = DirectoryBrowser.Directories(root, searchPattern);

		if (directories.Length == 0)
		{
			AnsiConsole.WriteLine("Искомая поддиректория не найдена");
			return ResultFactory.Failure<string>(2);
		}

		var matched = directories.Select(x => x.FullName).ToList();

		if (!string.IsNullOrWhiteSpace(subSearchPattern))
		{
			matched.Clear();
			foreach (DirectoryInfo directory in directories)
			{
				var subDirectories = DirectoryBrowser.FindSubdirectories(directory, subSearchPattern).Select(x => x.FullName);
				matched.AddRange(subDirectories);
			}
		}

		return HandleMatches(matched);
	}
	
	private static Result<string> FindDirectoryExtendedPrimarySearch(DirectoryInfo root, string searchPattern)
	{
		var matched = DirectoryBrowser.FindSubdirectories(root, searchPattern).Select(x => x.FullName).ToList();

		return HandleMatches(matched);
	}

	private static Result<string> HandleMatches(List<string> matched) {
		if (matched.Count == 0) {
			AnsiConsole.WriteLine("Искомая поддиректория не найдена");
			return ResultFactory.Failure<string>(2);
		}
		else if (matched.Count == 1) {
			return ResultFactory.Success(matched[0]);
		}
		matched.Insert(0, _exitWord);
		var selectedPath = AnsiConsole.Prompt(new SelectionPrompt<string>()
			.Title("Найдено несколько директорий, какая ваша?")
			.PageSize(10)
			.MoreChoicesText("[grey](Передвигайте курсор вверх и вниз чтобы увидеть больше каталогов)[/]")
			.AddChoices(matched));

		return ResultFactory.Success(selectedPath);
	}

	public override ValidationResult Validate([NotNull] CommandContext context, [NotNull] SearchDirectorySettings settings) =>
		settings.SearchPattern is { Length: > 0 }
			? ValidationResult.Success()
			: ValidationResult.Error("Не указан поисковой паттерн");

	private static void Open(string path) => Process.Start("explorer", path).Dispose();
}

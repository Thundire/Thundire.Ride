using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace RideCli.Commands;


public class SearchDirectorySettings : CommandSettings
{
	[CommandOption("-k | --kind-path")] public string? KindPath { get; set; }
	[CommandOption("-d | --default")] public bool? AsDefault { get; set; }

	[CommandArgument(0, "[search]")]
	public string SearchPattern { get; set; } = string.Empty;

	[CommandArgument(1, "[details-search]")]
	public string DetailsSearch { get; set; } = string.Empty;
}
internal class SearchDirectoryCommand : Command<SearchDirectorySettings>
{
	private string? KindPath { get; set; }

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

		string organization = settings.SearchPattern.ToLowerInvariant();
		string searchDirectory = settings.DetailsSearch.ToLowerInvariant();

		Result<string> result = FindDirectory(organization, searchDirectory);

		if (result.IsSuccess) Open(result.Data!);

		return result.ExitCode;
	}

	private Result<string> FindDirectory(string organization, string searchDirectory)
	{
		string path = KindPath ?? string.Empty;
		if (!Directory.Exists(path))
		{
			AnsiConsole.WriteLine("Директория поиска не найдена");
			return ResultFactory.Failure<string>(1);
		}

		DirectoryInfo dir = new(path);
		var directories = DirectoryBrowser.Directories(path, organization);

		if (directories.Length == 0)
		{
			AnsiConsole.WriteLine("Искомая поддиректория не найдена");
			return ResultFactory.Failure<string>(2);
		}

		var data = directories.Select(x => x.FullName).ToList();

		if (!string.IsNullOrWhiteSpace(searchDirectory))
		{
			data.Clear();
			foreach (DirectoryInfo directory in directories)
			{
				var matched = DirectoryBrowser.FindSubdirectories(directory, searchDirectory);
				data.AddRange(matched);
			}
		}

		if (data.Count == 0)
		{
			AnsiConsole.WriteLine("Искомая поддиректория не найдена");
			return ResultFactory.Failure<string>(2);
		}

		var selectedPath = AnsiConsole.Prompt(new SelectionPrompt<string>()
			.Title("Найдено несколько директорий, какая ваша?")
			.PageSize(10)
			.MoreChoicesText("[grey](Передвигайте курсор вверх и вниз чтобы увидеть больше каталогов)[/]")
			.AddChoices(data));

		return ResultFactory.Success(selectedPath);
	}

	public override ValidationResult Validate([NotNull] CommandContext context, [NotNull] SearchDirectorySettings settings) =>
		settings.SearchPattern is { Length: > 0 }
			? ValidationResult.Success()
			: ValidationResult.Error("Не указан поисковой паттерн");

	private static void Open(string path) => Process.Start("explorer", path).Dispose();
}

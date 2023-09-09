using System.Text.RegularExpressions;

namespace RideCli;
public class DirectoryBrowser
{
	public static DirectoryInfo[] Directories(string root, string searchPattern)
	{
		return new DirectoryInfo(root).EnumerateDirectories().Where(d => d.Name.ToLowerInvariant().Contains(searchPattern)).Where(x =>
		{
			try
			{
				_ = x.GetDirectories();
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}).ToArray();
	}

	public static IEnumerable<string> FindSubdirectories(DirectoryInfo root, string searchDirectory)
	{
		var directoryPrefixLength = root.FullName.Length;
		List<string> paths = new();
		var enumerator = root.EnumerateDirectories("*.*", SearchOption.AllDirectories).GetEnumerator();

		while (true)
		{
			try
			{
				if (!enumerator.MoveNext()) break;
				paths.Add(enumerator.Current.FullName[directoryPrefixLength..]);
			}
			catch (Exception)
			{
				// ignore
			}
		}
		enumerator.Dispose();

		var stringDirectories = string.Join("\n", paths);
		Regex regex = new($"^.+\\.*{searchDirectory}.*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
		return regex.Matches(stringDirectories).Select(x => root.FullName + x.Value);
	}
}

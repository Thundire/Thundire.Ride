using System.Text.RegularExpressions;

namespace RideCli;
public class DirectoryBrowser
{
	public static DirectoryInfo[] Directories(string root, string searchPattern)
	{
		return new DirectoryInfo(root).EnumerateDirectories().Where(d => d.Name.Contains(searchPattern, StringComparison.InvariantCultureIgnoreCase)).Where(x =>
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
		List<string> paths = new();
		var enumerator = root.EnumerateDirectories("*.*", SearchOption.AllDirectories).GetEnumerator();

		while (true)
		{
			try
			{
				if (!enumerator.MoveNext()) break;
				var directory = enumerator.Current;
				if (directory.Name.Contains(searchDirectory, StringComparison.InvariantCultureIgnoreCase))
					paths.Add(directory.FullName);
			}
			catch (Exception)
			{
				// ignore
			}
		}
		enumerator.Dispose();
		
		return paths;
	}
}

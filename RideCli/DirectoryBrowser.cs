namespace RideCli;
internal static class DirectoryBrowser
{
	public static DirectoryInfo[] Directories(DirectoryInfo root, string searchPattern)
	{
		return root.EnumerateDirectories().Where(d => d.Name.Contains(searchPattern, StringComparison.InvariantCultureIgnoreCase)).Where(x =>
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

	public static IEnumerable<DirectoryInfo> FindSubdirectories(DirectoryInfo root, string searchDirectory)
	{
		var enumerator = root.EnumerateDirectories("*.*", SearchOption.AllDirectories).GetEnumerator();
		DirectoryInfo? directory;
		while (true)
		{
			directory = null;
			try
			{
				if (!enumerator.MoveNext()) break;
				directory = enumerator.Current;
				
			}
			catch (Exception)
			{
				// ignore
			}

			if (directory?.Name.Contains(searchDirectory, StringComparison.InvariantCultureIgnoreCase) is true)
				yield return directory;
		}
		enumerator.Dispose();
	}
}

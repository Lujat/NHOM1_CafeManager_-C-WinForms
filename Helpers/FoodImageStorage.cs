namespace CafeManager.Helpers;

public static class FoodImageStorage
{
    const string ProjectImageFolder = "Resources/FoodImages";
    const string AppDataImagePrefix = "@appdata/Images/";
    public static string CopyToLibrary(string sourcePath, string foodName)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            throw new FileNotFoundException("Không tìm thấy file ảnh đã chọn.", sourcePath);

        string extension = Path.GetExtension(sourcePath).ToLowerInvariant();
        string fileName = $"{CreateSlug(foodName)}-{Guid.NewGuid():N}{extension}";
        string projectRoot = FindProjectRoot();

        if (projectRoot != null)
        {
            string directory = Path.Combine(projectRoot, "Resources", "FoodImages");
            Directory.CreateDirectory(directory);
            File.Copy(sourcePath, Path.Combine(directory, fileName), overwrite: false);
            return $"{ProjectImageFolder}/{fileName}";
        }

        string appDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CafeManager", "Images");
        Directory.CreateDirectory(appDataDirectory);
        File.Copy(sourcePath, Path.Combine(appDataDirectory, fileName), overwrite: false);
        return AppDataImagePrefix + fileName;
    }
    public static Image LoadImage(string path)
    {
        string resolvedPath = ResolvePath(path);
        if (string.IsNullOrWhiteSpace(resolvedPath) || !File.Exists(resolvedPath)) return null;

        using var stream = new FileStream(resolvedPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var source = Image.FromStream(stream);
        return new Bitmap(source);
    }
    static string ResolvePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        if (Path.IsPathRooted(path)) return path;

        if (path.StartsWith(AppDataImagePrefix, StringComparison.OrdinalIgnoreCase))
        {
            string fileName = path[AppDataImagePrefix.Length..].Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CafeManager", "Images", fileName);
        }

        string root = FindProjectRoot() ?? AppContext.BaseDirectory;
        return Path.GetFullPath(Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar)));
    }

    static string FindProjectRoot()
    {
        for (DirectoryInfo directory = new(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "CafeManager.csproj")))
                return directory.FullName;
        }

        return null;
    }
    static string CreateSlug(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "food";

        string normalized = value.Trim().ToLowerInvariant()
            .Replace('đ', 'd')
            .Normalize(System.Text.NormalizationForm.FormD);
        var result = new System.Text.StringBuilder(normalized.Length);
        bool lastWasSeparator = false;

        foreach (char character in normalized)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(character) ==
                System.Globalization.UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(character))
            {
                result.Append(character);
                lastWasSeparator = false;
            }
            else if (!lastWasSeparator && result.Length > 0)
            {
                result.Append('-');
                lastWasSeparator = true;
            }
        }

        return result.ToString().Trim('-') is { Length: > 0 } slug ? slug : "food";
    }
}

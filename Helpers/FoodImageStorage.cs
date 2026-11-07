namespace CafeManager.Helpers;

public static class FoodImageStorage
{
    public static string CopyToLibrary(string sourcePath)
    {
        string extension = Path.GetExtension(sourcePath);
        string directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CafeManager", "Images");
        Directory.CreateDirectory(directory);

        string destination = Path.Combine(directory, $"{Guid.NewGuid():N}{extension}");
        File.Copy(sourcePath, destination, overwrite: false);
        return destination;
    }

    public static Image LoadImage(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var source = Image.FromStream(stream);
        return new Bitmap(source);
    }
}

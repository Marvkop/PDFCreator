using System.IO;
using PDFCreatorUpdater.Config;

namespace PDFCreatorUpdater.Service;

public class FileService
{
    public static string SaveToFile(Stream stream, string name)
    {
        var programsFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var installPath = $"{programsFolder}\\{PdfCreatorConfig.ProjectName}";

        Directory.CreateDirectory(installPath);

        var filePath = $"{installPath}\\{name}";
        using var fileStream = File.Create(filePath);

        stream.CopyTo(fileStream);
        stream.Flush();

        return filePath;
    }

    public static string GetInstallationFolderPathForFile(string fileName)
    {
        return $"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\\{PdfCreatorConfig.ProjectName}\\{fileName}";
    }
}
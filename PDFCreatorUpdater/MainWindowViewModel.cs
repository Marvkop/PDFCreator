using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDFCreatorUpdater.Config;
using PDFCreatorUpdater.Service;

namespace PDFCreatorUpdater;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly GitHubService _gitHubService = new();

    [ObservableProperty]
    private ObservableCollection<string> _createdFiles = [];

    [RelayCommand]
    private async Task Initialize()
    {
        await CheckForUpdate();
    }

    [RelayCommand]
    private async Task CheckForUpdate()
    {
        var (assetsUrl, name, created) = await _gitHubService.GetLatestRelease("Marvkop", "PDFCreator");

        var versionFilePath = FileService.GetInstallationFolderPathForFile(PdfCreatorConfig.VersionFileName);

        if (!File.Exists(versionFilePath))
        {
            await Install(assetsUrl, name, created);
        }
        else
        {
            var (releaseName, releaseDate) = ReadVersionFile();

            if (releaseName != name || releaseDate != created)
            {
                await Install(assetsUrl, name, created);
            }
        }

        Process.Start(FileService.GetInstallationFolderPathForFile($"{PdfCreatorConfig.ProjectName}.exe"));
    }

    private async Task Install(string assetsUrl, string releaseName, string releaseDate)
    {
        var files = await _gitHubService.Download(assetsUrl);

        foreach (var file in files)
            CreatedFiles.Add(file);

        await CreateVersionFile(releaseName, releaseDate);
    }

    private (string ReleaseName, string ReleaseDate) ReadVersionFile()
    {
        var versionFileLines = File.ReadAllLines(FileService.GetInstallationFolderPathForFile(PdfCreatorConfig.VersionFileName));

        if (versionFileLines.Length is not 2)
            throw new InvalidOperationException("version file currupted.");

        return (versionFileLines[0], versionFileLines[1]);
    }

    private async Task CreateVersionFile(string releaseName, string releaseDate)
    {
        await using var memoryStream = new MemoryStream();
        await using var writer = new StreamWriter(memoryStream, Encoding.UTF8);

        await writer.WriteLineAsync(releaseName);
        await writer.WriteLineAsync(releaseDate);

        memoryStream.Seek(0, SeekOrigin.Begin);
        FileService.SaveToFile(memoryStream, PdfCreatorConfig.VersionFileName);
    }
}
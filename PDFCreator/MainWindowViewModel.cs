using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing.Imaging;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.Win32;
using PDFCreator.Files;

namespace PDFCreator;

public partial class MainWindowViewModel : ObservableObject
{
    private static readonly string ImageFilter = string.Join("|", CreateImageFilters());

    public MainWindowViewModel()
    {
        SelectedFiles.CollectionChanged += SelectedFilesOnCollectionChanged;
    }

    private void SelectedFilesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(SelectedFiles));
    }

    [NotifyCanExecuteChangedFor(nameof(MergeToPdfCommand))]
    [ObservableProperty]
    private ObservableCollection<SelectedFile> _selectedFiles = [];

    [RelayCommand]
    private void Clear()
    {
        SelectedFiles.Clear();
    }

    [RelayCommand]
    private void MergeToPdf()
    {
        var dialog = new SaveFileDialog();

        if (dialog.ShowDialog() is true)
        {
            using var writer = new PdfWriter(dialog.FileName.EndsWith(".pdf") ? dialog.FileName : $"{dialog.FileName}.pdf");
            var pdfDocument = new PdfDocument(writer);
            var document = new Document(pdfDocument);
            document.SetMargins(0, 0, 0, 0);

            for (var i = 0; i < SelectedFiles.Count; i++)
            {
                var file = SelectedFiles[i];
                var imageData = ImageDataFactory.Create(file.FilePath);
                var image = new Image(imageData);
                image.SetPadding(0);
                pdfDocument.AddNewPage(new PageSize(image.GetImageWidth(), image.GetImageHeight()));
                image.SetPageNumber(i);
                document.Add(image);
            }

            document.Close();
        }
    }

    [RelayCommand]
    private void SelectFile()
    {
        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = ImageFilter
        };

        if (dialog.ShowDialog(Application.Current.MainWindow) is true)
        {
            var fileNames = dialog.FileNames;

            foreach (var fileName in fileNames)
            {
                SelectedFiles.Add(new SelectedFile(FileType.Image, fileName));
            }
        }
    }

    private static IEnumerable<string> CreateImageFilters()
    {
        yield return "All files (*.*)|*.*";

        var imageCodecInfos = ImageCodecInfo.GetImageDecoders();

        foreach (var codecInfo in imageCodecInfos)
        {
            var codecName = codecInfo.CodecName;
            var extension = codecInfo.FilenameExtension;

            if (codecName is not null)
            {
                yield return $"{codecName.Substring(8).Replace("Codec", "files")} ({extension})|{extension}";
            }
        }
    }
}
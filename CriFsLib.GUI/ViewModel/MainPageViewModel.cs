using CriFsLib.GUI.Model;
using CriFsV2Lib;
using CriFsV2Lib.Encryption.Game;
using CriFsV2Lib.Utilities;
using Ookii.Dialogs.Wpf;
using Reloaded.WPF.MVVM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace CriFsLib.GUI.ViewModel;

public class MainPageViewModel : ObservableObject
{
    public Visibility ShowDragDropText { get; set; } = Visibility.Visible;
    public Visibility ShowItemList { get; set; } = Visibility.Collapsed;
    public IList SelectedItems { get; set; } = new List<CpkFileModel>();
    public CpkFileModel[] Files { get; set; } = Array.Empty<CpkFileModel>();
    public string CurrentFilePath { get; set; } = string.Empty;

    internal void Extract()
    {
        if (!TryGetOutputFolder(out var folderPath))
            return; 

        ExtractItems(folderPath, SelectedItems.Cast<CpkFileModel>().ToArray());
    }

    internal void ExtractAll()
    {
        if (!TryGetOutputFolder(out var folderPath))
            return;

        ExtractItems(folderPath, Files.ToArray()); // clone as we sort the array.
        ArrayRental.Reset(); // Don't keep around in memory in case user leaves application idle.
    }

    private void ExtractItems(string folder, CpkFileModel[] files)
    {
        // TODO: Selectable crypto function when we add more.
        // Sort in ascending order, hopefully will reduce seeks.
        Array.Sort(files, (a,b) => a.File.FileOffset.CompareTo(b.File.FileOffset));
        using var extractor = new BatchFileExtractor<CpkFileExtractorItemModel>(CurrentFilePath, P5RCrypto.DecryptionFunction);
        for (int x = 0; x < files.Length; x++)
        {
            ref var file = ref files[x];
            extractor.QueueItem(new CpkFileExtractorItemModel(Path.Combine(folder, file.FullPath), file));
        }

        extractor.WaitForCompletion();
    }

    internal void OpenCpk(string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open);
        var files    = CpkHelper.GetFilesFromStream(fileStream);
        var newFiles = GC.AllocateUninitializedArray<CpkFileModel>(files.Length);

        for (int x = 0; x < files.Length; x++)
        {
            CriFsV2Lib.Structs.CpkFile file = files[x];
            var fullPath = !string.IsNullOrEmpty(file.Directory)
                ? Path.Combine(file.Directory, file.FileName)
                : file.FileName;

            newFiles[x] = new CpkFileModel(file, fullPath);
        }

        CurrentFilePath = filePath;
        Files = newFiles;
        ShowDragDropText = Visibility.Collapsed;
        ShowItemList = Visibility.Visible;
    }

    private static bool TryGetOutputFolder(out string folderPath)
    {
        folderPath = null!;
        var dialog = new VistaFolderBrowserDialog();
        if (!dialog.ShowDialog().GetValueOrDefault())
            return false;

        folderPath = dialog.SelectedPath;
        return true;
    }
}

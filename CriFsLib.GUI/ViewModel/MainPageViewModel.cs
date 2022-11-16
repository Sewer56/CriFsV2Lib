using ByteSizeLib;
using CriFsLib.GUI.Model;
using CriFsV2Lib;
using CriFsV2Lib.Encryption.Game;
using CriFsV2Lib.Utilities;
using Ookii.Dialogs.Wpf;
using Reloaded.WPF.MVVM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CriFsV2Lib.Definitions.Structs;
using CriFsV2Lib.Definitions.Utilities;
using CriFsV2Lib.Utilities.Parsing;

namespace CriFsLib.GUI.ViewModel;

public class MainPageViewModel : ObservableObject
{
    public Visibility ShowDragDropText { get; set; } = Visibility.Visible;
    public Visibility ShowItemList { get; set; } = Visibility.Collapsed;
    public IList SelectedItems { get; set; } = new List<CpkFileModel>();
    public CpkFileModel[] Files { get; set; } = Array.Empty<CpkFileModel>();
    public string CurrentFilePath { get; set; } = string.Empty;
    public string Completion { get; set; } = "0.0% / 00:00";
    public string NumItems { get; set; } = "0 Items";

    internal async Task Extract()
    {
        if (!TryGetOutputFolder(out var folderPath))
            return; 

        await ExtractItemsAsync(folderPath, SelectedItems.Cast<CpkFileModel>().ToArray());
    }

    internal async Task ExtractAllAsync()
    {
        if (!TryGetOutputFolder(out var folderPath))
            return;

        await ExtractItemsAsync(folderPath, Files.ToArray()); // clone as we sort the array.
        ArrayRental.Reset(); // Don't keep around in memory in case user leaves application idle.
        
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.AddMemoryPressure(nint.MaxValue);
        GC.Collect(); // will clean up LOH
        GC.RemoveMemoryPressure(nint.MaxValue);
    }

    private async Task ExtractItemsAsync(string folder, CpkFileModel[] files)
    {
        // TODO: Selectable crypto function when we add more.
        // Sort in ascending order, hopefully will reduce seeks.
        Array.Sort(files, (a,b) => a.File.FileOffset.CompareTo(b.File.FileOffset));
        using var extractor = new BatchFileExtractor<CpkFileExtractorItemModel>(CurrentFilePath, P5RCrypto.DecryptionFunction);
        for (int x = 0; x < files.Length; x++)
        {
            var file = files[x];
            extractor.QueueItem(new CpkFileExtractorItemModel(Path.Combine(folder, file.FullPath), file));
        }

        var maxItems = files.Length;
        var watch = Stopwatch.StartNew();
        await extractor.WaitForCompletionAsync(125, () =>
        {
            var percentage = (float)extractor.ItemsProcessed / maxItems;
            Completion = $"{percentage:#0.0%} / {watch.Elapsed:mm\\:ss}";
        });

        Completion = $"100.0% / {watch.Elapsed:mm\\:ss}";
    }

    internal void OpenCpk(string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open);
        var files    = CpkHelper.GetFilesFromStream(fileStream);
        var newFiles = GC.AllocateUninitializedArray<CpkFileModel>(files.Length);
        long totalSizeBytes = 0;

        for (int x = 0; x < files.Length; x++)
        {
            CpkFile file = files[x];
            var fullPath = !string.IsNullOrEmpty(file.Directory)
                ? Path.Combine(file.Directory, file.FileName)
                : file.FileName;

            newFiles[x] = new CpkFileModel(file, fullPath);
            totalSizeBytes += file.ExtractSize;
        }

        CurrentFilePath = filePath;
        Files = newFiles;
        NumItems = $"{newFiles.Length} Items / {ByteSize.FromBytes(totalSizeBytes):#.##}";
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

using CriFsLib.GUI.Model;
using CriFsV2Lib;
using CriFsV2Lib.Encryption.Game;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using Reloaded.WPF.MVVM;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CriFsLib.GUI.ViewModel;

public class MainPageViewModel : ObservableObject
{
    public Visibility ShowDragDropText { get; set; } = Visibility.Visible;
    public Visibility ShowItemList { get; set; } = Visibility.Collapsed;
    public IList SelectedItems { get; set; } = new List<CpkFileModel>();
    public List<CpkFileModel> Files { get; set; } = new List<CpkFileModel>();
    public string CurrentFilePath { get; set; } = string.Empty;

    internal void Extract()
    {
        if (!TryGetOutputFolder(out var folderPath))
            return; 

        ExtractItems(folderPath, SelectedItems.Cast<CpkFileModel>().ToList());
    }

    internal void ExtractAll()
    {
        if (!TryGetOutputFolder(out var folderPath))
            return;

        ExtractItems(folderPath, Files);
    }

    private void ExtractItems(string folder, List<CpkFileModel> files)
    {
        // TODO: Selectable crypto function when we add more.

        // Extract in Parallel
        Parallel.ForEach(Partitioner.Create(0, files.Count), (range, loopState) =>
        {
            using var fileStream = new FileStream(CurrentFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            for (int x = range.Item1; x < range.Item2; x++)
            {
                var file = files[x];

                // Extract
                using var data = CpkHelper.ExtractFile(file.File, fileStream, P5RCrypto.DecryptionFunction);
                var path = Path.Combine(folder, file.FullPath);

                // Create Directory
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                // Write to disk.
                using var outputStream = new FileStream(path, FileMode.Create);
                outputStream.Write(data.Span);
            }
        });
    }

    internal void OpenCpk(string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open);
        var files    = CpkHelper.GetFilesFromStream(fileStream);
        var newFiles = new List<CpkFileModel>(files.Length);
        var existingFilePaths = new HashSet<string>();

        foreach (var file in files)
        {
            // Deduplicate (CPKs can have multiple files with same relative paths to speed up loads).
            var fullPath = !string.IsNullOrEmpty(file.Directory) 
                ? Path.Combine(file.Directory, file.FileName)
                : file.FileName;

            if (!existingFilePaths.Contains(fullPath))
            {
                existingFilePaths.Add(fullPath);
                newFiles.Add(new CpkFileModel(file, fullPath));
            }
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

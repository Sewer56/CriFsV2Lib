using ByteSizeLib;
using CriFsV2Lib.Structs;
using System.IO;
using CriFsV2Lib.Definitions.Structs;

namespace CriFsLib.GUI.Model;

/// <summary>
/// Represents an individual file inside an CPK.
/// </summary>
public class CpkFileModel
{
    /// <summary>
    /// Full path of the file (directory and file path combined).
    /// </summary>
    public string FullPath { get; set; }

    /// <summary>
    /// Compression ratio expressed as a string.
    /// </summary>
    public string CompressionRatio { get; set; }

    /// <summary>
    /// File Size Expressed as Human Units.
    /// </summary>
    public string HumanSize { get; set; }

    /// <summary>
    /// The library file tied to this model.
    /// </summary>
    public CpkFile File { get; set; }

    public CpkFileModel(in CpkFile file, string fullPath)
    {
        FullPath = fullPath;
        CompressionRatio = ((float)file.FileSize / file.ExtractSize).ToString("##0.0%");
        HumanSize = ByteSize.FromBytes(file.ExtractSize).ToString();
        File = file;
    }
}

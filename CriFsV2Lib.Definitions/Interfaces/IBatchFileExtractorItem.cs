using CriFsV2Lib.Definitions.Structs;

namespace CriFsV2Lib.Definitions.Interfaces;

/// <summary>
/// Base interface used by items that can be queued to batch extractor.
/// </summary>
public interface IBatchFileExtractorItem
{
    /// <summary>
    /// Full path to where the file should be extracted to.
    /// </summary>
    public string FullPath { get; }

    /// <summary>
    /// Details of the file to be extracted.
    /// </summary>
    public CpkFile File { get; }
}

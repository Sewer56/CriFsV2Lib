namespace CriFsV2Lib.Definitions.Structs;

/// <summary>
/// Encapsulates all known information about a file.
/// </summary>
public struct CpkFile
{
    /// <summary>
    /// String some developers attach to provide more info on file, e.g. encrypt this file.
    /// </summary>
    public string? UserString { get; set; }
    
    /// <summary>
    /// Directory in which the file is contained.
    /// </summary>
    public string? Directory { get; set; }

    /// <summary>
    /// Name of the file inside the directory.
    /// </summary>
    public string FileName { get; set; }
    
    /// <summary>
    /// Offset of the file inside the CPK.
    /// </summary>
    public long FileOffset { get; set; }

    /// <summary>
    /// Size of the file inside the CPK.
    /// </summary>
    public int FileSize { get; set; }
    
    /// <summary>
    /// Size of the file after it's extracted.
    /// </summary>
    public int ExtractSize { get; set; }
}
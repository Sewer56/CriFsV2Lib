using CriFsV2Lib.Definitions.Structs;
using CriFsV2Lib.Definitions.Utilities;

namespace CriFsV2Lib.Definitions;

/// <summary>
/// API for the reader of CPKs.
/// </summary>
public interface ICpkReader : IDisposable
{
    /// <summary>
    /// The decryption function to use.
    /// </summary>
    public InPlaceDecryptionFunction? Decrypt { get; }
    
    /// <summary>
    /// Extracts the file from the CPK without decompressing it using CRILAYLA.
    /// </summary>
    /// <param name="file">The file to extract from the archive.</param>
    /// <param name="needsDecompression">
    ///    True if the file will need to be decompressed with <see cref="CriLayla.DecompressToArrayPool"/>.
    /// </param>
    /// <returns>Extracted data.</returns>
    public unsafe ArrayRental ExtractFileNoDecompression(in CpkFile file, out bool needsDecompression);
    
    /// <summary>
    /// Gets all of the file data info within a CPK file from a stream.
    /// Use when CPK file is big, e.g. reading 2GB+ file.
    /// </summary>
    /// <param name="file">The file to extract from the archive.</param>
    /// <returns>Extracted data.</returns>
    public ArrayRental ExtractFile(in CpkFile file);
    
    /// <summary>
    /// Gets all of the file data info within a CPK file from a stream.
    /// Use when CPK file is big, e.g. reading 2GB+ file.
    /// </summary>
    public CpkFile[] GetFiles();
}
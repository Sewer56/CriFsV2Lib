using CriFsV2Lib.Definitions.Interfaces;
using CriFsV2Lib.Definitions.Structs;

namespace CriFsV2Lib.Definitions;

/// <summary>
/// Provides an API for the entire library.
/// </summary>
public interface ICriFsLib
{
    /// <summary>
    /// Gets the utility functions that don't fit into any specific category.
    /// </summary>
    public ICriFsLibUtilities Utilities { get; }

    /// <summary>
    /// Sets the default in place decryption function to be used when one isn't supplied.
    /// </summary>
    /// <param name="function">The function to use.</param>
    public void SetDefaultEncryptionFunction(InPlaceDecryptionFunction function);
    
    /// <summary>
    /// Creates a reader that can be used to read a CPK file.
    /// </summary>
    /// <param name="cpkStream">Stream which starts at the beginning of a CPK file.</param>
    /// <param name="ownsStream">True to dispose the stream alongside the reader, else false.</param>
    /// <param name="decrypt">Decryption function to use.</param>
    /// <returns>Reader which can be used to read a CPK file.</returns>
    public ICpkReader CreateCpkReader(Stream cpkStream, bool ownsStream, InPlaceDecryptionFunction? decrypt = null);
    
    /// <summary>
    /// Obtains a known decryption function.
    /// </summary>
    /// <param name="decryptFunc">The known decryption function to obtain.</param>
    public InPlaceDecryptionFunction? GetKnownDecryptionFunction(KnownDecryptionFunction decryptFunc);

    /// <summary>
    /// Creates a helper for Batch Extraction of files.
    /// </summary>
    /// <param name="sourceCpkPath">Full path to the source CPK where files are extracted from.</param>
    /// <param name="decrypt">The decryption function to use.</param>
    public IBatchFileExtractor<T> CreateBatchExtractor<T>(string sourceCpkPath, InPlaceDecryptionFunction? decrypt = null) where T : IBatchFileExtractorItem;
}

/// <summary>
/// List of known decryption functions.
/// </summary>
public enum KnownDecryptionFunction
{
    /// <summary>
    /// Persona 5 Royal
    /// </summary>
    P5R
}

/// <summary>
/// Defines a function that will perform encryption in place.
/// </summary>
/// <param name="file">The file which will be decrypted.</param>
/// <param name="dataPtr">Pointer to the data to decrypt.</param>
/// <param name="dataLength">Length of the data to decrypt.</param>
public unsafe delegate void InPlaceDecryptionFunction(in CpkFile file, byte* dataPtr, int dataLength);
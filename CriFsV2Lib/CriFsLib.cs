using CriFsV2Lib.Definitions;
using CriFsV2Lib.Definitions.Interfaces;
using CriFsV2Lib.Definitions.Structs;
using CriFsV2Lib.Encryption.Game;
using CriFsV2Lib.Utilities;
using CriFsV2Lib.Utilities.Parsing;

namespace CriFsV2Lib;

/// <inheritdoc />
public class CriFsLib : ICriFsLib
{
    /// <summary>
    /// Singleton instance of this class.
    /// </summary>
    public static readonly CriFsLib Instance = new();
    
    /// <inheritdoc />
    public ICriFsLibUtilities Utilities { get; } = new CriFsLibUtilities();    
    
    /// <inheritdoc />
    public ICpkReader CreateCpkReader(Stream cpkStream, bool ownsStream, InPlaceDecryptionFunction? decrypt = null) => new CpkReader(cpkStream, ownsStream, decrypt);

    /// <inheritdoc />
    public InPlaceDecryptionFunction? GetKnownDecryptionFunction(KnownDecryptionFunction decryptFunc)
    {
        return decryptFunc switch
        {
            KnownDecryptionFunction.P5R => P5RCrypto.DecryptionFunction,
            _ => null
        };
    }

    /// <inheritdoc />
    public IBatchFileExtractor<T> CreateBatchExtractor<T>(string sourceCpkPath, InPlaceDecryptionFunction? decrypt = null) where T : IBatchFileExtractorItem
    {
        return new BatchFileExtractor<T>(sourceCpkPath, decrypt);
    }
}

/// <inheritdoc />
public class CriFsLibUtilities : ICriFsLibUtilities
{
    /// <inheritdoc />
    public unsafe CpkFile[] GetFiles(byte* dataPtr) => CpkHelper.GetFilesFromFile(dataPtr);
}
using CriFsV2Lib.Compression;
using CriFsV2Lib.Structs;
using CriFsV2Lib.Utilities;

namespace CriFsV2Lib;

/// <summary>
/// High level APIs for reading data from CPKs.
/// </summary>
public static class CpkHelper
{
    /// <summary>
    /// Gets all of the file data info within a CPK file from a stream.
    /// Use when CPK file is big, e.g. reading 2GB+ file.
    /// </summary>
    /// <param name="file">The file to extract from the archive.</param>
    /// <param name="stream">
    ///     A stream which should start at the beginning of the CPK file.
    /// </param>
    /// <param name="decrypt">
    ///    Optional function for decrypting the data.
    ///    The CRI SDK allows for users to encrypt files in-place, so if your game has some custom encryption, pass it here.
    /// </param>
    /// <returns>Extracted data.</returns>
    public static unsafe byte[] ExtractFile(in CpkFile file, Stream stream, InPlaceDecryptionFunction? decrypt = null)
    {
        // Note: In theory we could read in chunks and decompress on the fly, incurring
        // less memory allocation; however, this is incompatible with decryption function.  
        var rawData = GC.AllocateUninitializedArray<byte>(file.FileSize);
        stream.Position = file.FileOffset;
        stream.TryReadSafe(rawData);
        
        // Run decryption func if needed.
        fixed (byte* dataPtr = rawData)
        {
            decrypt?.Invoke(file, dataPtr, rawData.Length);
            return !CriLayla.IsCompressed(dataPtr) 
                ? rawData 
                : CriLayla.Decompress(dataPtr);
        }
    }

    /// <summary>
    /// Gets all of the file data info within a CPK file from a stream.
    /// Use when CPK file is big, e.g. reading 2GB+ file.
    /// </summary>
    /// <param name="stream">A stream which should start at the beginning of the CPK file.</param>
    public static unsafe CpkFile[] GetFilesFromStream(Stream stream)
    {
        // Read table.
        var basePos = stream.Position;
        var table   = TableContainerReader.ReadTable(stream);

        fixed (byte* tableDataPtr = &table[0])
        {
            TocFinder.FindToc(tableDataPtr, out var tocOffset, out var contentOffset);

            stream.Position = basePos + tocOffset;   
            var toc = TableContainerReader.ReadTable(stream);
            fixed (byte* tocPtr = &toc[0])
                return TocReader.ReadToc(tocPtr, contentOffset);
        }
    }

    /// <summary>
    /// Reads all of the files present within a CPK file from a byte array.  
    /// Use when entire CPK is in memory.  
    /// </summary>
    /// <param name="dataPtr">Pointer to the start of the CPK data in memory.</param>
    public static unsafe CpkFile[] GetFilesFromFile(byte* dataPtr)
    {
        TocFinder.FindToc(&dataPtr[TableContainerReader.TableContainerSize], out var tocOffset, out var contentOffset);
        
        var tocPtr = dataPtr + tocOffset + TableContainerReader.TableContainerSize;
        return TocReader.ReadToc(tocPtr, contentOffset);
    }

    /// <summary>
    /// Defines a function that will perform encryption in place.
    /// </summary>
    /// <param name="file">The file which will be decrypted.</param>
    /// <param name="dataPtr">Pointer to the data to decrypt.</param>
    /// <param name="dataLength">Length of the data to decrypt.</param>
    public unsafe delegate void InPlaceDecryptionFunction(in CpkFile file, byte* dataPtr, int dataLength);
}
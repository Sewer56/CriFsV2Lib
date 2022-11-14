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
    /// Extracts the file from the CPK without decompressing it using CRILAYLA.
    /// </summary>
    /// <param name="file">The file to extract from the archive.</param>
    /// <param name="stream">
    ///     A stream which should start at the beginning of the CPK file.
    /// </param>
    /// <param name="decrypt">
    ///    Optional function for decrypting the data.
    ///    The CRI SDK allows for users to encrypt files in-place, so if your game has some custom encryption, pass it here.
    /// </param>
    /// <param name="needsDecompression">
    ///    True if the file will need to be decompressed with <see cref="CriLayla.DecompressToArrayPool"/>.
    /// </param>
    /// <returns>Extracted data.</returns>
    public static unsafe ArrayRental<byte> ExtractFileNoDecompression(in CpkFile file, Stream stream, out bool needsDecompression, InPlaceDecryptionFunction? decrypt = null)
    {
        // Just in case empty file is stored.
        needsDecompression = false;
        if (file.FileSize == 0)
            return ArrayRental<byte>.Empty;

        // Note: In theory we could read in chunks and decompress on the fly, incurring
        // less memory allocation; however, this is incompatible with decryption function.  
        int compressedDataSize;
        var rawData = new ArrayRental<byte>(file.FileSize);
        stream.Position = file.FileOffset;
        stream.ReadAtLeast(rawData.Span, rawData.Count);

        // Run decryption func if needed.
        fixed (byte* dataPtr = rawData.RawArray)
        {
            decrypt?.Invoke(file, dataPtr, rawData.Count);
            needsDecompression = CriLayla.IsCompressed(dataPtr, out compressedDataSize);
            if (!needsDecompression)
                return rawData;

            if (rawData.Count >= compressedDataSize)
                return rawData;
        }

        rawData.Dispose();

        // In some cases CRI can pack archives with incorrect size (e.g. 130 when file is several MBs).
        // We need to doublecheck with the compression header.
        rawData = new ArrayRental<byte>(compressedDataSize);
        stream.Position = file.FileOffset;
        stream.ReadAtLeast(rawData.Span, rawData.Count);

        fixed (byte* dataPtr = rawData.RawArray)
        {
            decrypt?.Invoke(file, dataPtr, rawData.Count);
            return rawData;
        }
    }

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
    public static unsafe ArrayRental<byte> ExtractFile(in CpkFile file, Stream stream, InPlaceDecryptionFunction? decrypt = null)
    {
        // Just in case empty file is stored.
        if (file.FileSize == 0)
            return ArrayRental<byte>.Empty;

        var rawData = ExtractFileNoDecompression(file, stream, out var needsDecompression, decrypt);
        if (!needsDecompression)
            return rawData;

        fixed (byte* dataPtr = rawData.RawArray)
        {
            var result = CriLayla.DecompressToArrayPool(dataPtr);
            rawData.Dispose();
            return result;
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
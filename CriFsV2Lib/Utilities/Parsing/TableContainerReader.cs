using CriFsV2Lib.Encryption;

namespace CriFsV2Lib.Utilities.Parsing;

/// <summary>
/// Class that reads the table of contents.
/// </summary>
public static unsafe class TableContainerReader
{
    /// <summary>
    /// Size of the table container.
    /// </summary>
    public const int TableContainerSize = 16;
    
    /// <summary>
    /// Reads a CRI table from a given a stream that starts at the table container.
    /// </summary>
    /// <param name="str">
    ///     Stream containing the data for the table.
    ///     This stream should start with a signature such as 'CPK', 'TOC' or 'ETOC'.</param>
    /// <returns>Data containing the table.</returns>
    public static byte[] ReadTable(Stream str)
    {
        // Read size and go to start of table.
        str.Seek(8, SeekOrigin.Current);
        var size = str.Read<int>();
        str.Seek(4, SeekOrigin.Current);

        // Read the damn table.
        var result = GC.AllocateUninitializedArray<byte>(size);
        str.TryReadSafe(result);
        
        // Check for encryption and decrypt.
        fixed (byte* arrayPtr = &result[0])
        {
            if (TableDecryptor.IsEncrypted(arrayPtr))
                TableDecryptor.DecryptUTFInPlace(arrayPtr, result.Length);
        }

        return result;
    }
}
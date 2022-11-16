using System.Runtime.CompilerServices;
using CriFsV2Lib.Definitions.Structs;
using CriFsV2Lib.Structs;

namespace CriFsV2Lib.Utilities.Parsing;

/// <summary>
/// Reader of the CRI Table of Contents.
/// </summary>
public static class TocReader
{
    /// <summary>
    /// Finds the table of content from a CPK file.
    /// </summary>
    /// <param name="header">Pointer to CRI Table header.</param>
    /// <param name="contentOffset">Offset of the raw data inside the TOC. This is used to get final file pointer inside file.</param>
    /// <returns>List of all files inside table of contents.</returns>
    [SkipLocalsInit]
    public static unsafe CpkFile[] ReadToc(byte* header, long contentOffset)
    {
        // Read Table Metadata
        var metadata  = new CriTableMetadata(header);

        // Column indices
        int dirNameColumnIndex = -1;
        int fileNameColumnIndex = -1;
        int fileSizeColumnIndex = -1;
        int extractSizeColumnIndex = -1;
        int fileOffsetColumnIndex = -1;
        int userStringColumnIndex = -1;
        
        // Parse out the column data to find fields for the file info we want.
        // Not the most readable thing but is all super inlined for performance!
        var stringPoolPtr = header + metadata.StringPoolOffset;
        var columnsFlags = stackalloc CriColumnFlagsUnion[metadata.ColumnCount];
        var columnPtr    = metadata.GetFirstColumnPtr(header);
        
        for (int x = 0; x < metadata.ColumnCount; x++)
        {
            // Read the column.
            var columnData = (CriColumnFlagsUnion*)columnPtr;
            int columnSize = 1;
            columnsFlags[x] = *columnData;
            
            // Check if this is one of our wanted columns.
            if (columnData->HasFlag(CriColumnFlags.HasName))
            {
                columnSize += sizeof(CriString);
                var stringAddress = columnData->GetStringAddress(columnData, stringPoolPtr);
                
                // Check if our desired offsets.
                if (Constants.Fields.IsDirName(stringAddress)) dirNameColumnIndex = x;
                else if (Constants.Fields.IsFileName(stringAddress)) fileNameColumnIndex = x;
                else if (Constants.Fields.IsFileSize(stringAddress)) fileSizeColumnIndex = x;
                else if (Constants.Fields.IsExtractSize(stringAddress)) extractSizeColumnIndex = x;
                else if (Constants.Fields.IsFileOffset(stringAddress)) fileOffsetColumnIndex = x;
                else if (Constants.Fields.IsUserString(stringAddress)) userStringColumnIndex = x;
            }
            
            if (columnData->HasFlag(CriColumnFlags.HasDefaultValue))
                columnSize += columnData->GetValueLength();

            columnPtr += columnSize;
        }
        
        // Let's read the data from the rows now.
        // CPK header has only one row, so we read all column data.
        var encoding = metadata.GetEncoding(header);
        
        var userStringPool = new CriStringPool();
        var namePool = new CriStringPool();
        var baseRowPtr = metadata.GetFirstRowPtr(header);
        var result = new CpkFile[metadata.RowCount];
        
        for (int x = 0; x < result.Length; x++)
        {
            var currentRowPtr = baseRowPtr + (x * metadata.RowSizeBytes);
            ref var file = ref result[x];
            
            for (int y = 0; y < metadata.ColumnCount; y++)
            {
                var column = columnsFlags[y];

                // Having row storage here is implied, no need to check for flag.
                // Technically speaking, those strings can have default values; however with testing
                // and knowing the context of wh at these values are, we can ignore them. Empty string will be just fine.
                if (column.HasFlag(CriColumnFlags.IsRowStorage))
                {
                    if (y == dirNameColumnIndex)
                        file.Directory = namePool.Get(stringPoolPtr, (CriString*)currentRowPtr, encoding);
                    else if (y == fileNameColumnIndex)
                        file.FileName = namePool.GetWithoutPooling(stringPoolPtr, (CriString*)currentRowPtr, encoding);
                    else if (y == fileSizeColumnIndex)
                        file.FileSize = column.ReadNumberInt(currentRowPtr);
                    else if (y == extractSizeColumnIndex)
                        file.ExtractSize = column.ReadNumberInt(currentRowPtr);
                    else if (y == fileOffsetColumnIndex)
                        file.FileOffset = column.ReadNumberLong(currentRowPtr) + contentOffset;
                    else if (y == userStringColumnIndex)
                        file.UserString = userStringPool.Get(stringPoolPtr, (CriString*)currentRowPtr, encoding);
                }

                if (column.HasFlag(CriColumnFlags.IsRowStorage))
                    currentRowPtr += column.GetValueLength();
            }
        }

        return result;
    }
}
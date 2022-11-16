using System.Runtime.CompilerServices;
using CriFsV2Lib.Structs;

namespace CriFsV2Lib.Utilities.Parsing;

/// <summary>
/// Class that can be used to parse the CPK's main CRI Table to find the table of contents CRI Table.
/// </summary>
public static class TocFinder
{
    /// <summary>
    /// Finds the table of content
    /// </summary>
    /// <param name="header">Pointer to CRI Table header.</param>
    /// <param name="tocOffset">Offset of the table of contents inside the archive.</param>
    /// <param name="contentOffset">Offset from which file offsets are offset from.</param>
    /// <returns>Offset of the TOC inside the CPK file.</returns>
    [SkipLocalsInit]
    public static unsafe bool FindToc(byte* header, out long tocOffset, out long contentOffset)
    {
        const long DefaultOffset = -1;
        
        // Read Table Metadata
        var metadata  = new CriTableMetadata(header);

        // The columns we want to extract.
        tocOffset = DefaultOffset;
        contentOffset = DefaultOffset;

        // Column indices
        int tocOffsetColumnIndex = -1;
        int contentOffsetColumnIndex = -1;
        
        // Parse out the column data to find fields for TOC offset and content offset.
        // Not the most readable thing but is all super inlined for performance!
        var stringPoolPtr = header + metadata.StringPoolOffset;
        var columnsFlags = stackalloc CriColumnFlagsUnion[metadata.ColumnCount];
        var columnPtr = metadata.GetFirstColumnPtr(header);
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
                if (Constants.Fields.IsTocOffset(stringAddress)) tocOffsetColumnIndex = x;
                else if (Constants.Fields.IsContentOffset(stringAddress)) contentOffsetColumnIndex = x;
            }
            
            if (columnData->HasFlag(CriColumnFlags.HasDefaultValue))
                columnSize += columnData->GetValueLength();

            columnPtr += columnSize;
        }
        
        // Let's read the data from the rows now.
        // CPK header has only one row, so we read all column data.
        var currentRowPtr = metadata.GetFirstRowPtr(header);
        for (int x = 0; x < metadata.ColumnCount; x++)
        {
            var column = columnsFlags[x];

            // Having row storage here is implied, no need to check for flag.
            // We know these offsets are stored as long(s), but we will read properly just in case.
            if (x == tocOffsetColumnIndex)
                tocOffset = column.ReadNumberLong(currentRowPtr);
            else if (x == contentOffsetColumnIndex)
                contentOffset = column.ReadNumberLong(currentRowPtr);

            // Usually these columns come near the very start, so an additional check is faster.
            if (tocOffset != DefaultOffset && contentOffset != DefaultOffset)
                break;

            if (column.HasFlag(CriColumnFlags.IsRowStorage))
                currentRowPtr += column.GetValueLength();
        }

        // In some CPKs offsets are relative to TOC as opposed to ContentOffset in header.
        // This happens when TOC address is before ContentOffset.
        if (tocOffset < contentOffset)
            contentOffset = tocOffset; 

        return (tocOffset != DefaultOffset) && (contentOffset != DefaultOffset);
    }
}
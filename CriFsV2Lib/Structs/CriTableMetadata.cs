using System.Buffers.Binary;
using System.Text;

namespace CriFsV2Lib.Structs;

/// <summary>
/// Minimal metadata needed for parsing CRI tables.
/// </summary>
public unsafe ref struct CriTableMetadata
{
    private static Encoding Shift_JIS => CodePagesEncodingProvider.Instance.GetEncoding(932)!;
    
    /// <summary>
    /// Base offset from which other offsets are relative to.
    /// </summary>
    public const int BaseOffset = 0x08;
    
    /// <summary>
    /// Offset to column data in header.
    /// </summary>
    public const int ColumnOffset = 0x20;
    
    /// <summary>
    /// Offset of row data relative to start of table.
    /// </summary>
    public ushort RowsOffset;
    
    /// <summary>
    /// Offset of string data relative to start of table.
    /// </summary>
    public int StringPoolOffset;
    
    /// <summary>
    /// Offset of raw data relative to start of table.
    /// </summary>
    public int DataPoolOffset;
    
    /// <summary>
    /// Number of columns present in this table. (sometimes referred to as fields in other parsers)
    /// </summary>
    public ushort ColumnCount;
    
    /// <summary>
    /// Size of an individual row in bytes.
    /// </summary>
    public ushort RowSizeBytes;
    
    /// <summary>
    /// Number of rows in this table.
    /// </summary>
    public int RowCount;

    /// <summary>
    /// Gets the metadata of a table.
    /// </summary>
    /// <param name="header">Pointer to header of table.</param>
    public CriTableMetadata(byte* header)
    {
        // Read the fields until we find what we need.
        RowsOffset       = BinaryPrimitives.ReverseEndianness(*(ushort*)(header + 0x0A));
        StringPoolOffset = BinaryPrimitives.ReverseEndianness(*(int*)(header + 0x0C));
        DataPoolOffset   = BinaryPrimitives.ReverseEndianness(*(int*)(header + 0x10));
        
        // Field Data
        ColumnCount   = BinaryPrimitives.ReverseEndianness(*(ushort*)(header + 0x18));
        RowSizeBytes = BinaryPrimitives.ReverseEndianness(*(ushort*)(header + 0x1A));
        RowCount     = BinaryPrimitives.ReverseEndianness(*(int*)(header + 0x1C));
        
        // Make Offsets Absolute
        RowsOffset += BaseOffset;
        StringPoolOffset += BaseOffset;
        DataPoolOffset += BaseOffset;
    }

    /// <summary>
    /// Gets the encoding for this file.
    /// </summary>
    /// <param name="header">Header of the file.</param>
    public Encoding GetEncoding(byte* header) => *(header + 0x9) == 0 ? Shift_JIS : Encoding.UTF8;

    /// <summary>
    /// Returns the pointer to the first column.
    /// </summary>
    /// <param name="headerPtr">Header pointer.</param>
    public byte* GetFirstColumnPtr(byte* headerPtr) => headerPtr + ColumnOffset;
    
    /// <summary>
    /// Returns the pointer to the first row.
    /// </summary>
    /// <param name="headerPtr">Header pointer.</param>
    public byte* GetFirstRowPtr(byte* headerPtr) => headerPtr + RowsOffset;
}
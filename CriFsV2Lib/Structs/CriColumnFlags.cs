using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CriFsV2Lib.Structs;

/// <summary>
/// Union between all valid flags for the CRI Field.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 1)]
public struct CriColumnFlagsUnion
{
    private const byte TypeMask = 15;
    
    [FieldOffset(0)]
    private CriColumnFlags _flags;
    
    [FieldOffset(0)]
    private CriColumnType _type;

    /// <summary>
    /// Returns the field flags from this union.
    /// </summary>
    public CriColumnFlags AsFlags() => (CriColumnFlags)((byte)_flags & ~TypeMask);
    
    /// <summary>
    /// Returns the field type from this union.
    /// </summary>
    public CriColumnType AsType() => (CriColumnType)((byte)_type & TypeMask);

    /// <summary>
    /// Returns true if the flag is present.
    /// </summary>
    /// <param name="flag">The flag to test.</param>
    public bool HasFlag(CriColumnFlags flag) => (_flags & flag) == flag;

    /// <summary>
    /// Gets the prefix for the name.
    /// Assumes <see cref="HasName"/> == true.
    /// </summary>
    /// <param name="thisPtr">Pointer to the 'this' instance.</param>
    /// <param name="stringPoolPtr">Pointer to the address of the first character in the string section/pool.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe long GetNamePrefixLongFast(CriColumnFlagsUnion* thisPtr, byte* stringPoolPtr)
    {
        return ((CriString*)(thisPtr + 1))->GetPrefixLongFast(stringPoolPtr);
    }

    /// <summary>
    /// Gets the length of the value in bytes (if present).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetValueLength()
    {
        // Do not refactor this to switch expression, the IL output is different
        // and the JIT is missing an optimisation there.
        switch (AsType())
        {
            case CriColumnType.Byte: return 1;
            case CriColumnType.SByte: return 1;
            case CriColumnType.UInt16: return 2;
            case CriColumnType.Int16: return 2;
            case CriColumnType.UInt32: return 4;
            case CriColumnType.Int32: return 4;
            case CriColumnType.UInt64: return 8;
            case CriColumnType.Int64: return 8;
            case CriColumnType.Single: return 4;
            case CriColumnType.Double: return 8;
            case CriColumnType.String: return 4;
            case CriColumnType.RawData: return 8;
            case CriColumnType.Guid: return 16;
            default: return 0;
        }
    }

    /// <summary>
    /// Reads the data value for this column and casts it into a long.
    /// </summary>
    /// <param name="currentRowPtr">Pointer to the data value.</param>
    public unsafe long ReadNumberLong(byte* currentRowPtr)
    {
        return AsType() switch
        {
            CriColumnType.Byte => *currentRowPtr,
            CriColumnType.SByte => *(sbyte*)currentRowPtr,
            CriColumnType.UInt16 => BinaryPrimitives.ReverseEndianness(*(ushort*)currentRowPtr),
            CriColumnType.Int16 => BinaryPrimitives.ReverseEndianness(*(short*)currentRowPtr),
            CriColumnType.UInt32 => BinaryPrimitives.ReverseEndianness(*(uint*)currentRowPtr),
            CriColumnType.Int32 => BinaryPrimitives.ReverseEndianness(*(int*)currentRowPtr),
            CriColumnType.UInt64 => BinaryPrimitives.ReverseEndianness(*(long*)currentRowPtr),
            CriColumnType.Int64 => BinaryPrimitives.ReverseEndianness(*(long*)currentRowPtr),
            _ => -1
        };
    }
    
    /// <summary>
    /// Reads the data value for this column and casts it into an int.
    /// </summary>
    /// <param name="currentRowPtr">Pointer to the data value.</param>
    public unsafe int ReadNumberInt(byte* currentRowPtr)
    {
        return AsType() switch
        {
            CriColumnType.Byte => *currentRowPtr,
            CriColumnType.SByte => *(sbyte*)currentRowPtr,
            CriColumnType.UInt16 => BinaryPrimitives.ReverseEndianness(*(ushort*)currentRowPtr),
            CriColumnType.Int16 => BinaryPrimitives.ReverseEndianness(*(short*)currentRowPtr),
            CriColumnType.UInt32 => (int)BinaryPrimitives.ReverseEndianness(*(uint*)currentRowPtr),
            CriColumnType.Int32 => BinaryPrimitives.ReverseEndianness(*(int*)currentRowPtr),
            CriColumnType.UInt64 => (int)BinaryPrimitives.ReverseEndianness(*(long*)currentRowPtr),
            CriColumnType.Int64 => (int)BinaryPrimitives.ReverseEndianness(*(long*)currentRowPtr),
            _ => -1
        };
    }
}

/// <summary>
/// Flags for CRI Table columns. This is an union with <see cref="CriColumnType"/>
/// </summary>
[Flags]
public enum CriColumnFlags : byte
{
    /// <summary>
    /// This field contains a string with field name.
    /// </summary>
    HasName = 16,
    
    /// <summary>
    /// This field contains a default value.
    /// </summary>
    HasDefaultValue = 32,
    
    /// <summary>
    /// Row contains data for this field.
    /// </summary>
    IsRowStorage = 64
}

/// <summary>
/// Type of data stored in this field. This is an union with <see cref="CriColumnFlags"/>
/// </summary>
public enum CriColumnType : byte
{
    Byte = 0,
    SByte = 1,
    UInt16 = 2,
    Int16 = 3,
    UInt32 = 4,
    Int32 = 5,
    UInt64 = 6,
    Int64 = 7,
    Single = 8,
    Double = 9,
    String = 10,
    RawData = 11,
    Guid = 12,
}
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace CriFsV2Lib.Structs;

/// <summary>
/// Represents an individual string in the CRI Table.
/// A string is simply an offset to the actual string data in the string pool.
/// </summary>
public struct CriString
{
    /// <summary>
    /// Offset of this string.
    /// </summary>
    public int Offset;
    
    /// <summary>
    /// Gets the prefix for the string. Use with strings 7 >= characters.
    /// </summary>
    /// <param name="stringPoolPtr">Pointer to the address of the first character in the string section/pool.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe long GetPrefixLongFast(byte* stringPoolPtr)
    {
        return *(long*)(stringPoolPtr + BinaryPrimitives.ReverseEndianness(Offset));
    }
}
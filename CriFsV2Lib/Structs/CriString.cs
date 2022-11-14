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
    /// Gets the address of the string contained here.
    /// Assumes <see cref="HasName"/> == true.
    /// </summary>
    /// <param name="stringPoolPtr">Pointer to the address of the first character in the string section/pool.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe byte* GetStringAddress(byte* stringPoolPtr)
    {
        return (stringPoolPtr + BinaryPrimitives.ReverseEndianness(Offset));
    }
}
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CriFsV2Lib.Structs;

namespace CriFsV2Lib.Utilities;

/// <summary>
/// Pools strings from CRI files into managed strings.
/// </summary>
public struct CriStringPool
{
    private string _lastString = null!;
    private int _lastOffset = -1;

    public CriStringPool() { }

    /// <summary>
    /// Gets a string from the string pool.
    /// </summary>
    /// <param name="stringPoolAddr">Address of the first byte of the string pool in CPK.</param>
    /// <param name="stringPtr">Pointer to the CRI String.</param>
    /// <param name="encoding">The encoding used for the string conversion.</param>
    /// <returns>Managed string at that position.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe string Get(byte* stringPoolAddr, CriString* stringPtr, Encoding encoding) => Get(stringPoolAddr, BinaryPrimitives.ReverseEndianness(stringPtr->Offset), encoding);

    /// <summary>
    /// Gets a string from the string pool.
    /// Use if string is expected to be non-unique.
    /// </summary>
    /// <param name="stringPoolAddr">Address of the first byte of the string pool in CPK.</param>
    /// <param name="offset">Offset of the string.</param>
    /// <param name="encoding">The encoding used for the string conversion.</param>
    /// <returns>Managed string at that position.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe string Get(byte* stringPoolAddr, int offset, Encoding encoding)
    {
        if (_lastOffset == offset)
            return _lastString;
            
        _lastOffset = offset;
        var result  = GetWithoutPooling(stringPoolAddr, offset, encoding);
        _lastString = result;
        return result;
    }
    
    /// <summary>
    /// Gets a string without adding it to the string cache.
    /// Use if string is expected to be unique.
    /// </summary>
    /// <param name="stringPoolAddr">Address of the first byte of the string pool in CPK.</param>
    /// <param name="stringPtr">Pointer to the CRI String.</param>
    /// <param name="encoding">The encoding used for the string conversion.</param>
    /// <returns>Managed string at that position.</returns>
    public unsafe string GetWithoutPooling(byte* stringPoolAddr, CriString* stringPtr, Encoding encoding) => GetWithoutPooling(stringPoolAddr, BinaryPrimitives.ReverseEndianness(stringPtr->Offset), encoding);

    /// <summary>
    /// Gets a string without adding it to the string cache.
    /// Use if string is expected to be unique.
    /// </summary>
    /// <param name="stringPoolAddr">Address of the first byte of the string pool in CPK.</param>
    /// <param name="offset">Offset of the string.</param>
    /// <param name="encoding">The encoding used for the string conversion.</param>
    /// <returns>Managed string at that position.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe string GetWithoutPooling(byte* stringPoolAddr, int offset, Encoding encoding)
    {
        // Note: CreateReadOnlySpanFromNullTerminated seems to be the only strlen exposed by the runtime. I checked runtime source.
        var addr = stringPoolAddr + offset;
        return encoding.GetString(addr, MemoryMarshal.CreateReadOnlySpanFromNullTerminated(addr).Length);
    }
}
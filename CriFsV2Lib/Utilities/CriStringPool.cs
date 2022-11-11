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
    private Dictionary<int, string> _offsetToString;

    /// <summary/>
    /// <param name="numItems">Expected number of items.</param>
    public CriStringPool(int numItems) => _offsetToString = new Dictionary<int, string>(numItems);

    /// <summary>
    /// Gets a string from the string pool.
    /// </summary>
    /// <param name="stringPoolAddr">Address of the first byte of the string pool in CPK.</param>
    /// <param name="stringPtr">Pointer to the CRI String.</param>
    /// <param name="encoding">The encoding used for the string conversion.</param>
    /// <returns>Managed string at that position.</returns>
    public unsafe string Get(byte* stringPoolAddr, CriString* stringPtr, Encoding encoding) => Get(stringPoolAddr, BinaryPrimitives.ReverseEndianness(stringPtr->Offset), encoding);

    /// <summary>
    /// Gets a string from the string pool.
    /// </summary>
    /// <param name="stringPoolAddr">Address of the first byte of the string pool in CPK.</param>
    /// <param name="offset">Offset of the string.</param>
    /// <param name="encoding">The encoding used for the string conversion.</param>
    /// <returns>Managed string at that position.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe string Get(byte* stringPoolAddr, int offset, Encoding encoding)
    {
        if (_offsetToString.TryGetValue(offset, out var result))
            return result;

        var addr = stringPoolAddr + offset;
        
        // Note: CreateReadOnlySpanFromNullTerminated seems to be the only strlen exposed by the runtime. I checked runtime source.
        result = encoding.GetString(addr, MemoryMarshal.CreateReadOnlySpanFromNullTerminated(addr).Length);
        _offsetToString[offset] = result;
        return result;
    }
}
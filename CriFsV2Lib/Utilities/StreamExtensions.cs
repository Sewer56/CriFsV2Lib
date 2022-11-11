using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CriFsV2Lib.Utilities;

/// <summary>
/// Extensions to the 'Stream' class.
/// </summary>
public static class StreamExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(this Stream stream) where T : unmanaged
    {
        Span<T> stackSpace = stackalloc T[1];
        stream.Read(MemoryMarshal.Cast<T, byte>(stackSpace));
        return stackSpace[0];
    }
}
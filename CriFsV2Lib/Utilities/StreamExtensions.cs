using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CriFsV2Lib.Utilities;

/// <summary>
/// Extensions to the 'Stream' class.
/// </summary>
internal static class StreamExtensions
{
    /// <summary>
    /// Reads a given number of bytes from a stream.
    /// </summary>
    /// <param name="stream">The stream to read the value from.</param>
    /// <param name="result">The buffer to receive the bytes.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryReadSafe(this Stream stream, byte[] result)
    {
        int numBytesRead = 0;
        int numBytesToRead = result.Length;

        while (numBytesToRead > 0)
        {
            int bytesRead = stream.Read(result, numBytesRead, numBytesToRead);
            if (bytesRead <= 0)
                return false;

            numBytesRead += bytesRead;
            numBytesToRead -= bytesRead;
        }

        return true;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(this Stream stream) where T : unmanaged
    {
        Span<T> stackSpace = stackalloc T[1];
        stream.Read(MemoryMarshal.Cast<T, byte>(stackSpace));
        return stackSpace[0];
    }
}
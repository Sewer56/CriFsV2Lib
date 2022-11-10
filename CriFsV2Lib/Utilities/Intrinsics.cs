using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace CriFsV2Lib.Utilities;

/// <summary>
/// Provides native intrinsics support.
/// </summary>
public static unsafe class Intrinsics
{
    /// <summary>
    /// Copies data between source and destination using SSE3.
    /// </summary>
    /// <param name="src">Source.</param>
    /// <param name="dst">Destination.</param>
    /// <param name="len">Length.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyNative(byte* src, byte* dst, int len)
    {
        Buffer.MemoryCopy(src, dst, len, len);
    }

    /// <summary>
    /// Multiplies individual bytes for AVX registers.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<byte> MultiplyBytesAvx(Vector256<short> a, Vector256<short> b)
    {
        // Derived from https://stackoverflow.com/questions/8193601/sse-multiplication-16-x-uint8-t
        var even = Avx2.MultiplyLow(a, b);
        var odd = Avx2.MultiplyLow(Avx2.ShiftRightLogical(a, 8), Avx2.ShiftRightLogical(b, 8));
        return Avx2.Or(Avx2.ShiftLeftLogical(odd, 8), Avx2.And(even, Vector256.Create((short)0xFF))).AsByte();
    }

    /// <summary>
    /// Multiplies individual bytes for SSE registers.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<byte> MultiplyBytesSse(Vector128<short> a, Vector128<short> b)
    {
        // unpack and multiply
        var even = Sse2.MultiplyLow(a, b);
        var odd = Sse2.MultiplyLow(Sse2.ShiftRightLogical(a, 8), Sse2.ShiftRightLogical(b, 8));
        return Sse2.Or(Sse2.ShiftLeftLogical(odd, 8), Sse2.ShiftRightLogical(Sse2.ShiftLeftLogical(even, 8), 8)).AsByte();
    }
}
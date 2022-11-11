using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using CriFsV2Lib.Utilities;

namespace CriFsV2Lib.Encryption;

/// <summary>
/// Class for fast decrypting of CRI Tables.
/// </summary>
public static unsafe class TableDecryptor
{
    /// <summary>
    /// Checks if this table is encrypted.
    /// </summary>
    /// <param name="input">Pointer to first byte in table.</param>
    public static bool IsEncrypted(byte* input) =>  *(uint*) input == 0xF5F39E1F;
    
    /// <summary>
    /// Decrypts the CRI table (usually starts with 'UTF'), creating a copy of the data so original is unmodified.
    /// </summary>
    /// <param name="input">Table data.</param>
    /// <param name="length">Length of th e given byte pointer.</param>
    /// <returns>The decrypted table data (original is unmodified)</returns>
    public static byte[] DecryptUTF(byte* input, int length)
    {
        var result = GC.AllocateUninitializedArray<byte>(length);
        fixed (byte* resultPtr = result)
        {
            Intrinsics.CopyNative(input, resultPtr, length);
            DecryptUTFInPlace(resultPtr, length);
        }

        return result;
    }

    /// <summary>
    /// Decrypts the CRI table (usually starts with 'UTF') in place.
    /// </summary>
    /// <param name="input">Table data.</param>
    /// <param name="length">Length of th e given byte pointer.</param>
    /// <returns></returns>
    public static void DecryptUTFInPlace(byte* input, int length)
    {
        // Following rules of indices, 
        const byte xorMultiplier = unchecked((byte)0x00004115);
        const byte xorMultiplier2 = unchecked((byte)(xorMultiplier * xorMultiplier));
        const byte xorMultiplier3 = unchecked((byte)unchecked(xorMultiplier2 * xorMultiplier));
        const byte xorMultiplier4 = unchecked((byte)unchecked(xorMultiplier3 * xorMultiplier));
        const byte xorMultiplier5 = unchecked((byte)unchecked(xorMultiplier4 * xorMultiplier));
        const byte xorMultiplier6 = unchecked((byte)unchecked(xorMultiplier5 * xorMultiplier));
        const byte xorMultiplier7 = unchecked((byte)unchecked(xorMultiplier6 * xorMultiplier));
        const byte xorMultiplier8 = unchecked((byte)unchecked(xorMultiplier7 * xorMultiplier));
        const byte xorMultiplier9 = unchecked((byte)unchecked(xorMultiplier8 * xorMultiplier));
        const byte xorMultiplier10 = unchecked((byte)unchecked(xorMultiplier9 * xorMultiplier));
        const byte xorMultiplier11 = unchecked((byte)unchecked(xorMultiplier10 * xorMultiplier));
        const byte xorMultiplier12 = unchecked((byte)unchecked(xorMultiplier11 * xorMultiplier));
        const byte xorMultiplier13 = unchecked((byte)unchecked(xorMultiplier12 * xorMultiplier));
        const byte xorMultiplier14 = unchecked((byte)unchecked(xorMultiplier13 * xorMultiplier));
        const byte xorMultiplier15 = unchecked((byte)unchecked(xorMultiplier14 * xorMultiplier));
        const byte xorMultiplier16 = unchecked((byte)unchecked(xorMultiplier15 * xorMultiplier));
        const byte xorMultiplier17 = unchecked((byte)unchecked(xorMultiplier16 * xorMultiplier));
        const byte xorMultiplier18 = unchecked((byte)unchecked(xorMultiplier17 * xorMultiplier));
        const byte xorMultiplier19 = unchecked((byte)unchecked(xorMultiplier18 * xorMultiplier));
        const byte xorMultiplier20 = unchecked((byte)unchecked(xorMultiplier19 * xorMultiplier));
        const byte xorMultiplier21 = unchecked((byte)unchecked(xorMultiplier20 * xorMultiplier));
        const byte xorMultiplier22 = unchecked((byte)unchecked(xorMultiplier21 * xorMultiplier));
        const byte xorMultiplier23 = unchecked((byte)unchecked(xorMultiplier22 * xorMultiplier));
        const byte xorMultiplier24 = unchecked((byte)unchecked(xorMultiplier23 * xorMultiplier));
        const byte xorMultiplier25 = unchecked((byte)unchecked(xorMultiplier24 * xorMultiplier));
        const byte xorMultiplier26 = unchecked((byte)unchecked(xorMultiplier25 * xorMultiplier));
        const byte xorMultiplier27 = unchecked((byte)unchecked(xorMultiplier26 * xorMultiplier));
        const byte xorMultiplier28 = unchecked((byte)unchecked(xorMultiplier27 * xorMultiplier));
        const byte xorMultiplier29 = unchecked((byte)unchecked(xorMultiplier28 * xorMultiplier));
        const byte xorMultiplier30 = unchecked((byte)unchecked(xorMultiplier29 * xorMultiplier));
        const byte xorMultiplier31 = unchecked((byte)unchecked(xorMultiplier30 * xorMultiplier));
        const byte xorMultiplier32 = unchecked((byte)unchecked(xorMultiplier31 * xorMultiplier));

        // Note: Could probably do even faster with SSE/AVX but for now I'm satisfied.
        byte xor = unchecked((byte)0x0000655f);

        if (Avx2.IsSupported)
        {
            // AVX Version
            const int unrollFactor = 32;
            int numLoops = length / unrollFactor;
            var multiplier = Vector256.Create(1, xorMultiplier, xorMultiplier2, xorMultiplier3, xorMultiplier4,
                xorMultiplier5, xorMultiplier6, xorMultiplier7, xorMultiplier8, xorMultiplier9, xorMultiplier10,
                xorMultiplier11, xorMultiplier12, xorMultiplier13, xorMultiplier14, xorMultiplier15, xorMultiplier16,
                xorMultiplier17, xorMultiplier18, xorMultiplier19, xorMultiplier20, xorMultiplier21, xorMultiplier22,
                xorMultiplier23, xorMultiplier24, xorMultiplier25, xorMultiplier26, xorMultiplier27, xorMultiplier28,
                xorMultiplier29, xorMultiplier30, xorMultiplier31);

            for (int x = 0; x < numLoops; x++)
            {
                // Repeat unrollFactor times
                int offset = x * unrollFactor;

                // Multiply many at once.
                var value = Avx.LoadDquVector256(input + offset);
                var xorPattern = Vector256.Create(xor);
                var multipliedXor = Intrinsics.MultiplyBytesAvx(xorPattern.AsInt16(), multiplier.AsInt16());
                var result = Avx2.Xor(value, multipliedXor);

                // Merge and XOR value
                result.Store(input + offset);
                xor *= xorMultiplier32;
            }

            // Do remaining looping (factor in for length not divisible by unrollFactor)
            for (int x = numLoops * unrollFactor; x < length; x++)
            {
                input[x] = (byte)(input[x] ^ (byte)(xor & 0xff));
                xor *= xorMultiplier;
            }
        }
        else if (Sse3.IsSupported)
        {
            // SSE Version
            const int unrollFactor = 16;
            int numLoops = length / unrollFactor;
            var multiplier = Vector128.Create(1, xorMultiplier, xorMultiplier2, xorMultiplier3, xorMultiplier4,
                xorMultiplier5, xorMultiplier6, xorMultiplier7, xorMultiplier8, xorMultiplier9, xorMultiplier10,
                xorMultiplier11, xorMultiplier12, xorMultiplier13, xorMultiplier14, xorMultiplier15);

            for (int x = 0; x < numLoops; x++)
            {
                // Repeat unrollFactor times
                int offset = x * unrollFactor;

                // Multiply many at once.
                var value = Sse3.LoadDquVector128(input + offset);
                var xorPattern = Vector128.Create(xor);
                var multipliedXor = Intrinsics.MultiplyBytesSse(xorPattern.AsInt16(), multiplier.AsInt16());
                var result = Sse3.Xor(value, multipliedXor);

                // Merge and XOR value
                result.Store(input + offset);
                xor *= xorMultiplier16;
            }

            // Do remaining looping (factor in for length not divisible by unrollFactor)
            for (int x = numLoops * unrollFactor; x < length; x++)
            {
                input[x] = (byte)(input[x] ^ (byte)(xor & 0xff));
                xor *= xorMultiplier;
            }
        }
        else
        {
            // Non-Vector Version
            const int unrollFactor = 8;
            int numLoops = length / unrollFactor;

            for (int x = 0; x < numLoops; x++)
            {
                // Repeat unrollFactor times
                int offset = x * unrollFactor;
                long value = *(long*)(input + offset);

                // Init multiple registers
                long a = xor;
                long b = (byte)(xor * xorMultiplier);
                long c = (byte)(xor * xorMultiplier2);
                long d = (byte)(xor * xorMultiplier3);
                long e = (byte)(xor * xorMultiplier4);
                long f = (byte)(xor * xorMultiplier5);
                long g = (byte)(xor * xorMultiplier6);
                long h = (byte)(xor * xorMultiplier7);

                // Shift all registers
                b <<= 8; c <<= 16; d <<= 24; e <<= 32; f <<= 40; g <<= 48; h <<= 56;

                // Merge w/o register dependencies
                a ^= b;
                c ^= d;
                e ^= f;
                g ^= h;

                // Merge again.
                a ^= c;
                e ^= g;

                // Merge and XOR value
                *(long*)(input + offset) = value ^ (a | e);
                xor *= xorMultiplier8;
            }

            // Do remaining looping (factor in for length not divisible by unrollFactor)
            for (int x = numLoops * unrollFactor; x < length; x++)
            {
                input[x] = (byte)(input[x] ^ (byte)(xor & 0xff));
                xor *= xorMultiplier;
            }
        }
    }
}
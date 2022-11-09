using System.Runtime.CompilerServices;

namespace CriFsV2Lib.Encryption;

/// <summary>
/// Class for fast decrypting of CRI Tables.
/// </summary>
public static unsafe class TableDecryptor
{
    /// <summary>
    /// Decrypts the CRI table (usually starts with 'UTF') in place.
    /// </summary>
    /// <param name="input">Table data.</param>
    /// <param name="length">Length of th e given byte pointer.</param>
    /// <returns></returns>
    public static void DecryptUTFInPlace(byte* input, int length)
    {
        const int xorMultiplier  = 0x00004115;
        const int xorMultiplier2 = xorMultiplier * xorMultiplier;
        const int xorMultiplier3 = unchecked(xorMultiplier * xorMultiplier * xorMultiplier);
        const int xorMultiplier4 = unchecked(xorMultiplier * xorMultiplier * xorMultiplier * xorMultiplier);
        const int xorMultiplier5 = unchecked(xorMultiplier * xorMultiplier * xorMultiplier * xorMultiplier * xorMultiplier);
        const int xorMultiplier6 = unchecked(xorMultiplier * xorMultiplier * xorMultiplier * xorMultiplier * xorMultiplier * xorMultiplier);
        const int xorMultiplier7 = unchecked(xorMultiplier * xorMultiplier * xorMultiplier * xorMultiplier * xorMultiplier * xorMultiplier * xorMultiplier);
        const int xorMultiplier8 = unchecked(xorMultiplier * xorMultiplier * xorMultiplier * xorMultiplier * xorMultiplier * xorMultiplier * xorMultiplier * xorMultiplier);
        const int unrollFactor = 8;

        // Note: Could probably do even faster with SSE/AVX but for now I'm satisfied.
        long xor = 0x0000655f;
        int numLoops  = length / unrollFactor;
        
        for (int x = 0; x < numLoops; x++)
        {
            // Repeat unrollFactor times
            int offset = x * unrollFactor;
            long value = *(long*)(input + offset);
            
            // Init multiple registers
            long a = xor;
            long b = xor * xorMultiplier;
            long c = xor * xorMultiplier2;
            long d = xor * xorMultiplier3;
            long e = xor * xorMultiplier4;
            long f = xor * xorMultiplier5;
            long g = xor * xorMultiplier6; 
            long h = xor * xorMultiplier7;
            
            // AND all registers
            a &= 0xff; b &= 0xff; c &= 0xff; d &= 0xff; e &= 0xff; f &= 0xff; g &= 0xff; h &= 0xff;
            
            // Shift all registers
            b <<= 8; 
            c <<= 16;
            d <<= 24; 
            e <<= 32;
            f <<= 40;
            g <<= 48; 
            h <<= 56;

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
            Buffer.MemoryCopy(input, resultPtr, length, length);
            DecryptUTFInPlace(resultPtr, length);
        } 
        
        return result;
    }
}
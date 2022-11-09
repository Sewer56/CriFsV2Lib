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

        long xor = 0x0000655f;
        int numLoops  = length / unrollFactor;
        
        for (int x = 0; x < numLoops; x++)
        {
            // Repeat unrollFactor times
            int offset = x * unrollFactor;
            long value = *(long*)(input + offset);
            var xor1 = (xor & 0xff);
            var xor2 = ((xor * xorMultiplier) & 0xff) << 8;
            var xor3 = ((xor * xorMultiplier2) & 0xff) << 16;
            var xor4 = ((xor * xorMultiplier3) & 0xff) << 24;
            var xor5 = ((xor * xorMultiplier4) & 0xff) << 32;
            var xor6 = ((xor * xorMultiplier5) & 0xff) << 40;
            var xor7 = ((xor * xorMultiplier6) & 0xff) << 48;
            var xor8 = ((xor * xorMultiplier7) & 0xff) << 56;
            
            value ^= (xor1 | xor2 | xor3 | xor4 | xor5 | xor6 | xor7 | xor8);
            *(long*)(input + offset) = value;
            xor *= xorMultiplier8;
        }
        
        // Do remaining looping (factor in for length not divisible by 4
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
            Unsafe.CopyBlock(resultPtr, input, (uint)length);
            DecryptUTFInPlace(resultPtr, length);
        } 
        
        return result;
    }
}
using System.Numerics;
using System.Runtime.CompilerServices;

namespace CriFsV2Lib.Tests.Reference;

/// <summary>
/// Lipsym's vectorised version of encryption function used in P5R.
/// </summary>
public static unsafe class LipsumP5REncrypt
{
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void EncryptVecUnsafe(byte* dataPtr, int dataLength)
    {
        // Files shorter than 0x820 can't be "decrypted".  
        // They aren't "encrypted" to begin with, even if they are marked with ENCRYPT user string
        if (dataLength <= 0x820) 
            return;
        
        for (int i = 0; i < 0x400; i += Vector<byte>.Count)
        {
            var a = Unsafe.Read<Vector<byte>>(&dataPtr[i + 0x20]);
            var b = Unsafe.Read<Vector<byte>>(&dataPtr[i + 0x420]);
            var c = a ^ b;
            Unsafe.Write(&dataPtr[i + 0x20], c);
        }
    }
}
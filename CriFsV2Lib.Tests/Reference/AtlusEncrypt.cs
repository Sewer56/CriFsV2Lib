namespace CriFsV2Lib.Tests.Reference;

/// <summary>
/// Original encryption function used in P5R (PC Version).  
/// Translated by hand back from assembly to C#.
/// It might not be completely 1:1 with original [we can never know] but should compile to same assembly based on SharpLab testing.
/// </summary>
public static class AtlusEncrypt
{
    public static unsafe void EncryptAtlusP5R(byte* dataPtr, int dataLength)
    {
        if ( dataLength > 0x820 )
        {
            var currentDecryptAddr = dataPtr + 0x20;
            var loopsLeft = 256;
            do
            {
                *currentDecryptAddr ^= currentDecryptAddr[0x400];
                currentDecryptAddr[1] ^= currentDecryptAddr[0x401];
                currentDecryptAddr[2] ^= currentDecryptAddr[0x402];
                currentDecryptAddr[3] ^= currentDecryptAddr[0x403];
                currentDecryptAddr += 4;
                --loopsLeft;
            }
            while (loopsLeft > 0);
        }
    }
}
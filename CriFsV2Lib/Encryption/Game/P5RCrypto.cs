using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace CriFsV2Lib.Encryption.Game;

/// <summary>
/// Provides decryption support in 
/// - Persona 5 Royal (PS4 JPN & PC)
/// </summary>
public static unsafe class P5RCrypto
{
    /// <summary>
    /// Offset of encrypted data.
    /// </summary>
    public const int EncryptedDataOffset = 0x20;
    
    /// <summary>
    /// Number of bytes to decrypt.
    /// </summary>
    public const int NumBytesToDecrypt = 0x400;
    
    /*
         In-place encryption mechanism used by Persona 5 Royal.
         Present on PC and PS4 JP version (not used in US PS4).
         
         Basically:
         - data[0x20..0x420] = data[0x20..0x420] ^ data[0x420..0x820]
         
         In human words:
         - XOR the first 0x20-0x420 bytes with 0x420-0x820 bytes.
         
         Credit: Lipsum/Zarroboogs for providing the original reference decryption code. 
    */
    
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void DecryptInPlace(byte* dataPtr, int dataLength)
    {
        // Files shorter than 0x820 can't be "decrypted".  
        // They aren't "encrypted" to begin with, even if they are marked with ENCRYPT user string
        if (dataLength <= 0x820) 
            return;
        
        // Go go AVX, SSE
        dataPtr += EncryptedDataOffset;
        
        // For SSE we will assume we're in 64-bit process, and thus have access to 16 registers.
        // For AVX 16 registers is guaranteed.
        if (Avx2.IsSupported)
            DecryptInPlaceAvx(dataPtr);
        else if (Sse.IsSupported) // guaranteed on x86_64
            DecryptInPlaceSse(dataPtr);
        else // fallback
            DecryptInPlaceLong(dataPtr);
    }
    
    /// <summary>
    /// Do not use this API directly, public for benchmarking only.
    /// Use <see cref="DecryptInPlace"/>, it will run this if supported.
    /// </summary>
    /// <param name="dataPtr">Pointer to start of encrypted data, i.e. start of file + <see cref="EncryptedDataOffset"/>.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void DecryptInPlaceAvx(byte* dataPtr)
    {
        const int AvxRegisterLength = 32;
        
        // AVX 32 bytes, needs 32 operations (1024 / 32)
        // We have 16 registers total, 8 for source, 8 for xor data.
        // So we do 256 bytes per loop, i.e. 4 loops.

        var maxDataPtr = dataPtr + NumBytesToDecrypt;
        do
        {
            var v0 = Avx2.LoadVector256(dataPtr);
            var v1 = Avx2.LoadVector256(dataPtr + AvxRegisterLength);
            var v2 = Avx2.LoadVector256(dataPtr + AvxRegisterLength * 2);
            var v3 = Avx2.LoadVector256(dataPtr + AvxRegisterLength * 3);
        
            var v0Next = Avx2.LoadVector256(dataPtr + NumBytesToDecrypt);
            var v1Next = Avx2.LoadVector256(dataPtr + NumBytesToDecrypt + AvxRegisterLength);
            var v2Next = Avx2.LoadVector256(dataPtr + NumBytesToDecrypt + AvxRegisterLength * 2);
            var v3Next = Avx2.LoadVector256(dataPtr + NumBytesToDecrypt + AvxRegisterLength * 3);

            v0 = Avx2.Xor(v0, v0Next);
            v1 = Avx2.Xor(v1, v1Next);
            v2 = Avx2.Xor(v2, v2Next);
            v3 = Avx2.Xor(v3, v3Next);
        
            Avx2.Store(dataPtr, v0);
            Avx2.Store(dataPtr + AvxRegisterLength, v1);
            Avx2.Store(dataPtr + AvxRegisterLength * 2, v2);
            Avx2.Store(dataPtr + AvxRegisterLength * 3, v3);
            dataPtr += AvxRegisterLength * 4;
        } 
        while (dataPtr < maxDataPtr);
    }
    
    /// <summary>
    /// Do not use this API directly, public for benchmarking only.
    /// Use <see cref="DecryptInPlace"/>, it will run this if supported.
    /// </summary>
    /// <param name="dataPtr">Pointer to start of encrypted data, i.e. start of file + <see cref="EncryptedDataOffset"/>.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void DecryptInPlaceSse(byte* dataPtr)
    {
        const int SseRegisterLength = 16;
        
        // SSE 16 bytes, needs 64 operations (1024 / 16)
        // We have 16 registers total, 8 for source, 8 for xor data.
        // So we do 128 bytes per loop, i.e. 16 loops.

        var maxDataPtr = dataPtr + NumBytesToDecrypt;
        do
        {
            var v0 = Sse2.LoadVector128(dataPtr);
            var v1 = Sse2.LoadVector128(dataPtr + SseRegisterLength);
            var v2 = Sse2.LoadVector128(dataPtr + SseRegisterLength * 2);
            var v3 = Sse2.LoadVector128(dataPtr + SseRegisterLength * 3);
        
            var v0Next = Sse2.LoadVector128(dataPtr + NumBytesToDecrypt);
            var v1Next = Sse2.LoadVector128(dataPtr + NumBytesToDecrypt + SseRegisterLength);
            var v2Next = Sse2.LoadVector128(dataPtr + NumBytesToDecrypt + SseRegisterLength * 2);
            var v3Next = Sse2.LoadVector128(dataPtr + NumBytesToDecrypt + SseRegisterLength * 3);

            v0 = Sse2.Xor(v0, v0Next);
            v1 = Sse2.Xor(v1, v1Next);
            v2 = Sse2.Xor(v2, v2Next);
            v3 = Sse2.Xor(v3, v3Next);
        
            Sse2.Store(dataPtr, v0);
            Sse2.Store(dataPtr + SseRegisterLength, v1);
            Sse2.Store(dataPtr + SseRegisterLength * 2, v2);
            Sse2.Store(dataPtr + SseRegisterLength * 3, v3);
            dataPtr += SseRegisterLength * 4;
        } 
        while (dataPtr < maxDataPtr);
    }
    
    /// <summary>
    /// Do not use this API directly, public for benchmarking only.
    /// Use <see cref="DecryptInPlace"/>, it will run this if supported.
    /// </summary>
    /// <param name="dataPtrByte">Pointer to start of encrypted data, i.e. start of file + <see cref="EncryptedDataOffset"/>.</param>
    /// <remarks>Custom unrolled loop.</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void DecryptInPlaceLong(byte* dataPtrByte)
    {
        const int numLongsToDecrypt = NumBytesToDecrypt / sizeof(long);
        
        // This is written with x86_64 in mind, where we have 7 volatile registers. 
        // Long 8 bytes, needs 128 operations (1024 / 8)
        // Do 8 operations per loop, so 16 loops at runtime.

        var dataPtr = (long*)dataPtrByte;
        var maxDataPtr = dataPtr + numLongsToDecrypt;
        do
        {
            var r0 = *(dataPtr);
            var r1 = *(dataPtr + 1);
            var r2 = *(dataPtr + 2);
            var r3 = *(dataPtr + 3);

            var r0Next = *(dataPtr + numLongsToDecrypt);
            var r1Next = *(dataPtr + numLongsToDecrypt + 1);
            var r2Next = *(dataPtr + numLongsToDecrypt + 2);
            var r3Next = *(dataPtr + numLongsToDecrypt + 3);

            // do not refactor, unoptimal codegen
            r0 = r0 ^ r0Next;
            r1 = r1 ^ r1Next;
            r2 = r2 ^ r2Next;
            r3 = r3 ^ r3Next;

            *(dataPtr) = r0;
            *(dataPtr + 1) = r1;
            *(dataPtr + 2) = r2;
            *(dataPtr + 3) = r3;
            dataPtr += 4;
        } 
        while (dataPtr < maxDataPtr);
    }
}
using CriFsV2Lib.Utilities;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CriFsV2Lib.Compression;

/// <summary>
/// Code for dealing with CRI Layla Compression.
/// </summary>
public static unsafe class CriLayla
{
    /// <summary>
    /// Size of uncompressed data under CRILAYLA.
    /// </summary>
    public const int UncompressedDataSize = 0x100;
    
    /// <summary>
    /// Minimum length of LZ77 copy command. 
    /// </summary>
    public const int MinCopyLength = 3;

    /// <summary>
    /// Checks if a file is compressed by checking the signature at the start of it.
    /// </summary>
    /// <param name="data">Pointer to start of the file data.</param>
    /// <param name="compressedDataSize">Size of compressed data.</param>
    /// <returns>True if compressed, else false.</returns>
    public static bool IsCompressed(byte* data, out int compressedDataSize) 
    {
        var result = *(long*)data == 0x414C59414C495243;
        compressedDataSize = *(int*)(data + 12) + 0x10 + UncompressedDataSize;
        return result; 
    }

    /*
        Explanation of CRILayla
        
        == General Structure ==
        - LZSS based.  (offset & length pairs, can encode raw byte)  
        - At end of file is 0x100 of uncompressed data, this is data that will be start of file.  
        
        == Compression Details == 
            
            Block uses compression flag, if flag is set to 0, next 8 bits are copied to output raw.
        
                1 ?????????
                | 
                compression flag 
            
            Data is decompressed from end, i.e.
                *writePtr = ???
                writePtr--
                
                We write the decompressed data from its end.
                
            Copy Block
            
                1 aaaaaaaaaaaaa bbb
                | |             | variable length 
                | 13 bits of offset
                compression flag 
                
                To get length we read in the following (fibbonaci) order i.e.
                    - 2 bits (max 3)
                    - 3 bits (max 7)
                    - 5 bits (max 31)
                    - 8 bits
                    
                And add the length to existing length variable.
                If the max value is returned, we read next number of bits in fib sequence, up to 8 bits. Then 
                read 8s until max value no longer returned.    
    */
    
    // Note: SkipLocalsInit would be nice but it ruins loop alignment
    
    /// <summary>
    /// Decompresses a compressed file to a byte array.
    /// </summary>
    /// <param name="input">Pointer to start of compressed data (CRILAYLA header).</param>
    /// <returns>Byte array of the file after decompression.</returns>
    public static byte[] DecompressToArray(byte* input)
    {
        // Read sizes from header.
        int uncompSizeOfCompData = *(int*)(input + 8);
        int uncompHeaderOffset = *(int*)(input + 12);

        // Create array for Result
        var result = GC.AllocateUninitializedArray<byte>(uncompSizeOfCompData + UncompressedDataSize);
        fixed (byte* resultPtr = result)
            Decompress(input, resultPtr, uncompSizeOfCompData, uncompHeaderOffset);
        
        return result;
    }

    /// <summary>
    /// Decompresses a compressed file to a pooled byte array.
    /// </summary>
    /// <param name="input">Pointer to start of compressed data (CRILAYLA header).</param>
    /// <returns>Pooled array of the file after decompression, please dispose after you're done with it.</returns>
    public static ArrayRental DecompressToArrayPool(byte* input)
    {
        // Read sizes from header.
        int uncompSizeOfCompData = *(int*)(input + 8);
        int uncompHeaderOffset = *(int*)(input + 12);

        // Create array for Result
        var result = new ArrayRental(uncompSizeOfCompData + UncompressedDataSize);
        fixed (byte* resultPtr = result.RawArray)
            Decompress(input, resultPtr, uncompSizeOfCompData, uncompHeaderOffset);
        
        return result;
    }

    private static void Decompress(byte* input, byte* output, int uncompSizeOfCompData, int uncompHeaderOffset)
    {
        // Copy uncompressed 0x100 header (after compressed data) to start of file
        byte* uncompressedDataPtr = input + uncompHeaderOffset + 0x10;
        Unsafe.CopyBlockUnaligned(output, uncompressedDataPtr, UncompressedDataSize);

        // Pointer to which we're copying data to.
        byte* writePtr = output + UncompressedDataSize + uncompSizeOfCompData - 1;
        byte* minAddr = output + UncompressedDataSize;

        // Bitstream State
        byte* compressedDataPtr = uncompressedDataPtr;
        int bitsTillNextByte = 0; // Bits left in the bitstream.

        while (writePtr >= minAddr) // Check if we're done writing from end
        {
            // Check for 1 bit compression flag.
            if (GetNextBit(ref compressedDataPtr, ref bitsTillNextByte) > 0)
            {
                int offset = Read13(ref compressedDataPtr, ref bitsTillNextByte) + MinCopyLength;
                int length = MinCopyLength;

                // Read variable fibonnaci length (unrolled).
                int thisLevel = Read2(ref compressedDataPtr, ref bitsTillNextByte);
                length += thisLevel;

                if (thisLevel == ((1 << 2) - 1))
                {
                    thisLevel = ReadMax8(ref compressedDataPtr, ref bitsTillNextByte, 3);
                    length += thisLevel;

                    if (thisLevel == ((1 << 3) - 1))
                    {
                        thisLevel = ReadMax8(ref compressedDataPtr, ref bitsTillNextByte, 5);
                        length += thisLevel;

                        if (thisLevel == ((1 << 5) - 1))
                        {
                            do
                            {
                                thisLevel = Read8(ref compressedDataPtr, ref bitsTillNextByte);
                                length += thisLevel;
                            }
                            while (thisLevel == byte.MaxValue); // 0b11111111
                        }
                    }
                }

                // LZ77 Copy Below.

                // The optimal way to write this loop depends on average length of copy, 
                // and average length of copy depends on the data we're dealing with.  

                // As such, this would vary per files.
                // For text, length tends to be around 6 on average, for models around 9. 
                // In this implementation we'll put bias towards short copies where length < 10.

                // Note: Min length is 3 (also seems to be most common length), so we can keep that out of the
                // loop and make best use of pipelining.

                *writePtr = writePtr[offset];
                *(writePtr - 1) = (writePtr - 1)[offset];
                *(writePtr - 2) = (writePtr - 2)[offset];

                const int defaultPipelineLength = 3; // Pipeline as in 'CPU Pipelining'
                const int extraPipelineLength = 8;

                if (length < extraPipelineLength)
                {
                    writePtr -= defaultPipelineLength;
                    if (length == defaultPipelineLength)
                        continue;

                    var numLeft = length - defaultPipelineLength;
                    for (int x = 0; x < numLeft; x++)
                    {
                        *writePtr = writePtr[offset];
                        writePtr--;
                    }
                }
                else
                {
                    *(writePtr - 3) = (writePtr - 3)[offset];
                    *(writePtr - 4) = (writePtr - 4)[offset];
                    *(writePtr - 5) = (writePtr - 5)[offset];
                    *(writePtr - 6) = (writePtr - 6)[offset];
                    *(writePtr - 7) = (writePtr - 7)[offset];
                    writePtr -= extraPipelineLength;
                    var numLeft = length - extraPipelineLength;
                    for (int i = 0; i < numLeft; i++)
                    {
                        *writePtr = writePtr[offset];
                        writePtr--;
                    }
                }
            }
            else
            {
                // verbatim byte
                *writePtr = Read8(ref compressedDataPtr, ref bitsTillNextByte);
                writePtr--;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte GetNextBit(ref byte* compressedDataPtr, ref int bitsLeft)
    {
        // Reads a single bit.
        byte currentByte;

        // Note: having if/else rather than single if generated faster runtime despite larger codegen.
        if (bitsLeft != 0)
        {
            currentByte = *(compressedDataPtr);
            bitsLeft--;
        }
        else
        {
            compressedDataPtr--;
            currentByte = *(compressedDataPtr);
            bitsLeft = 7;
        }

        return (byte)((currentByte >> bitsLeft) & 1);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Read13(ref byte* compressedDataPtr, ref int bitsLeft)
    {
        // Reads bits, and advances stream backwards.
        int bitCount = 13;
        byte currentByte;
        
        // Read first set.
        if (bitsLeft != 0)
        {
            currentByte = *(compressedDataPtr);
        }
        else
        {
            compressedDataPtr--;
            currentByte = *(compressedDataPtr);
            bitsLeft = 8;
        }
        
        int bitsThisRound = bitsLeft > bitCount ? bitCount : bitsLeft;
        int result = (currentByte >> (bitsLeft - bitsThisRound)) & ((1 << bitsThisRound) - 1);
        bitCount -= bitsThisRound;
        
        // bitsleft == 0 is guaranteed, so we reset to 8
        bitsLeft = 8;
        compressedDataPtr--;
        currentByte = *compressedDataPtr;
        
        // Read more from next byte.
        bitsThisRound = bitsLeft > bitCount ? bitCount : bitsLeft;
        result <<= bitsThisRound;
        result |= (currentByte >> (bitsLeft - bitsThisRound)) & ((1 << bitsThisRound) - 1);
        
        bitCount -= bitsThisRound;
        
        // It's possible, we might need 3 reads in some cases so we keep unrolling
        if (bitCount <= 0)
        {
            bitsLeft -= bitsThisRound;
            return result;
        }
        
        // Read byte if needed
        bitsLeft = 8;
        compressedDataPtr--;
        currentByte = *(compressedDataPtr);
        
        // If there are more to read from next byte.
        result <<= bitCount;
        result |= (currentByte >> (bitsLeft - bitCount)) & ((1 << bitCount) - 1);
        bitsLeft -= bitCount;
        return result;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte Read8(ref byte* compressedDataPtr, ref int bitsLeft)
    {
        compressedDataPtr--;
        if (bitsLeft != 0)
        {
            // We must split between 2 reads.
            int extraBitCount = 8 - bitsLeft;
            int high = *(compressedDataPtr + 1) & ((1 << bitsLeft) - 1);
            var low = (*(compressedDataPtr) >> (8 - extraBitCount)) & ((1 << extraBitCount) - 1);
            
            // If there are more to read from next byte.
            return (byte)(high << extraBitCount | low);
        }

        return *(compressedDataPtr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte Read2(ref byte* compressedDataPtr, ref int bitsLeft)
    {
        const int bitCount = 2;
        
        // Note: Fast/Common Path.
        bool hasNewBits = bitsLeft == 0;
        if (bitsLeft >= bitCount || hasNewBits)
        {
            // Branchless Programming
            bitsLeft += (Unsafe.As<bool, byte>(ref hasNewBits) * 8) - bitCount;
            compressedDataPtr -= (Unsafe.As<bool, byte>(ref hasNewBits));
            
            // We removed from bitsLeft above, so we don't subtract here. This is necessary because branchless.
            return (byte)((*(compressedDataPtr) >> (bitsLeft)) & 0b11);;
        }

        // Only possible scenario is if bitsLeft == 1
        // bitsLeft == 0 & bitsLeft == 2 will take fast path.
        int high = *(compressedDataPtr) & 1;
        int low =  (*(compressedDataPtr - 1) >> 7) & 1; // derived from ReadMax8's, simplified
        bitsLeft = 7; // Guaranteed
        compressedDataPtr--;
        return (byte)(high << 1 | low);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte ReadMax8(ref byte* compressedDataPtr, ref int bitsLeft, int bitCount)
    {
        // Reads bits, and advances stream backwards.
        byte currentByte;
        
        // Note: having if/else rather than single if generated faster runtime despite larger codegen.
        if (bitsLeft != 0)
        {
            currentByte = *(compressedDataPtr);
        }
        else
        {
            compressedDataPtr--;
            currentByte = *(compressedDataPtr);
            bitsLeft = 8;
        }
        
        int bitsThisRound = bitsLeft > bitCount ? bitCount : bitsLeft;
        int result = (currentByte >> (bitsLeft - bitsThisRound)) & ((1 << bitsThisRound) - 1);
        
        bitCount -= bitsThisRound;
        bitsLeft -= bitsThisRound;
        if (bitCount <= 0)
            return (byte)result;
        
        // This path can only be followed if bitsleft == 0
        compressedDataPtr--;
        currentByte = *(compressedDataPtr);
        bitsLeft = 8;
        
        // If there are more to read from next byte.
        result <<= bitCount;
        result |= (currentByte >> (bitsLeft - bitCount)) & ((1 << bitCount) - 1);
        bitsLeft -= bitCount;
        return (byte)result;
    }
}
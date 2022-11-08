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
    
    /*
        Explanation of CRILayla
        [based on the code I optimised]
        
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
                    
                
    */
    
    public static byte[] Decompress(byte* input)
    {
        // Read sizes from header.
        int uncompSizeOfCompData = *(int*)(input + 8);
        int uncompHeaderOffset = *(int*)(input + 12);

        // Create array for Result
        byte[] result = GC.AllocateUninitializedArray<byte>(uncompSizeOfCompData + UncompressedDataSize);
        fixed (byte* resultPtr = result)
        {
            // Copy uncompressed 0x100 header (after compressed data) to start of file
            byte* uncompressedDataPtr = input + uncompHeaderOffset + 0x10;
            Unsafe.CopyBlockUnaligned(resultPtr, uncompressedDataPtr, UncompressedDataSize);
            
            // VLE as in 'Variable Length'
            Span<byte> vleLengths = stackalloc byte[] { 2, 3, 5, 8 };
            
            // Pointer to which we're copying data to.
            byte* writePtr = resultPtr + UncompressedDataSize + uncompSizeOfCompData - 1;
            byte* minAddr = resultPtr + UncompressedDataSize;
            
            // Bitstream State
            byte* compressedDataPtr = uncompressedDataPtr - 1;
            int bitsTillNextByte = 0; // Bits left in the bitstream.
            
            while (writePtr >= minAddr) // Check if we're done writing from end
            {
                // Check for 1 bit compression flag.
                if (GetNextBit(ref compressedDataPtr, ref bitsTillNextByte) > 0)
                {
                    int offset = GetNextBits(ref compressedDataPtr, ref bitsTillNextByte, 13) + MinCopyLength;
                    int length = MinCopyLength;
                    int vleLevel;

                    // Read variable length.
                    for (vleLevel = 0; vleLevel < vleLengths.Length; vleLevel++)
                    {
                        int thisLevel = GetNextBits(ref compressedDataPtr, ref bitsTillNextByte, vleLengths[vleLevel]);
                        length += thisLevel;
                        
                        // (1 << vleLengths[vleLevel]) - 1) is max possible value for this level.
                        // If we didn't add max value, read some more!
                        if (thisLevel != ((1 << vleLengths[vleLevel]) - 1)) 
                            break;
                    }

                    // If copy length is larger than available lengths then keep reading length until not fully taken (byte.MaxValue)
                    if (vleLevel == vleLengths.Length)
                    {
                        int this_level;
                        do
                        {
                            this_level = GetNextBits(ref compressedDataPtr, ref bitsTillNextByte, 8);
                            length += this_level;
                        } 
                        while (this_level == byte.MaxValue); // 0b11111111
                    }

                    // lz77 copy, todo: this could be optimised.
                    for (int i = 0; i < length; i++)
                    {
                        *writePtr = writePtr[offset];
                        writePtr--;
                    }
                }
                else
                {
                    // verbatim byte
                    *writePtr = (byte)GetNextBits(ref compressedDataPtr, ref bitsTillNextByte, 8);
                    writePtr--;
                }
            }

            return result;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte GetNextBit(ref byte* compressedDataPtr, ref int bitsLeft)
    {
        // Reads a single bit.
        byte currentByte;

        if (bitsLeft != 0)
        {
            currentByte = *(compressedDataPtr + 1);
        }
        else
        {
            currentByte = *compressedDataPtr;
            bitsLeft = 8;
            compressedDataPtr--;
        }

        bitsLeft--;
        return (byte)((currentByte >> bitsLeft) & 1);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort GetNextBits(ref byte* compressedDataPtr, ref int bitsLeft, int bitCount)
    {
        // Reads bits, and advances stream backwards.
        ushort outBits = 0;
        int bitsThisRound;
        byte currentByte = *(compressedDataPtr + 1);
        
        do
        {
            if (bitsLeft == 0)
            {
                currentByte = *compressedDataPtr;
                bitsLeft = 8;
                compressedDataPtr--;
            }
            
            bitsThisRound = bitsLeft > bitCount ? bitCount : bitsLeft;
            outBits <<= bitsThisRound;
            outBits |= (ushort)((ushort)(currentByte >> (bitsLeft - bitsThisRound)) & ((1 << bitsThisRound) - 1));

            bitCount -= bitsThisRound;
            bitsLeft -= bitsThisRound;
        }
        while (bitCount > 0);

        return outBits;
    }
}
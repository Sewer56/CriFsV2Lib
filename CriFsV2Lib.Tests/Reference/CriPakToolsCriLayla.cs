using System.Diagnostics;

namespace CriFsV2Lib.Tests.Reference;

public static class CriPakToolsCriLayla
{
    public static byte[] DecompressLegacyCRI(byte[] input, int USize)
    {
        byte[] result; // = new byte[USize];

        MemoryStream ms = new MemoryStream(input);
        EndianReader br = new EndianReader(ms, true);

        br.BaseStream.Seek(8, SeekOrigin.Begin); // Skip CRILAYLA
        int uncompressed_size = br.ReadInt32();
        int uncompressed_header_offset = br.ReadInt32();

        result = new byte[uncompressed_size + 0x100];

        // do some error checks here.........

        // copy uncompressed 0x100 header to start of file
        Array.Copy(input, uncompressed_header_offset + 0x10, result, 0, 0x100);

        int input_end = input.Length - 0x100 - 1;
        int input_offset = input_end;
        int output_end = 0x100 + uncompressed_size - 1;
        byte bit_pool = 0;
        int bits_left = 0, bytes_output = 0;
        int[] vle_lens = new int[4] { 2, 3, 5, 8 };

        while (bytes_output < uncompressed_size)
        {
            if (get_next_bits(input, ref input_offset, ref bit_pool, ref bits_left, 1) > 0)
            {
                int backreference_offset = output_end - bytes_output +
                                           get_next_bits(input, ref input_offset, ref bit_pool, ref bits_left, 13) +
                                           3;
                int backreference_length = 3;
                int vle_level;

                for (vle_level = 0; vle_level < vle_lens.Length; vle_level++)
                {
                    int this_level = get_next_bits(input, ref input_offset, ref bit_pool, ref bits_left,
                        vle_lens[vle_level]);
                    backreference_length += this_level;
                    if (this_level != ((1 << vle_lens[vle_level]) - 1)) break;
                }

                if (vle_level == vle_lens.Length)
                {
                    int this_level;
                    do
                    {
                        this_level = get_next_bits(input, ref input_offset, ref bit_pool, ref bits_left, 8);
                        backreference_length += this_level;
                    } while (this_level == 255);
                }

                for (int i = 0; i < backreference_length; i++)
                {
                    result[output_end - bytes_output] = result[backreference_offset--];
                    bytes_output++;
                }
            }
            else
            {
                // verbatim byte
                result[output_end - bytes_output] = (byte)get_next_bits(input, ref input_offset, ref bit_pool, ref bits_left, 8);
                bytes_output++;
            }
        }

        br.Close();
        ms.Close();

        return result;
    }
    
    private static ushort get_next_bits(byte[] input, ref int offset_p, ref byte bit_pool_p, ref int bits_left_p, int bit_count)
    {
        ushort out_bits = 0;
        int num_bits_produced = 0;
        int bits_this_round;

        while (num_bits_produced < bit_count)
        {
            if (bits_left_p == 0)
            {
                bit_pool_p = input[offset_p];
                bits_left_p = 8;
                offset_p--;
            }

            if (bits_left_p > (bit_count - num_bits_produced))
                bits_this_round = bit_count - num_bits_produced;
            else
                bits_this_round = bits_left_p;

            out_bits <<= bits_this_round;

            out_bits |= (ushort)((ushort)(bit_pool_p >> (bits_left_p - bits_this_round)) & ((1 << bits_this_round) - 1));

            bits_left_p -= bits_this_round;
            num_bits_produced += bits_this_round;
        }

        return out_bits;
    }
}
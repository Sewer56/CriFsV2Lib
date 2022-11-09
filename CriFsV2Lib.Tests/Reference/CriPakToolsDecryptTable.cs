namespace CriFsV2Lib.Tests.Reference;

public class CriPakToolsDecryptTable
{
    public static byte[] DecryptUTF(byte[] input)
    {
        byte[] result = new byte[input.Length];

        int m, t;
        byte d;

        m = 0x0000655f;
        t = 0x00004115;

        for (int i = 0; i < input.Length; i++)
        {
            d = input[i];
            d = (byte)(d ^ (byte)(m & 0xff));
            result[i] = d;
            m *= t;
        }

        return result;
    }
}
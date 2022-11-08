using CriFsV2Lib.Tests.Reference;
using FileEmulationFramework.Tests;

namespace CriFsV2Lib.Tests;

public unsafe class CompressionTests
{
    [Fact]
    public void DecompressesCorrectly()
    {
        var data = File.ReadAllBytes(Assets.SampleCompressedTextFile);
        var unmodifiedData = File.ReadAllBytes(Assets.SampleUncompressedFile);
        fixed (byte* dataPtr = data)
        {
            var decompressedOld = CriPakToolsCriLayla.DecompressLegacyCRI(data, data.Length);
            var decompressed = Compression.CriLayla.Decompress(dataPtr);
            Assert.Equal(unmodifiedData, decompressedOld);
            Assert.Equal(unmodifiedData, decompressed);
        }
    }
}
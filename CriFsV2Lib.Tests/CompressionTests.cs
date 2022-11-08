using CriFsV2Lib.Tests.Reference;
using FileEmulationFramework.Tests;

namespace CriFsV2Lib.Tests;

public unsafe class CompressionTests
{
    [Fact]
    public void Text_DecompressesCorrectly()
    {
        var data = File.ReadAllBytes(Assets.SampleCompressedTextFile);
        var unmodifiedData = File.ReadAllBytes(Assets.SampleUncompressedTextFile);
        fixed (byte* dataPtr = data)
        {
            var decompressedOld = CriPakToolsCriLayla.DecompressLegacyCRI(data, data.Length);
            var decompressed = Compression.CriLayla.Decompress(dataPtr);
            Assert.Equal(unmodifiedData, decompressedOld);
            Assert.Equal(unmodifiedData, decompressed);
        }
    }
    
    [Fact]
    public void Model_DecompressesCorrectly()
    {
        var data = File.ReadAllBytes(Assets.SampleCompressedModelFile);
        var unmodifiedData = File.ReadAllBytes(Assets.SampleUncompressedModelFile);
        fixed (byte* dataPtr = data)
        {
            var decompressed = Compression.CriLayla.Decompress(dataPtr);
            Assert.Equal(unmodifiedData, decompressed);
        }
    }
}
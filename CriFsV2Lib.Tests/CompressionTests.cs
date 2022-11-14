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
            var decompressed = Compression.CriLayla.DecompressToArray(dataPtr);
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
            var decompressed = Compression.CriLayla.DecompressToArray(dataPtr);
            Assert.Equal(unmodifiedData, decompressed);
        }
    }
    
    [Fact]
    public void IsCompressed()
    {
        var data = File.ReadAllBytes(Assets.SampleCompressedModelFile);
        var unmodifiedData = File.ReadAllBytes(Assets.SampleUncompressedModelFile);
        fixed (byte* unmodifiedPtr = unmodifiedData)
        fixed (byte* dataPtr = data)
        {
            Assert.True(Compression.CriLayla.IsCompressed(dataPtr, out _));
            Assert.False(Compression.CriLayla.IsCompressed(unmodifiedPtr, out _));
        }
    }
}
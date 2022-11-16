using FileEmulationFramework.Tests;

namespace CriFsV2Lib.Tests;

public class HighLevelApiTests
{
    [Fact]
    public void CpkReader_GetFiles()
    {
        // Arrange
        using var fileStream = new FileStream(Assets.SampleCpkFile, FileMode.Open);
        using var reader = CriFsLib.Instance.CreateCpkReader(fileStream, true);
        CpkHelperTests.AssertSampleCpkFile(reader.GetFiles());
    }
    
    [Fact]
    public void CpkReader_Extract()
    {
        // Arrange
        var originalFile = File.ReadAllBytes(Assets.SampleUncompressedTextFile);
        
        using var fileStream = new FileStream(Assets.SampleCpkFile, FileMode.Open);
        using var reader = CriFsLib.Instance.CreateCpkReader(fileStream, true);
        using var extractedFile = reader.ExtractFile(reader.GetFiles()[2]);
        
        Assert.Equal(originalFile, extractedFile.Span.ToArray());
    }
    
    [Fact]
    public void CpkReader_Extract_Uncompressed()
    {
        // Arrange
        var originalFile = File.ReadAllBytes(Assets.SampleUncompressedImageFile);
        
        using var fileStream = new FileStream(Assets.SampleCpkFile, FileMode.Open);
        using var reader = CriFsLib.Instance.CreateCpkReader(fileStream, true);
        using var extractedFile = reader.ExtractFileNoDecompression(reader.GetFiles()[1], out _);
        
        Assert.Equal(originalFile, extractedFile.Span.ToArray());
    }
}
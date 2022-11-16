using CriFsV2Lib.Definitions.Structs;
using CriFsV2Lib.Utilities.Parsing;
using FileEmulationFramework.Tests;

namespace CriFsV2Lib.Tests;

/// <summary>
/// Tests for the library's high level API.
/// </summary>
public class CpkHelperTests
{
    [Fact]
    public unsafe void GetFileInfo_FromStream()
    {
        // Arrange
        using var fileStream = new FileStream(Assets.SampleCpkFile, FileMode.Open);
        var files = CpkHelper.GetFilesFromStream(fileStream);
        
        AssertSampleCpkFile(files);
    }

    [Fact]
    public unsafe void GetFileInfo_FromFile()
    {
        // Arrange
        var cpkData = File.ReadAllBytes(Assets.SampleCpkFile);
        fixed (byte* cpkDataPtr = &cpkData[0])
        {
            var files = CpkHelper.GetFilesFromFile(cpkDataPtr);
            AssertSampleCpkFile(files);
        }
    }
    
    [Fact]
    public unsafe void ExtractFile_Compressed()
    {
        // Arrange
        var originalFile = File.ReadAllBytes(Assets.SampleUncompressedTextFile);
        
        using var fileStream = new FileStream(Assets.SampleCpkFile, FileMode.Open);
        var files = CpkHelper.GetFilesFromStream(fileStream);
        using var extractedFile = CpkHelper.ExtractFile(files[2], fileStream);
        
        Assert.Equal(originalFile, extractedFile.Span.ToArray());
    }
    
    [Fact]
    public unsafe void ExtractFile_Uncompressed()
    {
        // Arrange
        var originalFile = File.ReadAllBytes(Assets.SampleUncompressedImageFile);
        
        using var fileStream = new FileStream(Assets.SampleCpkFile, FileMode.Open);
        var files = CpkHelper.GetFilesFromStream(fileStream);
        using var extractedFile = CpkHelper.ExtractFile(files[1], fileStream);
        
        Assert.Equal(originalFile, extractedFile.Span.ToArray());
    }
    
    internal static void AssertSampleCpkFile(CpkFile[] files)
    {
        Assert.Equal(3, files.Length);

        Assert.Equal("Audio-NoCompression.flac", files[0].FileName);
        Assert.Equal(48431, files[0].FileSize);
        Assert.Equal(48431, files[0].ExtractSize);
        Assert.Equal(2048, files[0].FileOffset);

        Assert.Equal("Text-Compressed.txt", files[2].FileName);
        Assert.Equal(2484, files[2].FileSize);
        Assert.Equal(3592, files[2].ExtractSize);
        Assert.Equal(171520, files[2].FileOffset);
    }
}
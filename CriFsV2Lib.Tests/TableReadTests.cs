using CriFsV2Lib.Structs;
using FileEmulationFramework.Tests;
using File = System.IO.File;

namespace CriFsV2Lib.Tests;

/// <summary>
/// Tests related to reading tables.
/// </summary>
public class TableReadTests
{
    [Fact]
    public void Table_CanRead_WhenEncrypted()
    {
        // Arrange
        var encryptedCpkStream = new FileStream(Assets.SampleCpkEncryptedFile, FileMode.Open);
        
        // Act
        var actual = TableContainerReader.ReadTable(encryptedCpkStream);
        encryptedCpkStream.Dispose();
        
        // Assert
        var expected = File.ReadAllBytes(Assets.SampleDecryptedTable);
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void Table_CanRead_WhenDecrypted()
    {
        // Arrange
        var encryptedCpkStream = new FileStream(Assets.SampleCpkFile, FileMode.Open);
        
        // Act
        var actual = TableContainerReader.ReadTable(encryptedCpkStream);
        encryptedCpkStream.Dispose();
        
        // Assert
        var expected = File.ReadAllBytes(Assets.SampleDecryptedTable);
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public unsafe void Table_CanReadMetadata()
    {
        // Arrange
        var tableData = File.ReadAllBytes(Assets.SampleDecryptedTable);
        fixed (byte* tableDataPtr = &tableData[0])
        {
            var metadata = new CriTableMetadata(tableDataPtr);
            
            // Assert
            Assert.Equal(252, metadata.RowsOffset);
            Assert.Equal(382, metadata.StringPoolOffset);
            Assert.Equal(856, metadata.DataPoolOffset);
            Assert.Equal(44, metadata.ColumnCount);
            Assert.Equal(130, metadata.RowSizeBytes);
            Assert.Equal(1, metadata.RowCount);
        }
    }
    
    [Fact]
    public unsafe void TocFinder_CanFindToc()
    {
        // Arrange
        var tableData = File.ReadAllBytes(Assets.SampleDecryptedTable);
        fixed (byte* tableDataPtr = &tableData[0])
        {
            Assert.True(TocFinder.FindToc(tableDataPtr, out var tocOffset, out var contentOffset));
            Assert.Equal(174080, tocOffset);
            Assert.Equal(2048, contentOffset);
        }
    }
    
    [Fact]
    public unsafe void TocReader_CanFindFiles()
    {
        // Arrange
        var tableData = File.ReadAllBytes(Assets.SampleCpkFile);
        fixed (byte* tableDataPtr = &tableData[TableContainerReader.TableContainerSize])
        {
            Assert.True(TocFinder.FindToc(tableDataPtr, out var tocOffset, out var contentOffset));

            var tocPtr = tableDataPtr + tocOffset;
            var files = TocReader.ReadToc(tocPtr, contentOffset);
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
}
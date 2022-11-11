using FileEmulationFramework.Tests;

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
}
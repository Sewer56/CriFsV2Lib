using CriFsV2Lib.Encryption;
using CriFsV2Lib.Tests.Reference;
using FileEmulationFramework.Tests;

namespace CriFsV2Lib.Tests;

/// <summary>
/// Tests for the library's encryption functionality.
/// </summary>
public unsafe class EncryptionTests
{
    [Fact]
    public void CanDecryptTable()
    {
        var data = File.ReadAllBytes(Assets.SampleEncryptedTable);
        var unmodifiedData = File.ReadAllBytes(Assets.SampleDecryptedTable);
        fixed (byte* dataPtr = data)
        {
            var decryptedOld = CriPakToolsDecryptTable.DecryptUTF(data);
            var decrypted = TableDecryptor.DecryptUTF(dataPtr, data.Length);
            Assert.Equal(unmodifiedData, decryptedOld);
            Assert.Equal(unmodifiedData, decrypted);
        }
    }
}
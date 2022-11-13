using CriFsV2Lib.Encryption.Game;
using CriFsV2Lib.Tests.Reference;

namespace CriFsV2Lib.Tests.Game;

public unsafe class P5R
{
    [Fact]
    public void CanDecrypt_WorksBothWays()
    {
        // Note: This is XOR Based algorithm, apply twice and it should return original data.
        var data = new byte[0x821];
        for (int x = 0; x < data.Length; x++)
            data[x] = (byte)x;

        var encrypted = data.ToArray();
        fixed (byte* encryptedPtr = &encrypted[0])
        {
            // Encrypt
            P5RCrypto.DecryptInPlace(encryptedPtr, encrypted.Length);
            Assert.NotEqual(data, encrypted);
            
            // Decrypt
            P5RCrypto.DecryptInPlace(encryptedPtr, encrypted.Length);
            Assert.Equal(data, encrypted);
        }
    }
    
    [Fact]
    public void CanDecrypt_WhenMinLengthExceeded()
    {
        // Note: This is XOR Based algorithm, apply twice and it should return original data.
        var data = new byte[0x821];
        for (int x = 0; x < data.Length; x++)
            data[x] = (byte)x;
        
        var encrypted = data.ToArray();
        fixed (byte* encryptedPtr = &encrypted[0])
        {
            // If we encrypt and decrypt with two different implementations, it should still equal original, quick way to validate is okay.
            P5RCrypto.DecryptInPlace(encryptedPtr, encrypted.Length);
            AtlusEncrypt.EncryptAtlusP5R(encryptedPtr, encrypted.Length);
            Assert.Equal(data, encrypted);
        }
    }
    
    [Fact]
    public void CanDecrypt_WhenUnderMinLength()
    {
        // Note: This is XOR Based algorithm, apply twice and it should return original data.
        var data = new byte[0x819];
        for (int x = 0; x < data.Length; x++)
            data[x] = (byte)x;
        
        var encrypted = data.ToArray();
        fixed (byte* encryptedPtr = &encrypted[0])
        {
            P5RCrypto.DecryptInPlace(encryptedPtr, encrypted.Length);
            Assert.Equal(data, encrypted); // no-op, under min length
        }
    }
    
    [Fact]
    public void CanDecrypt_AllImplementationsMatch()
    {
        // Note: This is XOR Based algorithm, apply twice and it should return original data.
        var data = new byte[0x821];
        for (int x = 0; x < data.Length; x++)
            data[x] = (byte)x;
        
        var encryptedAvx = data.ToArray();
        var encryptedSse = data.ToArray();
        var encryptedLong = data.ToArray();
        
        fixed (byte* encryptedSsePtr = &encryptedSse[0])
        fixed (byte* encryptedLongPtr = &encryptedLong[0])
        fixed (byte* encryptedAvxPtr = &encryptedAvx[0])
        {
            // If we encrypt and decrypt with two different implementations, it should still equal original, quick way to validate is okay.
            P5RCrypto.DecryptInPlaceAvx(encryptedAvxPtr + P5RCrypto.EncryptedDataOffset);
            P5RCrypto.DecryptInPlaceSse(encryptedSsePtr + P5RCrypto.EncryptedDataOffset);
            P5RCrypto.DecryptInPlaceLong(encryptedLongPtr + P5RCrypto.EncryptedDataOffset);
            Assert.Equal(encryptedAvx, encryptedSse);
            Assert.Equal(encryptedAvx, encryptedLong);
        }
    }
}